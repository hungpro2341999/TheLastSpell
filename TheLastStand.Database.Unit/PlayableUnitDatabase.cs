using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Definition.DLC;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.PlayableUnitGeneration;
using TheLastStand.Definition.Unit.Race;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Unit;
using UnityEngine;

namespace TheLastStand.Database.Unit;

public class PlayableUnitDatabase : Database<PlayableUnitDatabase>
{
	[SerializeField]
	private List<DataColor> portraitBackgroundColors = new List<DataColor>();

	[SerializeField]
	private Dictionary<int, int> perksPointsPerLevel = new Dictionary<int, int>();

	[SerializeField]
	private TextAsset playableUnitConfigTextAsset;

	[SerializeField]
	private TextAsset unitLevelUpStats;

	[SerializeField]
	private TextAsset unitLevelUpSettings;

	[SerializeField]
	private TextAsset unitEquipmentSlotsDefinitions;

	[SerializeField]
	private TextAsset unitTraitDefinitions;

	[SerializeField]
	private TextAsset unitTraitGenerationDefinition;

	[SerializeField]
	private TextAsset playableUnitGenerationDefinitionTextAsset;

	[SerializeField]
	private TextAsset playableUnitTemplateDefinition;

	[SerializeField]
	private TextAsset[] unitsGenerationsStartDefinitionsTextAssets;

	[SerializeField]
	private TextAsset unitGenerationLevelDefinitions;

	[SerializeField]
	private TextAsset recruitmentDefinition;

	[SerializeField]
	private TextAsset unitStartingRosterRacesDistributionDefinitions;

	[SerializeField]
	private TextAsset unitPerkTemplateDefinition;

	[SerializeField]
	private TextAsset unitPerkCollectionDefinitions;

	[SerializeField]
	private TextAsset[] perksDefinitions;

	[SerializeField]
	private TextAsset[] racesDefinitions;

	[SerializeField]
	private DLCTextAssetDefinition[] dlcRacesDefinitions;

	[SerializeField]
	private RuntimeAnimatorController raceDefaultAnimator;

	[SerializeField]
	private TextAsset unitMaleHeads;

	[SerializeField]
	private TextAsset unitFemaleHeads;

	[SerializeField]
	private TextAsset unitNakedBodyPartDefinitions;

	[SerializeField]
	private TextAsset unitHairColorDefinitions;

	[SerializeField]
	private TextAsset unitSkinColorDefinitions;

	[SerializeField]
	private TextAsset unitEyesColorDefinitions;

	[SerializeField]
	private TextAsset unitLinkHairSkinColorDefinitions;

	[SerializeField]
	private TextAsset unitMaleNames;

	[SerializeField]
	private TextAsset unitFemaleNames;

	[SerializeField]
	private int startingUnitsSpawnAreaSize = 8;

	[Tooltip("Areas are currently on the left AND and on the right of the circle, so this value should be rather small, like 2 or 3.")]
	[SerializeField]
	private int victoryUnitsGatherAreaSize = 2;

	[SerializeField]
	[Range(0f, 100f)]
	private float unitMoveSpeed = 15f;

	public static Node ExperienceNeededToNextLevel { get; private set; }

	public static float KillerBonusExperienceFactor { get; private set; }

	public static Dictionary<int, int> PerksPointsPerLevel => TPSingleton<PlayableUnitDatabase>.Instance.perksPointsPerLevel;

	public static UnitFaceIdDefinitions PlayableFemaleUnitFaceIds { get; set; }

	public static UnitFaceIdDefinitions PlayableMaleUnitFaceIds { get; set; }

	public static Dictionary<string, PlayableUnitGenerationDefinition> PlayableUnitGenerationDefinitions { get; private set; }

	public static Dictionary<string, ColorSwapPaletteDefinition> PlayableUnitHairColorDefinitions { get; private set; }

	public static Dictionary<string, BodyPartDefinition> PlayableUnitNakedBodyPartsDefinitions { get; private set; }

	public static Dictionary<string, ColorSwapPaletteDefinition> PlayableUnitSkinColorDefinitions { get; private set; }

