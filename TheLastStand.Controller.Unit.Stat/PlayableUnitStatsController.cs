using System.Collections.Generic;
using TPLib;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.PlayableUnitGeneration;
using TheLastStand.Definition.Unit.Race;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Item;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.Model.Unit.Stat;
using TheLastStand.Serialization.Unit;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Stat;

public class PlayableUnitStatsController : UnitStatsController
{
	public PlayableUnitStats PlayableUnitStats => base.UnitStats as PlayableUnitStats;

	public PlayableUnitStatsController(PlayableUnit playableUnit)
		: base(playableUnit)
	{
		RandomizeGenerationStats();
	}

	public PlayableUnitStatsController(SerializedUnitStats serializedUnitStats, PlayableUnit playableUnit)
		: base(playableUnit)
	{
	}

	public override void Init()
	{
		InitStats();
		LinkParentStats();
		LinkChildStats();
	}

	public override void InitStat(UnitStatDefinition.E_Stat stat, float value)
	{
		if (!base.UnitStats.Stats.ContainsKey(stat))
		{
			base.UnitStats.Stats.Add(stat, new PlayableUnitStat(this, stat, UnitDatabase.UnitStatDefinitions[stat].Boundaries[base.UnitStats.UnitType]));
		}
		SetBaseStat(stat, value);
	}

	public override float ComputeStatBonus(UnitStatDefinition.E_Stat stat, bool withoutStatus = false)
	{
		return base.ComputeStatBonus(stat, withoutStatus) + GetStat(stat).Race + GetStat(stat).Traits + GetStat(stat).Perks + GetStat(stat).Equipment;
	}

	public new PlayableUnitStat GetStat(UnitStatDefinition.E_Stat stat)
	{
		return base.GetStat(stat) as PlayableUnitStat;
	}

	public override float IncreaseBaseStat(UnitStatDefinition.E_Stat stat, float value, bool includeChildStat, bool refreshHud = true)
	{
		PlayableUnitStat stat2 = GetStat(stat);
		float num = stat2.Base;
		SetBaseStat(stat, GetStat(stat).Base + value, includeChildStat, refreshHud);
		value -= Mathf.Max(0f, GetStat(stat).Base - num);
		int num2 = IncreasePerkGaugeStat(stat2, value);
		return GetStat(stat).Base + (float)num2 - num;
	}

	public int IncreasePerkGaugeStat(PlayableUnitStat playableUnitStat, float value)
	{
		int num = 0;
		foreach (GaugeModule perkGaugeModule in playableUnitStat.PerkGaugeModules)
		{
			if (value <= (float)num)
			{
				break;
			}
			if (perkGaugeModule.IsActive && perkGaugeModule.GaugeValue != perkGaugeModule.GaugeMaxValue)
			{
				int gaugeValue = perkGaugeModule.GaugeValue;
				perkGaugeModule.ValueOffset -= (int)value;
				num += perkGaugeModule.GaugeValue - gaugeValue;
			}
		}
		return num;
	}

	public override float DecreaseBaseStat(UnitStatDefinition.E_Stat stat, float value, bool includeChildStat, bool refreshHud = true)
	{
		PlayableUnitStat stat2 = GetStat(stat);
		float num = stat2.Base;
		value -= (float)DecreasePerkGaugeStat(stat2, value);
		SetBaseStat(stat, num - value, includeChildStat, refreshHud);
		return num - GetStat(stat).Base;
	}

	public int DecreasePerkGaugeStat(PlayableUnitStat playableUnitStat, float value)
	{
		int num = 0;
		foreach (GaugeModule perkGaugeModule in playableUnitStat.PerkGaugeModules)
		{
			if (value <= (float)num)
			{
				break;
			}
			if (perkGaugeModule.IsActive && perkGaugeModule.GaugeValue > 0)
			{
				int gaugeValue = perkGaugeModule.GaugeValue;
				perkGaugeModule.ValueOffset += (int)value;
				num += gaugeValue - perkGaugeModule.GaugeValue;
			}
		}
		return num;
	}

	public void IncreaseEquipmentStat(UnitStatDefinition.E_Stat statType, float value, bool includeChildStat)
	{
		PlayableUnitStat stat = GetStat(statType);
		float finalClamped = stat.FinalClamped;
		stat.Equipment += value;
		if (includeChildStat)
		{
			UnitStatDefinition.E_Stat childStatIfExists = UnitDatabase.UnitStatDefinitions[statType].GetChildStatIfExists();
			if (statType != childStatIfExists)
			{
				float num = stat.FinalClamped - finalClamped;
				SetBaseStat(childStatIfExists, GetStat(childStatIfExists).Base + num);
			}
		}
	}

