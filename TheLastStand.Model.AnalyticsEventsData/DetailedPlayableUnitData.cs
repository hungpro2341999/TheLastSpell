using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Item;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;

namespace TheLastStand.Model.AnalyticsEventsData;

[Serializable]
public class DetailedPlayableUnitData
{
	public class EquipmentData
	{
		[Serializable]
		public struct EquipmentStatData
		{
			[JsonProperty("stat_name")]
			public string Name;

			[JsonProperty("stat_value")]
			public float Value;
		}

		[JsonProperty("equipment_type")]
		public readonly string Type;

		[JsonProperty("equipment_name")]
		public readonly string Name;

		[JsonProperty("equipment_level")]
		public readonly int Level;

		[JsonProperty("equipment_rarity")]
		public readonly int Rarity;

		[JsonProperty("equipment_stats")]
		public readonly List<EquipmentStatData> Stats;

		public EquipmentData(TheLastStand.Model.Item.Item item)
		{
			Type = item.ItemDefinition.Category.ToString();
			Name = item.ItemDefinition.Id;
			Level = item.Level;
			Rarity = (int)item.Rarity;
			Stats = new List<EquipmentStatData>();
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item2 in item.GetAllStatBonusesMerged())
			{
				if (item2.Value != 0f)
				{
					Stats.Add(new EquipmentStatData
					{
						Name = item2.Key.ToString(),
						Value = item2.Value
					});
				}
			}
		}
	}

	public class PerkData
	{
		[JsonProperty("family_perk")]
		public readonly string Collection;

		[JsonProperty("perk_name")]
		public readonly string Name;

		[JsonProperty("unlocked")]
		public readonly bool Unlocked;

		public PerkData(Perk perk)
		{
			Collection = perk.CollectionId;
			Name = perk.PerkDefinition.Id;
			Unlocked = perk.Unlocked;
		}
	}

	[JsonProperty("hero_name")]
	public readonly string Id;

	[JsonProperty("hero_race")]
	public readonly string Race;

	[JsonProperty("hero_level")]
	public readonly int Level;

	[JsonProperty("is_starting_unit")]
	public readonly bool IsStartingUnit;

	[JsonProperty("equipment")]
	public readonly List<EquipmentData> Equipment;

	[JsonProperty("perks")]
	public readonly List<PerkData> Perks;

	public DetailedPlayableUnitData(PlayableUnit playableUnit)
	{
		Id = playableUnit.AnalyticsIdentifier;
		Race = playableUnit.RaceDefinition.Id;
		Level = (int)playableUnit.Level;
		IsStartingUnit = playableUnit.IsStartingUnit;
		Equipment = new List<EquipmentData>();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in playableUnit.EquipmentSlots)
		{
			foreach (EquipmentSlot item2 in equipmentSlot.Value)
			{
				TheLastStand.Model.Item.Item item = item2.Item;
				if (item != null)
				{
					Equipment.Add(new EquipmentData(item));
				}
			}
		}
		Perks = new List<PerkData>();
		foreach (UnitPerkTier unitPerkTier in playableUnit.PerkTree.UnitPerkTiers)
		{
			foreach (Perk perk in unitPerkTier.Perks)
			{
				if (perk != null)
				{
					Perks.Add(new PerkData(perk));
				}
			}
		}
	}
}
