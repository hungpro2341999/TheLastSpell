using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Brazier;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheLastStand.Database.Building;

public class BuildingDatabase : Database<BuildingDatabase>
{
	[SerializeField]
	private TextAsset buildingGaugeEffectDefinitions;

	[SerializeField]
	private TextAsset buildingPassiveDefinitions;

	[SerializeField]
	private TextAsset buildingActionDefinitions;

	[SerializeField]
	private TextAsset buildingUpgradeDefinitions;

	[SerializeField]
	private TextAsset buildingSkillSoundDefinitions;

	[SerializeField]
	private IEnumerable<TextAsset> individualBuildingDefinitions;

	[SerializeField]
	private IEnumerable<TextAsset> groupBuildingDefinitions;

	[SerializeField]
	private TextAsset shopDefinition;

	[SerializeField]
	private TextAsset brazierDefinition;

	[SerializeField]
	private TextAsset randomBuildingsGenerationsDefinitions;

	[SerializeField]
	private TextAsset randomBuildingsDirectionsDefinitions;

	[SerializeField]
	private TextAsset randomBuildingsPerDayDefinitions;

	[SerializeField]
	private DataColor validColor;

	[SerializeField]
	private DataColor invalidColor;

	[SerializeField]
	private TileBySpriteDictionary tileBySpriteDictionary;

	public static ShopDefinition ShopDefinition;

	public static BraziersDefinition BraziersDefinition;

	public static Dictionary<string, BuildingActionDefinition> BuildingActionDefinitions { get; private set; }

	public static Dictionary<string, BuildingDefinition> BuildingDefinitions { get; private set; }

	public static Dictionary<string, BuildingLimitGroupDefinition> BuildingLimitGroupDefinitions { get; private set; }

	public static Dictionary<string, BuildingGaugeEffectDefinition> BuildingGaugeEffectDefinitions { get; private set; }

	public static Dictionary<string, BuildingPassiveDefinition> BuildingPassiveDefinitions { get; private set; }

	public static Dictionary<string, BuildingUpgradeDefinition> BuildingUpgradeDefinitions { get; private set; }

	public static Dictionary<string, BuildingSkillSoundDefinition> BuildingSkillSoundDefinitions { get; private set; }

	public static Dictionary<string, RandomBuildingsGenerationDefinition> RandomBuildingsGenerationDefinitions { get; private set; }

	public static Dictionary<string, RandomBuildingsDirectionsDefinition> RandomBuildingsDirectionsDefinitions { get; private set; }

	public static Dictionary<string, RandomBuildingsPerDayDefinition> RandomBuildingsPerDayDefinitions { get; private set; }

	public static Color InvalidColor => TPSingleton<BuildingDatabase>.Instance.invalidColor._Color;

	public static Dictionary<ItemDefinition.E_Category, HashSet<string>> ShopItemsByCategory { get; private set; }

	public static TileBySpriteDictionary TileBySpriteDictionary => TPSingleton<BuildingDatabase>.Instance.tileBySpriteDictionary;

	public static Color ValidColor => TPSingleton<BuildingDatabase>.Instance.validColor._Color;

	public override void Deserialize(XContainer container = null)
	{
		DeserializeShop();
		DeserializeGaugeEffects();
		DeserializePassive();
		DeserializeActions();
		DeserializeUpgrades();
		DeserializeBuildingDefinitions();
		DeserializeSkillSoundDefinitions();
		ComputeShopItemsByCategory();
		DeserializeBraziers();
		DeserializeRandomBuildingsGenerations();
		DeserializeRandomBuildingsDirections();
		DeserializeRandomBuildingsPerDay();
	}

	private void DeserializeBraziers()
	{
		if (BraziersDefinition == null)
		{
			XElement xElement = XDocument.Parse(brazierDefinition.text, LoadOptions.SetBaseUri).Element("BraziersDefinition");
			if (xElement == null)
			{
				CLoggerManager.Log("BraziersDefinition document must have a BraziersDefinition Element", LogType.Error);
			}
			else
			{
				BraziersDefinition = new BraziersDefinition(xElement);
			}
		}
	}

	public TileBase LoadTileFromResourcesOnce(string path)
	{
		return ResourcePooler<TileBase>.LoadOnce(path);
	}

