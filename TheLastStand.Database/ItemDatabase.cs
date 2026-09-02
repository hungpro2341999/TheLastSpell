using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Definition.DLC;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Manager.Item;
using UnityEngine;

namespace TheLastStand.Database;

public class ItemDatabase : Database<ItemDatabase>
{
	[SerializeField]
	private TextAsset affixDefinitions;

	[SerializeField]
	private TextAsset affixLevelsDefinition;

	[SerializeField]
	private TextAsset affixMalusDefinitions;

	[SerializeField]
	private TextAsset[] itemListsDefinitions;

	[SerializeField]
	private TextAsset itemGenerationModifierListDefinitions;

	[SerializeField]
	private TextAsset itemRaritiesListDefinitions;

	[SerializeField]
	private TextAsset[] groupItemDefinitions;

	[SerializeField]
	private DLCTextAssetDefinition[] dlcGroupItemDefinitions;

	[SerializeField]
	private TextAsset[] individualItemDefinitions;

	[SerializeField]
	private TextAsset[] groupItemsArtDefinitions;

	[SerializeField]
	private DLCTextAssetDefinition[] dlcGroupItemsArtDefinitions;

	[SerializeField]
	private TextAsset[] individualArtItemDefinitions;

	[SerializeField]
	private TextAsset itemRestrictionCategoriesCollectionDefinitions;

	[SerializeField]
	private TextAsset itemSlotDefinitions;

	[SerializeField]
	private TextAsset itemConfig;

	[SerializeField]
	private TextAsset startStockItemDefinitions;

	public static Dictionary<string, AffixDefinition> AffixDefinitions { get; private set; }

	public static Dictionary<ItemDefinition.E_Rarity, int> AffixesCountPerRarity { get; private set; }

	public static Dictionary<UnitStatDefinition.E_Stat, AffixMalusDefinition> AffixMalusDefinitions { get; private set; }

	public static AffixLevelsDefinition AffixLevelsDefinition { get; private set; }

	public static Dictionary<string, ItemDefinition> AllItemsDefinitions { get; private set; }

	public static Dictionary<string, ItemDefinition> ItemDefinitions { get; private set; }

	public static Dictionary<string, ProbabilityTreeEntriesDefinition> ItemGenerationModifierListDefinitions { get; private set; }

	public static Dictionary<string, ItemsListDefinition> ItemsListDefinitions { get; private set; }

	public static Dictionary<string, ProbabilityTreeEntriesDefinition> ItemRaritiesListDefinitions { get; private set; }

	public static Dictionary<string, ItemRestrictionCategoriesCollectionDefinition> ItemRestrictionCategoriesCollectionDefinitions { get; private set; }

	public static Dictionary<string, ItemRestrictionFamilyDefinition> ItemRestrictionFamiliesDefinitions { get; private set; }

	public static Dictionary<ItemSlotDefinition.E_ItemSlotId, ItemSlotDefinition> ItemSlotDefinitions { get; private set; }

	public static Dictionary<string, List<string>> ItemsByTag { get; } = new Dictionary<string, List<string>>();

	public static Node ItemPriceEquation { get; private set; }

	public static float ItemPriceEquationConstant1 { get; private set; }

	public static float ItemPriceEquationConstant2 { get; private set; }

	public static float ItemPriceEquationPowerConstant { get; private set; }

	public static List<CreateItemDefinition> StartStockItemDefinitions { get; set; }

	public override void Deserialize(XContainer container = null)
	{
		DeserializeAffixes();
		DeserializeAffixesMalus();
		DeserializeItems();
		DeserializeItemLists();
		DeserializeItemConfig();
		DeserializeItemRaritiesList();
		DeserializeModifierLists();
		DeserializeStartStockItemDefinitions();
		CleanEmptyListsAndMissingItems();
		DeserializeItemRestrictions();
	}