	public void DecreaseEquipmentStat(UnitStatDefinition.E_Stat stat, float value, bool includeChildStat)
	{
		IncreaseEquipmentStat(stat, 0f - value, includeChildStat);
	}

	public void OnRaceGenerated(RaceDefinition raceDefinition)
	{
		for (int num = raceDefinition.StatModifiers.Count - 1; num >= 0; num--)
		{
			GetStat(raceDefinition.StatModifiers[num].Stat).Race += raceDefinition.StatModifiers[num].Value;
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, Vector2Int> statBoundaryModifier in raceDefinition.StatBoundaryModifiers)
		{
			GetStat(statBoundaryModifier.Key).RaceBoundaryModifier += (Vector2)statBoundaryModifier.Value;
		}
	}

	public void OnRaceRemoved(RaceDefinition raceDefinition)
	{
		for (int num = raceDefinition.StatModifiers.Count - 1; num >= 0; num--)
		{
			GetStat(raceDefinition.StatModifiers[num].Stat).Race -= raceDefinition.StatModifiers[num].Value;
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, Vector2Int> statBoundaryModifier in raceDefinition.StatBoundaryModifiers)
		{
			GetStat(statBoundaryModifier.Key).RaceBoundaryModifier -= (Vector2)statBoundaryModifier.Value;
		}
	}

	public void OnTraitGenerated(UnitTraitDefinition unitTraitDefinition)
	{
		for (int num = unitTraitDefinition.StatModifiers.Count - 1; num >= 0; num--)
		{
			GetStat(unitTraitDefinition.StatModifiers[num].Stat).Traits += unitTraitDefinition.StatModifiers[num].Value;
		}
	}

	public void OnTraitRemoved(UnitTraitDefinition unitTraitDefinition)
	{
		for (int num = unitTraitDefinition.StatModifiers.Count - 1; num >= 0; num--)
		{
			GetStat(unitTraitDefinition.StatModifiers[num].Stat).Traits -= unitTraitDefinition.StatModifiers[num].Value;
		}
	}