	private void ComputeShopItemsByCategory()
	{
		if (!BuildingDefinitions.TryGetValue("Shop", out var value))
		{
			CLoggerManager.Log("Shop definition is missing !", LogType.Error, CLogLevel.MAJOR);
			return;
		}
		GenerateNewItemsRosterDefinition generateNewItemsRosterDefinition = null;
		foreach (BuildingPassiveDefinition buildingPassiveDefinition in value.PassivesModuleDefinition.BuildingPassiveDefinitions)
		{
			generateNewItemsRosterDefinition = buildingPassiveDefinition.PassiveEffectDefinitions.FirstOrDefault((BuildingPassiveEffectDefinition e) => e is GenerateNewItemsRosterDefinition) as GenerateNewItemsRosterDefinition;
			if (generateNewItemsRosterDefinition != null)
			{
				break;
			}
		}
		ShopItemsByCategory = new Dictionary<ItemDefinition.E_Category, HashSet<string>>();
		foreach (CreateRosterItemDefinition createItemRosterDefinition in generateNewItemsRosterDefinition.CreateItemRosterDefinitions)
		{
			foreach (KeyValuePair<string, int> item in createItemRosterDefinition.CreateItemDefinition.ItemsListDefinition.ItemsWithOdd)
			{
				if (item.Value != 0)
				{
					AddItemToShopItemsByCategory(item.Key);
				}
			}
		}
	}

	private void AddItemToShopItemsByCategory(string itemId)
	{
		if (!ItemDatabase.ItemDefinitions.TryGetValue(itemId, out var value))
		{
			if (ItemDatabase.ItemsListDefinitions.TryGetValue(itemId, out var value2))
			{
				foreach (KeyValuePair<string, int> item in value2.ItemsWithOdd)
				{
					if (item.Value != 0)
					{
						AddItemToShopItemsByCategory(item.Key);
					}
				}
				return;
			}
			CLoggerManager.Log("item or itemsList definition with id " + itemId + " is missing !", LogType.Error, CLogLevel.MAJOR);
		}
		else
		{
			ItemDefinition.E_Category category = value.Category;
			if (!ShopItemsByCategory.ContainsKey(category))
			{
				ShopItemsByCategory.Add(category, new HashSet<string> { itemId });
			}
			else
			{
				ShopItemsByCategory[category].Add(itemId);
			}
		}
	}

	private void DeserializeActions()
	{
		if (BuildingActionDefinitions != null)
		{
			return;
		}
		BuildingActionDefinitions = new Dictionary<string, BuildingActionDefinition>();
		foreach (XElement item in XDocument.Parse(buildingActionDefinitions.text, LoadOptions.SetBaseUri).Element("BuildingActionDefinitions").Elements("BuildingActionDefinition"))
		{
			BuildingActionDefinition buildingActionDefinition = new BuildingActionDefinition(item);
			BuildingActionDefinitions.Add(buildingActionDefinition.Id, buildingActionDefinition);
		}
	}

	private void DeserializeBuildingDefinitions()
	{
		if (BuildingDefinitions != null)
		{
			return;
		}
		BuildingDefinitions = new Dictionary<string, BuildingDefinition>();
		BuildingLimitGroupDefinitions = new Dictionary<string, BuildingLimitGroupDefinition>();
		string text = typeof(BuildingDefinition).Name;
		string text2 = text + "s";
		ConcurrentQueue<XElement> xDefinitionsList = new ConcurrentQueue<XElement>();
		ConcurrentQueue<XElement> xBuildingLimitGroupDefinitionsList = new ConcurrentQueue<XElement>();
		foreach (TextAsset groupBuildingDefinition in groupBuildingDefinitions)
		{
			XDocument xDocument = XDocument.Parse(groupBuildingDefinition.text, LoadOptions.SetBaseUri);
			xDocument.Element(text2).Elements("BuildingLimitGroupDefinition").All(delegate(XElement o)
			{
				xBuildingLimitGroupDefinitionsList.Enqueue(o);
				return true;
			});
			xDocument.Element(text2).Elements(text).All(delegate(XElement o)
			{
				xDefinitionsList.Enqueue(o);
				return true;
			});
		}
		foreach (TextAsset individualBuildingDefinition in individualBuildingDefinitions)
		{
			try
			{
				XElement item = XDocument.Parse(individualBuildingDefinition.text, LoadOptions.SetBaseUri)?.Element(text);
				xDefinitionsList.Enqueue(item);
			}
			catch (InvalidOperationException)
			{
				CLoggerManager.Log("Invalid  template definition: " + individualBuildingDefinition.name + ". Please check the XML thoroughly. Loading of this building will be skipped.", LogType.Error);
			}
			catch (ArgumentNullException)
			{
				CLoggerManager.Log("Please check the " + GetType().Name + " prefab: a NULL building has been linked as part of it.", LogType.Error);
			}
		}
		XElement result;
		while (xBuildingLimitGroupDefinitionsList.TryDequeue(out result))
		{
			BuildingLimitGroupDefinition buildingLimitGroupDefinition = new BuildingLimitGroupDefinition(result);
			try
			{
				BuildingLimitGroupDefinitions.Add(buildingLimitGroupDefinition.Id, buildingLimitGroupDefinition);
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log("Duplicate BuildingLimitGroupDefinition found for ID " + buildingLimitGroupDefinition.Id + ".", LogType.Warning);
			}
		}
		XElement result2;
		while (xDefinitionsList.TryDequeue(out result2))
		{
			BuildingDefinition buildingDefinition = ((!(result2.Attribute("Id").Value == "MagicCircle")) ? new BuildingDefinition(result2) : new MagicCircleDefinition(result2));
			try
			{
				BuildingDefinitions.Add(buildingDefinition.Id, buildingDefinition);
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log("Duplicate " + text + " found for ID " + buildingDefinition.Id + ": the individual files will have PRIORITY over the all-in-one template file.", LogType.Warning);
			}
		}
	}