	private void DeserializeAffixes()
	{
		AffixDefinitions = new Dictionary<string, AffixDefinition>();
		XElement xElement = XDocument.Parse(affixDefinitions.text, LoadOptions.SetBaseUri).Element("AffixDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("No AffixDefinitions", LogType.Error);
			return;
		}
		foreach (XElement item in xElement.Elements("AffixDefinition"))
		{
			AffixDefinition affixDefinition = new AffixDefinition(item);
			AffixDefinitions.Add(affixDefinition.Id, affixDefinition);
		}
		XElement xElement2 = XDocument.Parse(affixLevelsDefinition.text, LoadOptions.SetBaseUri).Element("AffixLevelsDefinition");
		if (xElement2 == null)
		{
			CLoggerManager.Log("No AffixLevelsDefinition in TextAsset " + affixLevelsDefinition.name + "!", LogType.Error);
		}
		else
		{
			AffixLevelsDefinition = new AffixLevelsDefinition(xElement2);
		}
	}

	private void DeserializeAffixesMalus()
	{
		AffixMalusDefinitions = new Dictionary<UnitStatDefinition.E_Stat, AffixMalusDefinition>(UnitStatDefinition.SharedStatComparer);
		XElement xElement = XDocument.Parse(affixMalusDefinitions.text, LoadOptions.SetBaseUri).Element("AffixMalusDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("No AffixMalusDefinition in TextAsset " + affixMalusDefinitions.name + "!", LogType.Error);
			return;
		}
		foreach (XElement item in xElement.Elements("AffixMalusDefinition"))
		{
			AffixMalusDefinition affixMalusDefinition = new AffixMalusDefinition(item);
			AffixMalusDefinitions.Add(affixMalusDefinition.Stat, affixMalusDefinition);
		}
	}

	private void DeserializeItemLists()
	{
		if (ItemsListDefinitions != null)
		{
			return;
		}
		ItemsListDefinitions = new Dictionary<string, ItemsListDefinition>();
		Queue<XElement> queue = GatherElements(itemListsDefinitions, null, "ItemsListDefinition");
		while (queue.Count > 0)
		{
			ItemsListDefinition itemsListDefinition = new ItemsListDefinition(queue.Dequeue());
			try
			{
				ItemsListDefinitions.Add(itemsListDefinition.Id, itemsListDefinition);
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log("Duplicate ItemsListDefinition found for ID " + itemsListDefinition.Id + ": the individual files will have PRIORITY over the all-in-one template file.", LogType.Warning);
			}
		}
	}

	private void DeserializeItemRaritiesList()
	{
		if (ItemRaritiesListDefinitions != null)
		{
			return;
		}
		ItemRaritiesListDefinitions = new Dictionary<string, ProbabilityTreeEntriesDefinition>();
		XElement xElement = XDocument.Parse(itemRaritiesListDefinitions.text, LoadOptions.SetBaseUri).Element("ItemRaritiesListDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("The document must have ItemRaritiesListDefinitions", LogType.Error);
			return;
		}
		foreach (XElement item in xElement.Elements("ItemRaritiesListDefinition"))
		{
			ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition = new ProbabilityTreeEntriesDefinition(item);
			ItemRaritiesListDefinitions.Add(probabilityTreeEntriesDefinition.Id, probabilityTreeEntriesDefinition);
		}
	}

	private void DeserializeItemRestrictions()
	{
		if (ItemRestrictionCategoriesCollectionDefinitions != null)
		{
			return;
		}
		ItemRestrictionCategoriesCollectionDefinitions = new Dictionary<string, ItemRestrictionCategoriesCollectionDefinition>();
		ItemRestrictionFamiliesDefinitions = new Dictionary<string, ItemRestrictionFamilyDefinition>();
		XElement xElement = XDocument.Parse(itemRestrictionCategoriesCollectionDefinitions.text, LoadOptions.SetBaseUri).Element("ItemRestrictionCategoriesCollectionDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("The document must have ItemRestrictionCategoriesCollectionDefinitions", LogType.Error);
			return;
		}
		foreach (XElement item in xElement.Elements("ItemRestrictionCategoriesCollectionDefinition"))
		{
			ItemRestrictionCategoriesCollectionDefinition itemRestrictionCategoriesCollectionDefinition = new ItemRestrictionCategoriesCollectionDefinition(item);
			ItemRestrictionCategoriesCollectionDefinitions.Add(itemRestrictionCategoriesCollectionDefinition.Id, itemRestrictionCategoriesCollectionDefinition);
		}
	}