	public static Dictionary<string, ColorSwapPaletteDefinition> PlayableUnitEyesColorDefinitions { get; private set; }

	public static PlayableUnitTemplateDefinition PlayableUnitTemplateDefinition { get; private set; }

	public static List<DataColor> PortraitBackgroundColors => TPSingleton<PlayableUnitDatabase>.Instance.portraitBackgroundColors;

	public static List<string> SecondaryTraitIds { get; private set; }

	public static List<int> SecondaryTraitCost { get; private set; }

	public static int StartingUnitsSpawnAreaSize => TPSingleton<PlayableUnitDatabase>.Instance.startingUnitsSpawnAreaSize;

	public static Dictionary<ItemSlotDefinition.E_ItemSlotId, UnitEquipmentSlotDefinition> UnitEquipmentSlotDefinitions { get; set; }

	public static Dictionary<string, UnitGenerationLevelDefinition> UnitGenerationLevelDefinitions { get; set; }

	public static Dictionary<string, List<UnitGenerationDefinition>> UnitsGenerationStartDefinitions { get; set; }

	public static Dictionary<int, UnitStartingRosterRacesDistributionDefinition> UnitStartingRosterRacesDistributionDefinitionsByUnlockedRacesNb { get; set; }

	public static Dictionary<UnitStatDefinition.E_Stat, UnitLevelUpStatDefinition> UnitLevelUpMainStatDefinitions { get; private set; }

	public static Dictionary<UnitStatDefinition.E_Stat, UnitLevelUpStatDefinition> UnitLevelUpSecondaryStatDefinitions { get; private set; }

	public static UnitLevelUpDefinition UnitLevelUpDefinition { get; private set; }

	public static UnitLinkHairSkin UnitLinkHairSkin { get; private set; }

	public static float UnitMoveSpeed => TPSingleton<PlayableUnitDatabase>.Instance.unitMoveSpeed;

	public static Dictionary<string, UnitPerkCollectionDefinition> UnitPerkCollectionDefinitions { get; private set; }

	public static Dictionary<string, PerkDefinition> PerkDefinitions { get; private set; }

	public static Dictionary<string, RaceDefinition> RaceDefinitions { get; private set; }

	public static RuntimeAnimatorController RaceDefaultAnimatorController { get; private set; }

	public static UnitPerkTemplateDefinition UnitPerkTemplateDefinition { get; private set; }

	public static Dictionary<string, UnitTraitDefinition> UnitTraitDefinitions { get; private set; }

	public static Dictionary<int, string> UnitTraitTiersId { get; private set; }

	public static Dictionary<int, string> UnitBackgroundTraitTiersId { get; private set; }

	public static UnitTraitGenerationDefinition UnitTraitGenerationDefinition { get; private set; }

	private static List<string> PlayableFemaleUnitNames { get; set; }

	private static List<string> PlayableMaleUnitNames { get; set; }

	public static RecruitmentDefinition RecruitmentDefinition { get; private set; }

	public static int VictoryUnitsGatherAreaSize => TPSingleton<PlayableUnitDatabase>.Instance.victoryUnitsGatherAreaSize;

	public static List<string> GetFaceIdsForGender(string gender)
	{
		return gender switch
		{
			"Female" => PlayableFemaleUnitFaceIds.ToStringList(), 
			"Male" => PlayableMaleUnitFaceIds.ToStringList(), 
			_ => null, 
		};
	}

	public static List<string> GetNamesForGender(string gender)
	{
		return gender switch
		{
			"Female" => PlayableFemaleUnitNames, 
			"Male" => PlayableMaleUnitNames, 
			_ => null, 
		};
	}

	public override void Deserialize(XContainer container = null)
	{
		DeserializePlayableUnitConfig();
		DeserializeUnitFaceIds();
		DeserializeUnitNakedBodyParts();
		DeserializeUnitNames();
		DeserializeUnitHairColors();
		DeserializeUnitSkinColors();
		DeserializeUnitEyesColors();
		DeserializeUnitLinkHairSkinColors();
		DeserializePerks();
		DeserializeRaces();
		DeserializeUnitPerksCollections();
		DeserializeUnitPerksTemplate();
		DeserializeUnitEquipmentSlots();
		DeserializeUnitTraits();
		DeserializeUnitsGeneration();
		DeserializePlayableUnitTemplate();
		DeserializeUnitLevelUpStats();
	}

