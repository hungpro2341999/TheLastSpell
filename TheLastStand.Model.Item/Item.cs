using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.Skill;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.Database;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Item;
using UnityEngine;

namespace TheLastStand.Model.Item;

public class Item : ISkillContainer, IPerkUnlocker, ISerializable, IDeserializable
{
	public static class Constants
	{
		public static class Ids
		{
			public const string Beer = "Beer";
		}

		public const string ItemCategoryPathPrefix = "View/Sprites/UI/Items/Categories/Icon_ItemCategory_";

		public const string ItemHandsPathPrefix = "View/Sprites/UI/Items/Hands/Icon_ItemCategory_";

		public const string LevelRarityPathPrefix = "View/Sprites/UI/Items/Rarity/ItemBox_0";

		public const int LevelMax = 10;
	}

	public class StringToItemIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(ItemDatabase.ItemDefinitions.Keys);
	}

	public class StringToItemsListIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(ItemDatabase.ItemsListDefinitions.Keys);
	}

	public class StringToRarityProbabilityListIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(ItemDatabase.ItemRaritiesListDefinitions.Keys);
	}

	private ItemInterpreterContext itemInterpreterContext;

	public List<Affix> AdditionalAffixes { get; set; } = new List<Affix>();

	public List<AffixMalus> AdditionalAffixesMalus { get; set; } = new List<AffixMalus>();

	public ISkillCaster Holder
	{
		get
		{
			if (!(ItemSlot is EquipmentSlot equipmentSlot))
			{
				return null;
			}
			return equipmentSlot.PlayableUnit;
		}
	}

	public Vector2 BaseDamages => ItemDefinition.BaseDamageByLevel[Level];

	public Dictionary<UnitStatDefinition.E_Stat, float> BaseStatBonuses => ItemDefinition.BaseStatBonusesByLevel[Level];

	public int FinalPrice
	{
		get
		{
			int num = Mathf.RoundToInt(ItemDatabase.ItemPriceEquation.EvalToFloat(itemInterpreterContext));
			int num2 = 0;
			num2 += ResourceManager.ComputeExtraPercentageForCost(ResourceManager.E_PriceModifierType.Items, ResourceManager.E_ResourceType.Gold);
			return num + Mathf.RoundToInt((float)(num * num2) / 100f);
		}
	}

	public bool HasBeenSoldBefore { get; set; }

	public bool IsTwoHandedWeapon => ItemDefinition.Hands == ItemDefinition.E_Hands.TwoHands;

	public bool IsBeer => ItemDefinition.Id.Contains("Beer");

	public ItemController ItemController { get; }

	public ItemDefinition ItemDefinition { get; private set; }

	public ItemSlot ItemSlot { get; set; }

	public int Level { get; set; }

	public Tuple<UnitStatDefinition.E_Stat, float> MainStatBonusByLevel => ItemDefinition.MainStatBonusByLevel[Level];

	public string Name => ItemDefinition.BaseName + ((Level > 0) ? $" +{Level}" : "");

	public int Resistance { get; set; }

	public ItemDefinition.E_Rarity Rarity { get; set; }

	public string RarityName => Localizer.Get(string.Format("{0}{1}", "RarityName_", Rarity));

	public int DefaultSellingPrice => Mathf.RoundToInt(ItemDatabase.ItemPriceEquation.EvalToFloat(itemInterpreterContext));

	public Dictionary<string, Perk> Perks { get; } = new Dictionary<string, Perk>();

	public HashSet<string> PerksId { get; private set; } = new HashSet<string>();

	public int SellingPrice => Mathf.FloorToInt((float)DefaultSellingPrice * TPSingleton<BuildingManager>.Instance.Shop.SellingMultiplier / 100f);

	public List<TheLastStand.Model.Skill.Skill> Skills { get; } = new List<TheLastStand.Model.Skill.Skill>();

	public List<TheLastStand.Model.Skill.Skill> PerkReplacementSkills { get; } = new List<TheLastStand.Model.Skill.Skill>();

	public Dictionary<string, int> SkillsOverallUses => ItemDefinition.SkillsByLevel[Level];

	public Item(SerializedItem container, ItemController itemController, ItemSlot itemSlot)
	{
		ItemController = itemController;
		ItemSlot = itemSlot;
		itemInterpreterContext = new ItemInterpreterContext(this);
		Deserialize(container);
	}

	public Item(ItemDefinition itemDefinition, ItemController itemController)
	{
		ItemDefinition = itemDefinition;
		ItemController = itemController;
		itemInterpreterContext = new ItemInterpreterContext(this);
	}

	public Dictionary<UnitStatDefinition.E_Stat, float> GetAllStatBonusesMerged()
	{
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary = new Dictionary<UnitStatDefinition.E_Stat, float>();
		dictionary.Add(UnitStatDefinition.E_Stat.Resistance, Resistance);
		if (MainStatBonusByLevel != null)
		{
			dictionary.AddValueOrCreateKey(MainStatBonusByLevel.Item1, MainStatBonusByLevel.Item2, (float a, float b) => a + b);
		}
		if (BaseStatBonuses != null)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> baseStatBonuse in BaseStatBonuses)
			{
				dictionary.AddValueOrCreateKey(baseStatBonuse.Key, baseStatBonuse.Value, (float a, float b) => a + b);
			}
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item in ItemController.MergeAllAffixes())
		{
			dictionary.AddValueOrCreateKey(item.Key, item.Value, (float a, float b) => a + b);
		}
		return dictionary;
	}

	public TheLastStand.Model.Skill.Skill GetReplacementSkill(string skillId)
	{
		return PerkReplacementSkills.First((TheLastStand.Model.Skill.Skill replacementSkill) => replacementSkill.Id == skillId);
	}

	public TheLastStand.Model.Skill.Skill GetSkillOrReplacementSkillFromId(string skillId)
	{
		return Skills.Find((TheLastStand.Model.Skill.Skill aSkill) => aSkill.Id == skillId) ?? GetReplacementSkill(skillId);
	}

	public bool HasReplacementSkill(string skillId)
	{
		return PerkReplacementSkills.Any((TheLastStand.Model.Skill.Skill replacementSkill) => replacementSkill.Id == skillId);
	}

	public override string ToString()
	{
		string text = ItemDefinition.Id + "\n" + $"Category: {ItemDefinition.Category}\n" + $"Rarity: {Rarity}\n" + $"Level: {Level}";
		if (BaseStatBonuses != null)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> baseStatBonuse in BaseStatBonuses)
			{
				text += $"\nBase Stat Bonuses: {baseStatBonuse.Key} / {baseStatBonuse.Value} ";
			}
		}
		text += "\n";
		foreach (Affix additionalAffix in AdditionalAffixes)
		{
			text = text + "\nGenerated Affix: " + additionalAffix.AffixDefinition.Id;
		}
		text += "\n";
		foreach (AffixMalus additionalAffixesMalu in AdditionalAffixesMalus)
		{
			text += $"\nGenerated Malus Affix stat: {additionalAffixesMalu.AffixMalusDefinition.Stat}";
		}
		text += "\n";
		if (ItemDefinition.Resistance != Vector2Int.zero)
		{
			text += $"\n*Resistance:  {ItemDefinition.Resistance.x} - {ItemDefinition.Resistance.y}";
		}
		if (ItemDefinition.SkillsByLevel.TryGetValue(Level, out var value) && value != null)
		{
			foreach (KeyValuePair<string, int> item in value)
			{
				text = text + "\n*Skill " + item.Key;
				if (item.Value != -1)
				{
					text += $" (nb uses: {item.Value})";
				}
			}
		}
		if (ItemDefinition.PerksByLevel.TryGetValue(Level, out var value2) && value2 != null)
		{
			foreach (string item2 in value2)
			{
				text = text + "\n*Perk " + item2;
			}
		}
		return text + "\n";
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (!(container is SerializedItem serializedItem))
		{
			return;
		}
		try
		{
			ItemDefinition = ItemDatabase.ItemDefinitions[serializedItem.Id];
		}
		catch (KeyNotFoundException)
		{
			throw new Database<ItemDatabase>.MissingAssetException(serializedItem.Id);
		}
		Level = serializedItem.Level;
		Resistance = serializedItem.Resistance;
		Rarity = serializedItem.Rarity;
		HasBeenSoldBefore = serializedItem.HasBeenSoldBefore;
		if (ItemDefinition.PerksByLevel.ContainsKey(Level) && ItemDefinition.PerksByLevel[Level] != null)
		{
			foreach (string item in ItemDefinition.PerksByLevel[Level])
			{
				if (PlayableUnitDatabase.PerkDefinitions.ContainsKey(item) && !Perks.ContainsKey(item))
				{
					Perks.Add(item, new PerkController(PlayableUnitDatabase.PerkDefinitions[item], null, null, null, string.Empty, isNative: false, isFromRace: false).Perk);
				}
				PerksId.Add(item);
			}
		}
		DeserializePerkReplacementSkills(serializedItem.PerkReplacementSkills);
		DeserializeSkills(serializedItem.Skills);
		foreach (SerializedAffix affix in serializedItem.Affixes)
		{
			AdditionalAffixes.Add(new AffixController(affix).Affix);
		}
		foreach (SerializedAffixMalus affixesMalu in serializedItem.AffixesMalus)
		{
			AdditionalAffixesMalus.Add(new AffixMalusController(affixesMalu).AffixMalus);
		}
	}

	private void DeserializePerkReplacementSkills(List<SerializedSkill> replacementSkills)
	{
		if (replacementSkills == null || replacementSkills.Count == 0)
		{
			return;
		}
		foreach (SerializedSkill replacementSkill in replacementSkills)
		{
			PerkReplacementSkills.Add(new SkillController(replacementSkill, this).Skill);
		}
	}

	private void DeserializeSkills(List<SerializedSkill> skills)
	{
		if (ItemDefinition.SkillsByLevel[Level] == null)
		{
			return;
		}
		foreach (KeyValuePair<string, int> skillByLevel in ItemDefinition.SkillsByLevel[Level])
		{
			if (SkillDatabase.SkillDefinitions.TryGetValue(skillByLevel.Key, out var value))
			{
				if (skills.TryFind((SerializedSkill s) => s.Id == skillByLevel.Key, out var value2))
				{
					Skills.Add(new SkillController(value2, this).Skill);
					continue;
				}
				TheLastStand.Model.Skill.Skill skill = new SkillController(value, this, skillByLevel.Value).Skill;
				skill.SetUsesPerTurnRemaining(value.UsesPerTurnCount);
				Skills.Add(skill);
			}
		}
	}

	public ISerializedData Serialize()
	{
		return new SerializedItem
		{
			Id = ItemDefinition.Id,
			Level = Level,
			Resistance = Resistance,
			Rarity = Rarity,
			HasBeenSoldBefore = HasBeenSoldBefore,
			Skills = Skills.Select((TheLastStand.Model.Skill.Skill o) => o.Serialize() as SerializedSkill).ToList(),
			PerkReplacementSkills = PerkReplacementSkills.Select((TheLastStand.Model.Skill.Skill o) => o.Serialize() as SerializedSkill).ToList(),
			Affixes = AdditionalAffixes.Select((Affix o) => o.Serialize() as SerializedAffix).ToList(),
			AffixesMalus = AdditionalAffixesMalus.Select((AffixMalus o) => o.Serialize() as SerializedAffixMalus).ToList()
		};
	}
}