	private void DeserializeItems()
	{
		if (ItemDefinitions != null)
		{
			return;
		}
		AllItemsDefinitions = new Dictionary<string, ItemDefinition>();
		ItemDefinitions = new Dictionary<string, ItemDefinition>();
		List<TextAsset> list = new List<TextAsset>();
		list.AddRange(groupItemDefinitions);
		int count = list.Count;
		list.AddRange(GenericDatabase.GetDLCTextAssets(dlcGroupItemDefinitions));
		int num = list.Count - count;
		List<TextAsset> list2 = new List<TextAsset>();
		list2.AddRange(groupItemsArtDefinitions);
		list2.AddRange(GenericDatabase.GetDLCTextAssets(dlcGroupItemsArtDefinitions));
		Queue<XElement> queue = GatherElements(list, individualItemDefinitions, "ItemDefinition");
		Queue<XElement> queue2 = GatherElements(list2, individualArtItemDefinitions, "ItemArtDefinition");
		while (queue.Count > 0)
		{
			ItemDefinition itemDefinition = new ItemDefinition(queue.Dequeue());
			try
			{
				ItemDefinitions.Add(itemDefinition.Id, itemDefinition);
				AllItemsDefinitions.Add(itemDefinition.Id, itemDefinition);
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log("Duplicate ItemDefinition found for ID " + itemDefinition.Id + ": the individual files will have PRIORITY over the all-in-one template file.", LogType.Warning);
			}
		}
		list.Clear();
		list.AddRange(GenericDatabase.GetDLCTextAssets(dlcGroupItemDefinitions, forceGetAllTextAssetDefinitions: true));
		Queue<XElement> queue3 = GatherElements(list, individualItemDefinitions, "ItemDefinition");
		if (num != list.Count)
		{
			while (queue3.Count > 0)
			{
				ItemDefinition itemDefinition2 = new ItemDefinition(queue3.Dequeue());
				if (!AllItemsDefinitions.ContainsKey(itemDefinition2.Id))
				{
					AllItemsDefinitions.Add(itemDefinition2.Id, itemDefinition2);
				}
			}
		}
		while (queue2.Count > 0)
		{
			XElement xElement = queue2.Dequeue();
			XAttribute xAttribute = xElement.Attribute("Id");
			try
			{
				ItemDefinitions[xAttribute.Value].DeserializeArtRelatedDatas(xElement);
			}
			catch (KeyNotFoundException)
			{
				CLoggerManager.Log($"ID {xAttribute} found in an item art definition but could not find the actual item definition to link.", LogType.Warning);
			}
		}
		ItemSlotDefinitions = new Dictionary<ItemSlotDefinition.E_ItemSlotId, ItemSlotDefinition>();
		XElement xElement2 = XDocument.Parse(itemSlotDefinitions.text, LoadOptions.SetBaseUri).Element("ItemSlotDefinitions");
		if (xElement2 == null)
		{
			CLoggerManager.Log("No ItemSlotDefinitions!", LogType.Error);
			return;
		}
		foreach (XElement item in xElement2.Elements("ItemSlotDefinition"))
		{
			ItemSlotDefinition itemSlotDefinition = new ItemSlotDefinition(item);
			ItemSlotDefinitions.Add(itemSlotDefinition.Id, itemSlotDefinition);
		}
	}