	private static int CompareTraitsByCost(string traitId1, string traitId2)
	{
		return UnitTraitDefinitions[traitId1].Cost.CompareTo(UnitTraitDefinitions[traitId2].Cost);
	}

	private static void LoadUnitFaceIds(XDocument idsDocument, string gender)
	{
		switch (gender)
		{
		case "Male":
			PlayableMaleUnitFaceIds = new UnitFaceIdDefinitions(idsDocument);
			break;
		case "Female":
			PlayableFemaleUnitFaceIds = new UnitFaceIdDefinitions(idsDocument);
			break;
		}
	}

	private static void LoadUnitNames(string names, string gender)
	{
		List<string> namesForGender = GetNamesForGender(gender);
		string[] array = names.Split('\n');
		for (int num = array.Length - 1; num >= 0; num--)
		{
			array[num] = array[num].Trim();
			if (array[num] != string.Empty)
			{
				string text = string.Empty;
				for (int i = 0; i < array[num].Length; i++)
				{
					text += array[num][i];
				}
				namesForGender.Add(text);
			}
		}
	}

	private void DeserializePlayableUnitConfig()
	{
		XElement xElement = XDocument.Parse(playableUnitConfigTextAsset.text, LoadOptions.SetBaseUri).Element("PlayableUnitConfig");
		ExperienceNeededToNextLevel = Parser.Parse(xElement.Element("ExperienceNeededToNextLevel").Value);
		XElement xElement2 = xElement.Element("BonusExperiencePerKill");
		if (!float.TryParse(xElement2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			CLoggerManager.Log("Could not parse BonusExperiencePerKill element value \"" + xElement2.Value + "\" to a valid float value.", LogType.Error);
		}
		else
		{
			KillerBonusExperienceFactor = result;
		}
	}

	private void DeserializePlayableUnitTemplate()
	{
		if (PlayableUnitTemplateDefinition == null)
		{
			PlayableUnitTemplateDefinition = new PlayableUnitTemplateDefinition(XDocument.Parse(playableUnitTemplateDefinition.text, LoadOptions.SetBaseUri).Element("PlayableUnitTemplateDefinition"));
		}
	}

	private void DeserializeUnitLinkHairSkinColors()
	{
		UnitLinkHairSkin = new UnitLinkHairSkin(XDocument.Parse(unitLinkHairSkinColorDefinitions.text, LoadOptions.SetBaseUri).Element("UnitLinkHairSkinColorDefinition"));
	}

	private void DeserializeUnitFaceIds()
	{
		if (PlayableFemaleUnitFaceIds == null && PlayableMaleUnitFaceIds == null)
		{
			XDocument idsDocument = XDocument.Parse(TPSingleton<PlayableUnitDatabase>.Instance.unitMaleHeads.text, LoadOptions.SetBaseUri);
			XDocument idsDocument2 = XDocument.Parse(TPSingleton<PlayableUnitDatabase>.Instance.unitFemaleHeads.text, LoadOptions.SetBaseUri);
			LoadUnitFaceIds(idsDocument, "Male");
			LoadUnitFaceIds(idsDocument2, "Female");
		}
	}

	private void DeserializeUnitEquipmentSlots()
	{
		UnitEquipmentSlotDefinitions = new Dictionary<ItemSlotDefinition.E_ItemSlotId, UnitEquipmentSlotDefinition>();
		foreach (XElement item in XDocument.Parse(unitEquipmentSlotsDefinitions.text, LoadOptions.SetBaseUri).Element("UnitEquipmentSlotDefinitions").Elements("UnitEquipmentSlotDefinition"))
		{
			if (item.Attribute("Id").IsNullOrEmpty())
			{
				CLoggerManager.Log("The UnitEquipmentSlotDefinition must have a valid id", LogType.Error);
				continue;
			}
			UnitEquipmentSlotDefinition unitEquipmentSlotDefinition = new UnitEquipmentSlotDefinition(item);
			UnitEquipmentSlotDefinitions.Add(unitEquipmentSlotDefinition.Id, unitEquipmentSlotDefinition);
		}
	}

	private void DeserializeUnitsGeneration()
	{
		if (UnitGenerationLevelDefinitions != null)
		{
			return;
		}
		UnitGenerationLevelDefinitions = new Dictionary<string, UnitGenerationLevelDefinition>();
		XElement xElement = XDocument.Parse(unitGenerationLevelDefinitions.text, LoadOptions.SetBaseUri).Element("UnitGenerationLevelDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("The document must have UnitGenerationLevelDefinitions", LogType.Error);
			return;
		}
		foreach (XElement item2 in xElement.Elements("UnitGenerationLevelDefinition"))
		{
			UnitGenerationLevelDefinition unitGenerationLevelDefinition = new UnitGenerationLevelDefinition(item2);
			UnitGenerationLevelDefinitions.Add(unitGenerationLevelDefinition.Id, unitGenerationLevelDefinition);
		}
		UnitStartingRosterRacesDistributionDefinitionsByUnlockedRacesNb = new Dictionary<int, UnitStartingRosterRacesDistributionDefinition>();
		XElement xElement2 = XDocument.Parse(unitStartingRosterRacesDistributionDefinitions.text, LoadOptions.SetBaseUri).Element("UnitStartingRosterRacesDistributionDefinitions");
		if (xElement2 == null)
		{
			CLoggerManager.Log("The document must have UnitStartingRosterRacesDistributionDefinitions", LogType.Error);
			return;
		}
		foreach (XElement item3 in xElement2.Elements("UnitStartingRosterRacesDistributionDefinition"))
		{
			UnitStartingRosterRacesDistributionDefinition unitStartingRosterRacesDistributionDefinition = new UnitStartingRosterRacesDistributionDefinition(item3);
			UnitStartingRosterRacesDistributionDefinitionsByUnlockedRacesNb.Add(unitStartingRosterRacesDistributionDefinition.UnlockedNonHumanRacesNb, unitStartingRosterRacesDistributionDefinition);
		}
		UnitsGenerationStartDefinitions = new Dictionary<string, List<UnitGenerationDefinition>>();
		Queue<XElement> queue = GatherElements(unitsGenerationsStartDefinitionsTextAssets, null, "UnitsGenerationStartDefinitions", "UnitsGenerationsStartDefinitions");
		while (queue.Count > 0)
		{
			XElement xElement3 = queue.Dequeue();
			XAttribute xAttribute = xElement3.Attribute("Id");
			UnitsGenerationStartDefinitions.Add(xAttribute.Value, new List<UnitGenerationDefinition>());
			foreach (XElement item4 in xElement3.Elements("UnitGenerationStartDefinition"))
			{
				UnitGenerationDefinition item = new UnitGenerationDefinition(item4);
				UnitsGenerationStartDefinitions[xAttribute.Value].Add(item);
			}
		}
		XElement xElement4 = XDocument.Parse(recruitmentDefinition.text, LoadOptions.SetBaseUri).Element("RecruitmentDefinition");
		if (xElement4 == null)
		{
			CLoggerManager.Log("The document must have RecruitmentDefinition", LogType.Error);
			return;
		}
		RecruitmentDefinition = new RecruitmentDefinition(xElement4);
		XElement xElement5 = XDocument.Parse(playableUnitGenerationDefinitionTextAsset.text, LoadOptions.SetBaseUri).Element("PlayableUnitGenerationDefinitions");
		if (xElement5 == null)
		{
			CLoggerManager.Log("The playableUnitGenerationDefinitionsDocument must have an element PlayableUnitGenerationDefinitions", LogType.Error);
			return;
		}
		PlayableUnitGenerationDefinitions = new Dictionary<string, PlayableUnitGenerationDefinition>();
		foreach (XElement item5 in xElement5.Elements("PlayableUnitGenerationDefinition"))
		{
			PlayableUnitGenerationDefinition playableUnitGenerationDefinition = new PlayableUnitGenerationDefinition(item5);
			PlayableUnitGenerationDefinitions.Add(playableUnitGenerationDefinition.ArchetypeId, playableUnitGenerationDefinition);
		}
	}

	private void DeserializeUnitHairColors()
	{
		if (PlayableUnitHairColorDefinitions != null)
		{
			return;
		}
		if (unitHairColorDefinitions == null)
		{
			TPDebug.LogError("The document unitHairColorDefinitions can't be null", this);
			return;
		}
		XElement xElement = XDocument.Parse(unitHairColorDefinitions.text, LoadOptions.SetBaseUri).Element("UnitHairColorDefinitions");
		if (xElement == null)
		{
			TPDebug.LogError("The document unitHairColorDefinitions must define UnitHairColorDefinitions", this);
			return;
		}
		PlayableUnitHairColorDefinitions = new Dictionary<string, ColorSwapPaletteDefinition>();
		foreach (XElement item in xElement.Elements("ColorSwapPalette"))
		{
			ColorSwapPaletteDefinition colorSwapPaletteDefinition = new ColorSwapPaletteDefinition(item);
			PlayableUnitHairColorDefinitions.Add(colorSwapPaletteDefinition.Id, colorSwapPaletteDefinition);
		}
	}

	private void DeserializeUnitLevelUpStats()
	{
		XElement xElement = XDocument.Parse(unitLevelUpStats.text, LoadOptions.SetBaseUri).Element("UnitLevelUpStatDefinitions");
		UnitLevelUpMainStatDefinitions = new Dictionary<UnitStatDefinition.E_Stat, UnitLevelUpStatDefinition>(UnitStatDefinition.SharedStatComparer);
		UnitLevelUpSecondaryStatDefinitions = new Dictionary<UnitStatDefinition.E_Stat, UnitLevelUpStatDefinition>(UnitStatDefinition.SharedStatComparer);
		foreach (XElement item in xElement.Element("MainStats").Elements("UnitLevelUpStatDefinition"))
		{
			UnitLevelUpStatDefinition unitLevelUpStatDefinition = new UnitLevelUpStatDefinition(item);
			UnitLevelUpMainStatDefinitions.Add(unitLevelUpStatDefinition.Stat, unitLevelUpStatDefinition);
		}
		foreach (XElement item2 in xElement.Element("SecondaryStats").Elements("UnitLevelUpStatDefinition"))
		{
			UnitLevelUpStatDefinition unitLevelUpStatDefinition2 = new UnitLevelUpStatDefinition(item2);
			UnitLevelUpSecondaryStatDefinitions.Add(unitLevelUpStatDefinition2.Stat, unitLevelUpStatDefinition2);
		}
		UnitLevelUpDefinition = new UnitLevelUpDefinition(XDocument.Parse(unitLevelUpSettings.text, LoadOptions.SetBaseUri).Element("UnitLevelUpDefinition"));
	}

	private void DeserializeUnitNakedBodyParts()
	{
		if (PlayableUnitNakedBodyPartsDefinitions != null)
		{
			return;
		}
		if (unitNakedBodyPartDefinitions == null)
		{
			TPDebug.LogError("The document nakedBodyPartsDefinitions can't be null", this);
			return;
		}
		XElement xElement = XDocument.Parse(unitNakedBodyPartDefinitions.text, LoadOptions.SetBaseUri).Element("UnitNakedBodyPartDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("The document nakedBodyPartsDefinitions must define xBodyPartDefinitions", LogType.Error);
			return;
		}
		PlayableUnitNakedBodyPartsDefinitions = new Dictionary<string, BodyPartDefinition>();
		foreach (XElement item in xElement.Elements("BodyPartDefinition"))
		{
			BodyPartDefinition bodyPartDefinition = new BodyPartDefinition(item);
			PlayableUnitNakedBodyPartsDefinitions.Add(bodyPartDefinition.Id, bodyPartDefinition);
		}
	}

	private void DeserializeUnitNames()
	{
		if (PlayableFemaleUnitNames == null && PlayableMaleUnitNames == null)
		{
			PlayableFemaleUnitNames = new List<string>();
			PlayableMaleUnitNames = new List<string>();
			LoadUnitNames(TPSingleton<PlayableUnitDatabase>.Instance.unitMaleNames.text, "Male");
			LoadUnitNames(TPSingleton<PlayableUnitDatabase>.Instance.unitFemaleNames.text, "Female");
		}
	}

	private void DeserializePerks()
	{
		if (PerkDefinitions == null)
		{
			PerkDefinitions = new Dictionary<string, PerkDefinition>();
			Queue<XElement> queue = GatherElements(perksDefinitions, null, "PerkDefinition", "PerkDefinitions");
			while (queue.Count > 0)
			{
				XElement xElement = queue.Dequeue();
				PerkDefinitions[xElement.Attribute("Id").Value] = new PerkDefinition(xElement);
			}
		}
	}

	private void DeserializeRaces()
	{
		if (RaceDefinitions == null)
		{
			RaceDefinitions = new Dictionary<string, RaceDefinition>();
			List<TextAsset> list = new List<TextAsset>();
			list.AddRange(racesDefinitions);
			list.AddRange(GenericDatabase.GetDLCTextAssets(dlcRacesDefinitions));
			Queue<XElement> queue = GatherElements(list, null, "RaceDefinition", "RaceDefinitions");
			while (queue.Count > 0)
			{
				XElement xElement = queue.Dequeue();
				XAttribute xAttribute = xElement.Attribute("Id");
				RaceDefinitions[xAttribute.Value] = new RaceDefinition(xElement);
			}
			RaceDefaultAnimatorController = raceDefaultAnimator;
		}
	}

	private void DeserializeUnitPerksCollections()
	{
		XElement xElement = XDocument.Parse(unitPerkCollectionDefinitions.text, LoadOptions.SetBaseUri).Element("UnitPerkCollectionDefinitions");
		UnitPerkCollectionDefinitions = new Dictionary<string, UnitPerkCollectionDefinition>();
		foreach (XElement item in xElement.Elements("UnitPerkCollectionDefinition"))
		{
			UnitPerkCollectionDefinition unitPerkCollectionDefinition = new UnitPerkCollectionDefinition(item);
			if (UnitPerkCollectionDefinitions.ContainsKey(unitPerkCollectionDefinition.Id))
			{
				CLoggerManager.Log("Perk collection \"" + unitPerkCollectionDefinition.Id + "\" already exists in database. Skip.", TPSingleton<PlayableUnitManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
			}
			else
			{
				UnitPerkCollectionDefinitions.Add(unitPerkCollectionDefinition.Id, unitPerkCollectionDefinition);
			}
		}
	}

	private void DeserializeUnitPerksTemplate()
	{
		UnitPerkTemplateDefinition = new UnitPerkTemplateDefinition(XDocument.Parse(unitPerkTemplateDefinition.text, LoadOptions.SetBaseUri).Element("UnitPerkTemplateDefinition"));
	}

	private void DeserializeUnitSkinColors()
	{
		if (PlayableUnitSkinColorDefinitions != null)
		{
			return;
		}
		if (unitSkinColorDefinitions == null)
		{
			TPDebug.LogError("The document unitSkinColorDefinitions can't be null", this);
			return;
		}
		XElement xElement = XDocument.Parse(unitSkinColorDefinitions.text, LoadOptions.SetBaseUri).Element("UnitSkinColorDefinitions");
		if (xElement == null)
		{
			TPDebug.LogError("The document unitSkinColorDefinitions must define UnitSkinColorDefinitions", this);
			return;
		}
		PlayableUnitSkinColorDefinitions = new Dictionary<string, ColorSwapPaletteDefinition>();
		foreach (XElement item in xElement.Elements("ColorSwapPalette"))
		{
			ColorSwapPaletteDefinition colorSwapPaletteDefinition = new ColorSwapPaletteDefinition(item);
			PlayableUnitSkinColorDefinitions.Add(colorSwapPaletteDefinition.Id, colorSwapPaletteDefinition);
		}
	}

	private void DeserializeUnitEyesColors()
	{
		if (PlayableUnitEyesColorDefinitions != null)
		{
			return;
		}
		if (unitEyesColorDefinitions == null)
		{
			CLoggerManager.Log("The document unitEyesColorDefinitions can't be null", LogType.Error);
			return;
		}
		XElement xElement = XDocument.Parse(unitEyesColorDefinitions.text, LoadOptions.SetBaseUri).Element("UnitEyesColorDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("The document unitEyesColorDefinitions must define UnitEyesColorDefinitions", LogType.Error);
			return;
		}
		PlayableUnitEyesColorDefinitions = new Dictionary<string, ColorSwapPaletteDefinition>();
		foreach (XElement item in xElement.Elements("ColorSwapPalette"))
		{
			ColorSwapPaletteDefinition colorSwapPaletteDefinition = new ColorSwapPaletteDefinition(item);
			PlayableUnitEyesColorDefinitions.Add(colorSwapPaletteDefinition.Id, colorSwapPaletteDefinition);
		}
	}

	private void DeserializeUnitTraits()
	{
		if (UnitTraitDefinitions != null)
		{
			return;
		}
		XElement xElement = XDocument.Parse(unitTraitDefinitions.text, LoadOptions.SetBaseUri).Element("UnitTraitDefinitions");
		List<UnitTraitTierDefinition> list = new List<UnitTraitTierDefinition>();
		foreach (XElement item in xElement.Element("UnitTraitTiers").Elements("TraitTier"))
		{
			list.Add(new UnitTraitTierDefinition(item));
		}
		UnitTraitTiersId = new Dictionary<int, string>();
		UnitBackgroundTraitTiersId = new Dictionary<int, string>();
		foreach (UnitTraitTierDefinition item2 in list)
		{
			Dictionary<int, string> dictionary = (item2.IsBackground ? UnitBackgroundTraitTiersId : UnitTraitTiersId);
			foreach (int cost in item2.Costs)
			{
				if (dictionary.ContainsKey(cost))
				{
					CLoggerManager.Log($"Cost {cost} is present in multiple trait tiers, this is unexpected.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitDatabase");
				}
				else
				{
					dictionary.Add(cost, item2.Id);
				}
			}
		}
		UnitTraitDefinitions = new Dictionary<string, UnitTraitDefinition>();
		SecondaryTraitIds = new List<string>();
		foreach (XElement item3 in xElement.Elements("UnitTraitDefinition"))
		{
			UnitTraitDefinition unitTraitDefinition = new UnitTraitDefinition(item3);
			UnitTraitDefinitions.Add(unitTraitDefinition.Id, unitTraitDefinition);
			if (!unitTraitDefinition.IsBackgroundTrait)
			{
				SecondaryTraitIds.Add(unitTraitDefinition.Id);
			}
		}
		SecondaryTraitIds.Sort(CompareTraitsByCost);
		UnitTraitGenerationDefinition = new UnitTraitGenerationDefinition(XDocument.Parse(unitTraitGenerationDefinition.text, LoadOptions.SetBaseUri));
	}

	protected override void Awake()
	{
		base.Awake();
		if (base._IsValid || SecondaryTraitCost == null)
		{
			InitializeSecondaryTraitCost();
		}
	}

	private void InitializeSecondaryTraitCost()
	{
		SecondaryTraitCost = new List<int>();
		foreach (string secondaryTraitId in SecondaryTraitIds)
		{
			if (!UnitTraitDefinitions.ContainsKey(secondaryTraitId))
			{
				CLoggerManager.Log("UnitTraitDefinitions doesn't contains this trait id : " + secondaryTraitId, LogType.Error, CLogLevel.MAJOR);
				continue;
			}
			int cost = UnitTraitDefinitions[secondaryTraitId].Cost;
			if (!SecondaryTraitCost.Contains(cost))
			{
				SecondaryTraitCost.Add(cost);
			}
		}
	}
}