	private void DeserializeGaugeEffects()
	{
		if (BuildingGaugeEffectDefinitions != null)
		{
			return;
		}
		BuildingGaugeEffectDefinitions = new Dictionary<string, BuildingGaugeEffectDefinition>();
		XElement xElement = XDocument.Parse(buildingGaugeEffectDefinitions.text, LoadOptions.SetBaseUri).Element("BuildingGaugeEffectDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("Document must have BuildingGaugeEffectDefinitions", LogType.Error);
			return;
		}
		foreach (XElement item in xElement.Elements("BuildingGaugeEffectDefinition"))
		{
			BuildingGaugeEffectDefinition buildingGaugeEffectDefinition = null;
			XAttribute xAttribute = item.Attribute("Id");
			if (xAttribute.IsNullOrEmpty())
			{
				CLoggerManager.Log("BuildingGaugeEffectDefinition must have Id", LogType.Error);
				continue;
			}
			string value = xAttribute.Value;
			buildingGaugeEffectDefinition = value switch
			{
				"CreateItem" => new CreateItemGaugeEffectDefinition(item), 
				"GainGold" => new GainGoldDefinition(item), 
				"GainMaterials" => new GainMaterialsDefinition(item), 
				"OpenMagicSeal" => new OpenMagicSealDefinition(item), 
				"GlobalUpgradeStat" => new UpgradeStatGaugeEffectDefinition(item), 
				_ => null, 
			};
			if (buildingGaugeEffectDefinition == null)
			{
				CLoggerManager.Log("BuildingGaugeEffectDefinition " + value + " not found!", LogType.Error);
			}
			else
			{
				BuildingGaugeEffectDefinitions.Add(value, buildingGaugeEffectDefinition);
			}
		}
	}

	private void DeserializePassive()
	{
		if (BuildingPassiveDefinitions != null)
		{
			return;
		}
		BuildingPassiveDefinitions = new Dictionary<string, BuildingPassiveDefinition>();
		foreach (XElement item in XDocument.Parse(buildingPassiveDefinitions.text, LoadOptions.SetBaseUri).Element("BuildingPassiveDefinitions").Elements("BuildingPassiveDefinition"))
		{
			BuildingPassiveDefinition buildingPassiveDefinition = new BuildingPassiveDefinition(item);
			BuildingPassiveDefinitions.Add(buildingPassiveDefinition.Id, buildingPassiveDefinition);
		}
	}

	private void DeserializeRandomBuildingsGenerations()
	{
		if (RandomBuildingsGenerationDefinitions != null)
		{
			return;
		}
		XElement xElement = XDocument.Parse(randomBuildingsGenerationsDefinitions.text, LoadOptions.SetBaseUri).Element("RandomBuildingsGenerationsDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("randomBuildingsGenerationsDefinitions document must have a RandomBuildingsGenerationsDefinitions Element", LogType.Error);
			return;
		}
		RandomBuildingsGenerationDefinitions = new Dictionary<string, RandomBuildingsGenerationDefinition>();
		foreach (XElement item in xElement.Elements("RandomBuildingsGenerationDefinition"))
		{
			RandomBuildingsGenerationDefinition randomBuildingsGenerationDefinition = new RandomBuildingsGenerationDefinition(item);
			RandomBuildingsGenerationDefinitions.Add(randomBuildingsGenerationDefinition.Id, randomBuildingsGenerationDefinition);
		}
	}