	private void DeserializeItemConfig()
	{
		if (AffixesCountPerRarity != null)
		{
			return;
		}
		XElement xElement = XDocument.Parse(itemConfig.text, LoadOptions.SetBaseUri).Element("ItemConfig");
		XElement xElement2 = xElement.Element("AffixesCountPerRarity");
		AffixesCountPerRarity = new Dictionary<ItemDefinition.E_Rarity, int>(ItemDefinition.SharedRarityComparer);
		foreach (XElement item in xElement2.Elements("Rarity"))
		{
			int result2;
			if (!Enum.TryParse<ItemDefinition.E_Rarity>(item.Attribute("Id").Value, out var result) || result < ItemDefinition.E_Rarity.Common || result > ItemDefinition.E_Rarity.Epic)
			{
				CLoggerManager.Log("The rarity has an invalid Id!", LogType.Error);
			}
			else if (!int.TryParse(item.Value, out result2))
			{
				CLoggerManager.Log($"The rarity {result} has an invalid AffixesCount!", LogType.Error);
			}
			else
			{
				AffixesCountPerRarity.Add(result, result2);
			}
		}
		ItemPriceEquation = Parser.Parse(xElement.Element("ItemPriceEquation").Value);
		XElement xElement3 = xElement.Element("ItemPriceEquationConstant1");
		XElement xElement4 = xElement.Element("ItemPriceEquationConstant2");
		XElement xElement5 = xElement.Element("ItemPriceEquationPowerConstant");
		if (!float.TryParse(xElement3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
		{
			TPSingleton<ItemManager>.Instance.LogError("Could not cast the item price equation constant 1 into a float. Current value : " + xElement3.Value);
			return;
		}
		if (!float.TryParse(xElement4.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result4))
		{
			TPSingleton<ItemManager>.Instance.LogError("Could not cast the item price equation constant 2 into a float. Current value : " + xElement4.Value);
			return;
		}
		if (!float.TryParse(xElement5.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result5))
		{
			TPSingleton<ItemManager>.Instance.LogError("Could not cast the item price equation power constant into a float. Current value : " + xElement5.Value);
			return;
		}
		ItemPriceEquationConstant1 = result3;
		ItemPriceEquationConstant2 = result4;
		ItemPriceEquationPowerConstant = result5;
	}

	private void DeserializeModifierLists()
	{
		if (ItemGenerationModifierListDefinitions != null)
		{
			return;
		}
		ItemGenerationModifierListDefinitions = new Dictionary<string, ProbabilityTreeEntriesDefinition>();
		XElement xElement = XDocument.Parse(itemGenerationModifierListDefinitions.text, LoadOptions.SetBaseUri).Element("ItemGenerationModifiersListDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("The document must have xItemGenerationModifierListDefinitions", LogType.Error);
			return;
		}
		foreach (XElement item in xElement.Elements("ItemGenerationModifiersListDefinition"))
		{
			ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition = new ProbabilityTreeEntriesDefinition(item);
			ItemGenerationModifierListDefinitions.Add(probabilityTreeEntriesDefinition.Id, probabilityTreeEntriesDefinition);
		}
	}

	private void DeserializeStartStockItemDefinitions()
	{
		if (StartStockItemDefinitions != null)
		{
			return;
		}
		XElement xElement = XDocument.Parse(startStockItemDefinitions.text, LoadOptions.SetBaseUri).Element("StartInventoryItemDefinitions");
		StartStockItemDefinitions = new List<CreateItemDefinition>();
		foreach (XElement item2 in xElement.Elements("CreateItem"))
		{
			CreateItemDefinition item = new CreateItemDefinition(item2);
			StartStockItemDefinitions.Add(item);
		}
	}

	protected void CleanEmptyListsAndMissingItems()
	{
		RemoveEmptyLists();
		RemovedMissingDefinitionsFromLists();
	}

	private void RemoveEmptyLists()
	{
		List<string> list = new List<string>();
		foreach (ItemsListDefinition value in ItemsListDefinitions.Values)
		{
			if (value.IsEmpty)
			{
				list.Add(value.Id);
			}
		}
		foreach (string item in list)
		{
			if (ItemsListDefinitions.ContainsKey(item))
			{
				ItemsListDefinitions.Remove(item);
			}
		}
	}

	private void RemovedMissingDefinitionsFromLists()
	{
		foreach (ItemsListDefinition value3 in ItemsListDefinitions.Values)
		{
			if (value3.IsEmpty)
			{
				continue;
			}
			List<string> list = new List<string>();
			foreach (string key in value3.ItemsWithOdd.Keys)
			{
				if (!ItemDefinitions.TryGetValue(key, out var _) && !ItemsListDefinitions.TryGetValue(key, out var _))
				{
					list.Add(key);
				}
			}
			foreach (string item in list)
			{
				value3.ItemsWithOdd.Remove(item);
			}
		}
	}
}