	public void OnItemEquiped(TheLastStand.Model.Item.Item item, bool onLoad = false)
	{
		bool flag = false;
		GetStat(UnitStatDefinition.E_Stat.Resistance).Equipment += item.Resistance;
		if (item.MainStatBonusByLevel != null)
		{
			IncreaseEquipmentStat(item.MainStatBonusByLevel.Item1, item.MainStatBonusByLevel.Item2, !onLoad);
			flag |= item.MainStatBonusByLevel.Item1 == UnitStatDefinition.E_Stat.Health || item.MainStatBonusByLevel.Item1 == UnitStatDefinition.E_Stat.HealthTotal;
		}
		if (item.BaseStatBonuses != null)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> baseStatBonuse in item.BaseStatBonuses)
			{
				IncreaseEquipmentStat(baseStatBonuse.Key, baseStatBonuse.Value, !onLoad);
				flag |= baseStatBonuse.Key == UnitStatDefinition.E_Stat.Health || baseStatBonuse.Key == UnitStatDefinition.E_Stat.HealthTotal;
			}
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item2 in item.ItemController.MergeAllAffixes())
		{
			IncreaseEquipmentStat(item2.Key, item2.Value, !onLoad);
			flag |= item2.Key == UnitStatDefinition.E_Stat.Health || item2.Key == UnitStatDefinition.E_Stat.HealthTotal;
		}
		SaveFromGearDeath();
		if (flag && !onLoad)
		{
			base.UnitStats.Unit.UnitController.UpdateInjuryStage();
		}
	}

	public void OnItemUnequiped(TheLastStand.Model.Item.Item item, bool onLoad = false)
	{
		bool flag = false;
		GetStat(UnitStatDefinition.E_Stat.Resistance).Equipment -= item.Resistance;
		if (item.MainStatBonusByLevel != null)
		{
			DecreaseEquipmentStat(item.MainStatBonusByLevel.Item1, item.MainStatBonusByLevel.Item2, !onLoad);
			flag |= item.MainStatBonusByLevel.Item1 == UnitStatDefinition.E_Stat.Health || item.MainStatBonusByLevel.Item1 == UnitStatDefinition.E_Stat.HealthTotal;
		}
		if (item.BaseStatBonuses != null)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> baseStatBonuse in item.BaseStatBonuses)
			{
				DecreaseEquipmentStat(baseStatBonuse.Key, baseStatBonuse.Value, !onLoad);
				flag |= baseStatBonuse.Key == UnitStatDefinition.E_Stat.Health || baseStatBonuse.Key == UnitStatDefinition.E_Stat.HealthTotal;
			}
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item2 in item.ItemController.MergeAllAffixes())
		{
			DecreaseEquipmentStat(item2.Key, item2.Value, !onLoad);
			flag |= item2.Key == UnitStatDefinition.E_Stat.Health || item2.Key == UnitStatDefinition.E_Stat.HealthTotal;
		}
		SaveFromGearDeath();
		if (flag)
		{
			base.UnitStats.Unit.UnitController.UpdateInjuryStage();
		}
	}

	public void OnStatModifierUpgradeUnlocked(UnitAttributeModifierMetaEffectDefinition unitAttributeModifierDefinition)
	{
		if (PlayableUnitStats.PlayableUnit.ArchetypeId != unitAttributeModifierDefinition.Archetype && !unitAttributeModifierDefinition.AllArchetypes)
		{
			return;
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, Vector2> item in unitAttributeModifierDefinition.StatAndValue)
		{
			if (PlayableUnitStats.Stats.ContainsKey(item.Key))
			{
				IncreaseBaseStat(item.Key, item.Value.y, includeChildStat: true);
			}
		}
	}

	public void RefreshEquipmentValues()
	{
		PlayableUnitStat.AllowPerkRefreshInjuries = false;
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, UnitStat> stat in PlayableUnitStats.Stats)
		{
			(stat.Value as PlayableUnitStat).Equipment = 0f;
		}
		PlayableUnitStats.ArmorMasterAffectedItems = 0;
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnitStats.PlayableUnit.EquipmentSlots)
		{
			foreach (EquipmentSlot item in equipmentSlot.Value)
			{
				if (item.Item != null)
				{
					OnItemEquiped(item.Item, onLoad: true);
				}
			}
		}
		PlayableUnitStat.AllowPerkRefreshInjuries = true;
	}

	public void OnLevelUpStatValidated(UnitLevelUp unitLevelUp)
	{
		IncreaseBaseStat(unitLevelUp.SelectedStat.Definition.Stat, unitLevelUp.SelectedStat.Definition.Bonuses[unitLevelUp.SelectedStat.BonusIndex], includeChildStat: true);
		if (unitLevelUp.SelectedStat.Definition.Stat == UnitStatDefinition.E_Stat.HealthTotal)
		{
			base.UnitStats.Unit.UnitController.UpdateInjuryStage();
		}
	}

	protected override void CreateModel(TheLastStand.Model.Unit.Unit unit)
	{
		base.UnitStats = new PlayableUnitStats(this, unit as PlayableUnit);
	}

	protected override void InitStats()
	{
		PlayableUnitGenerationDefinition playableUnitGenerationDefinition = PlayableUnitDatabase.PlayableUnitGenerationDefinitions[PlayableUnitStats.PlayableUnit.ArchetypeId];
		InitStat(UnitStatDefinition.E_Stat.HealthTotal, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.HealthTotal].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.HealthRegen, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.HealthRegen].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.Health, 0f);
		InitStat(UnitStatDefinition.E_Stat.ManaTotal, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.ManaTotal].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.ManaRegen, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.ManaRegen].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.Mana, 0f);
		InitStat(UnitStatDefinition.E_Stat.MovePointsTotal, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.MovePointsTotal].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.MovePoints, 0f);
		InitStat(UnitStatDefinition.E_Stat.ActionPointsTotal, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.ActionPointsTotal].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.ActionPoints, 0f);
		InitStat(UnitStatDefinition.E_Stat.ArmorTotal, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.ArmorTotal].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.Armor, 0f);
		InitStat(UnitStatDefinition.E_Stat.PhysicalDamage, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.PhysicalDamage].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.RangedDamage, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.RangedDamage].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.MagicalDamage, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.MagicalDamage].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.Dodge, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.Dodge].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.Critical, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.Critical].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.InjuryDamageMultiplier, 100f);
		InitStat(UnitStatDefinition.E_Stat.Resistance, 0f);
		InitStat(UnitStatDefinition.E_Stat.Block, 0f);
		InitStat(UnitStatDefinition.E_Stat.CriticalPower, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.CriticalPower].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.HealingReceived, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.HealingReceived].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.SkillRangeModifier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.SkillRangeModifier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.ExperienceGainMultiplier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.ExperienceGainMultiplier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.OverallDamage, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.OverallDamage].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.Reliability, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.Reliability].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.Accuracy, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.Accuracy].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.StunChanceModifier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.StunChanceModifier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.PropagationBouncesModifier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.PropagationBouncesModifier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.PropagationDamage, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.PropagationDamage].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.ResistanceReduction, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.ResistanceReduction].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.MomentumAttacks, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.MomentumAttacks].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.OpportunisticAttacks, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.OpportunisticAttacks].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.IsolatedAttacks, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.IsolatedAttacks].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.ArmorShreddingAttacks, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.ArmorShreddingAttacks].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.PoisonDamageModifier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.PoisonDamageModifier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.MultiHitsCountModifier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.MultiHitsCountModifier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.StunResistance, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.StunResistance].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.PoisonDurationModifier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.PoisonDurationModifier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.DebuffDurationModifier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.DebuffDurationModifier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.BuffDurationModifier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.BuffDurationModifier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.PotionRangeModifier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.PotionRangeModifier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.PercentageResistanceReduction].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.BonusSkillUses, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.BonusSkillUses].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.BonusUsableItemsUses, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.BonusUsableItemsUses].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.StunDurationModifier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.StunDurationModifier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.ContagionDurationModifier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.ContagionDurationModifier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.GauntletRangeModifier, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.GauntletRangeModifier].GetBoundaries().x);
		InitStat(UnitStatDefinition.E_Stat.BonusMainHandSkillUses, playableUnitGenerationDefinition.StatGenerationDefinitions[UnitStatDefinition.E_Stat.BonusMainHandSkillUses].GetBoundaries().x);
	}

	private void RandomizeGenerationStats()
	{
		PlayableUnitGenerationDefinition playableUnitGenerationDefinition = PlayableUnitDatabase.PlayableUnitGenerationDefinitions[PlayableUnitStats.PlayableUnit.ArchetypeId];
		UnitStatDefinition.E_Stat[] array = new UnitStatDefinition.E_Stat[29]
		{
			UnitStatDefinition.E_Stat.HealthTotal,
			UnitStatDefinition.E_Stat.ManaTotal,
			UnitStatDefinition.E_Stat.ManaRegen,
			UnitStatDefinition.E_Stat.MovePointsTotal,
			UnitStatDefinition.E_Stat.HealthRegen,
			UnitStatDefinition.E_Stat.PhysicalDamage,
			UnitStatDefinition.E_Stat.RangedDamage,
			UnitStatDefinition.E_Stat.MagicalDamage,
			UnitStatDefinition.E_Stat.Dodge,
			UnitStatDefinition.E_Stat.Critical,
			UnitStatDefinition.E_Stat.CriticalPower,
			UnitStatDefinition.E_Stat.HealingReceived,
			UnitStatDefinition.E_Stat.SkillRangeModifier,
			UnitStatDefinition.E_Stat.ExperienceGainMultiplier,
			UnitStatDefinition.E_Stat.OverallDamage,
			UnitStatDefinition.E_Stat.Reliability,
			UnitStatDefinition.E_Stat.Accuracy,
			UnitStatDefinition.E_Stat.StunChanceModifier,
			UnitStatDefinition.E_Stat.PropagationBouncesModifier,
			UnitStatDefinition.E_Stat.PropagationDamage,
			UnitStatDefinition.E_Stat.ResistanceReduction,
			UnitStatDefinition.E_Stat.MomentumAttacks,
			UnitStatDefinition.E_Stat.OpportunisticAttacks,
			UnitStatDefinition.E_Stat.IsolatedAttacks,
			UnitStatDefinition.E_Stat.ArmorShreddingAttacks,
			UnitStatDefinition.E_Stat.PoisonDamageModifier,
			UnitStatDefinition.E_Stat.MultiHitsCountModifier,
			UnitStatDefinition.E_Stat.StunResistance,
			UnitStatDefinition.E_Stat.ArmorTotal
		};
		for (int num = array.Length - 1; num >= 0; num--)
		{
			UnitStat stat = GetStat(array[num]);
			float randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, playableUnitGenerationDefinition.StatGenerationDefinitions[stat.StatId].GetBoundaries().x, playableUnitGenerationDefinition.StatGenerationDefinitions[stat.StatId].GetBoundaries().y);
			SetBaseStat(stat.StatId, Mathf.Round(randomRange));
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item in TPSingleton<GlyphManager>.Instance.PlayableUnitsStatsToModify)
		{
			IncreaseBaseStat(item.Key, item.Value, includeChildStat: true);
		}
	}

	private void SaveFromGearDeath()
	{
		PlayableUnitStat stat = GetStat(UnitStatDefinition.E_Stat.Health);
		if (stat.FinalClamped < 1f)
		{
			SetBaseStat(UnitStatDefinition.E_Stat.Health, 0f - stat.Perks + 1f);
		}
	}
}