	private void DeserializeRandomBuildingsDirections()
	{
		if (RandomBuildingsDirectionsDefinitions != null)
		{
			return;
		}
		XElement xElement = XDocument.Parse(randomBuildingsDirectionsDefinitions.text, LoadOptions.SetBaseUri).Element("RandomBuildingsDirectionsDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("randomBuildingsDirectionsDefinitions document must have a RandomBuildingsDirectionsDefinitions Element", LogType.Error);
			return;
		}
		RandomBuildingsDirectionsDefinitions = new Dictionary<string, RandomBuildingsDirectionsDefinition>();
		foreach (XElement item in xElement.Elements("RandomBuildingsDirectionsDefinition"))
		{
			RandomBuildingsDirectionsDefinition randomBuildingsDirectionsDefinition = new RandomBuildingsDirectionsDefinition(item);
			RandomBuildingsDirectionsDefinitions.Add(randomBuildingsDirectionsDefinition.Id, randomBuildingsDirectionsDefinition);
		}
	}

	private void DeserializeRandomBuildingsPerDay()
	{
		if (RandomBuildingsPerDayDefinitions != null)
		{
			return;
		}
		XElement xElement = XDocument.Parse(randomBuildingsPerDayDefinitions.text, LoadOptions.SetBaseUri).Element("RandomBuildingsPerDayDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("randomBuildingsPerDayDefinitions document must have a RandomBuildingsGenerationsDefinitions Element", LogType.Error);
			return;
		}
		RandomBuildingsPerDayDefinitions = new Dictionary<string, RandomBuildingsPerDayDefinition>();
		foreach (XElement item in xElement.Elements("RandomBuildingsPerDayDefinition"))
		{
			RandomBuildingsPerDayDefinition randomBuildingsPerDayDefinition = new RandomBuildingsPerDayDefinition(item);
			RandomBuildingsPerDayDefinitions.Add(randomBuildingsPerDayDefinition.Id, randomBuildingsPerDayDefinition);
		}
	}

	private void DeserializeShop()
	{
		if (ShopDefinition == null)
		{
			XElement xElement = XDocument.Parse(shopDefinition.text, LoadOptions.SetBaseUri).Element("ShopDefinition");
			if (xElement == null)
			{
				CLoggerManager.Log("ShopDefinition document must have a ShopDefinition Element", LogType.Error);
			}
			else
			{
				ShopDefinition = new ShopDefinition(xElement);
			}
		}
	}

	private void DeserializeSkillSoundDefinitions()
	{
		if (BuildingSkillSoundDefinitions != null)
		{
			return;
		}
		XElement xElement = XDocument.Parse(buildingSkillSoundDefinitions.text, LoadOptions.SetBaseUri).Element("BuildingSkillSoundDefinitions");
		BuildingSkillSoundDefinitions = new Dictionary<string, BuildingSkillSoundDefinition>();
		foreach (XElement item in xElement.Elements("BuildingSkillSoundDefinition"))
		{
			BuildingSkillSoundDefinition buildingSkillSoundDefinition = new BuildingSkillSoundDefinition(item);
			BuildingSkillSoundDefinitions.Add(buildingSkillSoundDefinition.BuildingTemplateDefinitionId, buildingSkillSoundDefinition);
		}
	}

	private void DeserializeUpgrades()
	{
		if (BuildingUpgradeDefinitions != null)
		{
			return;
		}
		BuildingUpgradeDefinitions = new Dictionary<string, BuildingUpgradeDefinition>();
		foreach (XElement item in XDocument.Parse(buildingUpgradeDefinitions.text, LoadOptions.SetBaseUri).Element("BuildingUpgradeDefinitions").Elements("BuildingUpgradeDefinition"))
		{
			BuildingUpgradeDefinition buildingUpgradeDefinition = new BuildingUpgradeDefinition(item);
			BuildingUpgradeDefinitions.Add(buildingUpgradeDefinition.Id, buildingUpgradeDefinition);
		}
	}
}
