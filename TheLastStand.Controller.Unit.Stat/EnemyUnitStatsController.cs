using System.Collections.Generic;
using TPLib;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Stat;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Controller.Unit.Stat;

public class EnemyUnitStatsController : UnitStatsController
{
	public EnemyUnitStats EnemyUnitStats => base.UnitStats as EnemyUnitStats;

	public EnemyUnitStatsController(EnemyUnit enemyUnit)
		: base(enemyUnit)
	{
	}

	public EnemyUnitStatsController(SerializedEnemyUnitStats serializedUnitStats, EnemyUnit enemyUnit, int saveVersion)
		: base(enemyUnit)
	{
		enemyUnit.EnemyUnitController.TriggerAffixes(E_EffectTime.OnStatsLoadStart, this);
		EnemyUnitStats.Deserialize(serializedUnitStats, saveVersion);
	}

	public override void Init()
	{
		InitStats();
		ComputeApocalypseStatBaseValueModifier();
		ComputeProgressionValue();
		LinkParentStats();
		LinkChildStats();
	}

	public override void InitStat(UnitStatDefinition.E_Stat stat, float value)
	{
		if (!base.UnitStats.Stats.ContainsKey(stat))
		{
			base.UnitStats.Stats.Add(stat, new EnemyUnitStat(this, stat, UnitDatabase.UnitStatDefinitions[stat].Boundaries[base.UnitStats.UnitType]));
		}
		SetBaseStat(stat, value);
	}

	public override void InitStat(ISerializedData container)
	{
		SerializedEnemyUnitStat serializedEnemyUnitStat = container as SerializedEnemyUnitStat;
		InitStat(serializedEnemyUnitStat.Stat.StatId, serializedEnemyUnitStat.Stat.Base);
	}

	public void InitStatsModifiersFromApocalypse()
	{
		if (ApocalypseManager.CurrentApocalypse == null)
		{
			return;
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, int> item in ApocalypseManager.CurrentApocalypse.ActiveStatModifierFromTurn)
		{
			GetStat(item.Key).Apocalypse += item.Value;
		}
	}

	public override float ComputeStatBonus(UnitStatDefinition.E_Stat stat, bool withoutStatus = false)
	{
		return base.ComputeStatBonus(stat, withoutStatus) + GetStat(stat).Progression;
	}

	public new EnemyUnitStat GetStat(UnitStatDefinition.E_Stat stat)
	{
		return base.GetStat(stat) as EnemyUnitStat;
	}

	protected virtual void ComputeProgressionValue(string id)
	{
		int initialDay = 1 - (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.EnemiesProgressionOffset + ApocalypseManager.CurrentApocalypse.EnemiesProgressionOffset);
		foreach (EnemyUnitTemplateDefinition.StatProgression statProgression in EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.StatProgressions)
		{
			float num = EnemyUnitStat.ComputeStatProgression(initialDay, statProgression.IncreaseEveryXDay, increaseOnFirstDay: true, statProgression.Value, statProgression.Delay, 0f, statProgression.MaxIncreases);
			GetStat(statProgression.Id).Progression += num;
		}
	}

	protected virtual void ComputeProgressionValue()
	{
		ComputeProgressionValue(EnemyUnitStats.EnemyUnit.Id);
	}

	protected void ComputeApocalypseStatBaseValueModifier()
	{
		if (!ApocalypseManager.CurrentApocalypse.EnemiesStatsBaseValueModifiers.TryGetValue(EnemyUnitStats.EnemyUnit.Id, out var value))
		{
			return;
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item in value)
		{
			GetStat(item.Key).ApocalypseBaseValueStatModifier += item.Value;
		}
	}

	protected override void CreateModel(TheLastStand.Model.Unit.Unit unit)
	{
		base.UnitStats = new EnemyUnitStats(this, unit as EnemyUnit);
	}

	protected override void InitStats()
	{
		InitStat(UnitStatDefinition.E_Stat.HealthTotal, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.HealthTotal.Min);
		InitStat(UnitStatDefinition.E_Stat.Health, 0f);
		InitStat(UnitStatDefinition.E_Stat.ArmorTotal, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.ArmorTotal.Min);
		InitStat(UnitStatDefinition.E_Stat.Armor, 0f);
		InitStat(UnitStatDefinition.E_Stat.MovePointsTotal, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.MovePointsTotal.Min);
		InitStat(UnitStatDefinition.E_Stat.MovePoints, 0f);
		InitStat(UnitStatDefinition.E_Stat.PhysicalDamage, 100f);
		InitStat(UnitStatDefinition.E_Stat.MagicalDamage, 100f);
		InitStat(UnitStatDefinition.E_Stat.RangedDamage, 100f);
		InitStat(UnitStatDefinition.E_Stat.Dodge, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.Dodge.Min);
		InitStat(UnitStatDefinition.E_Stat.Critical, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.Critical.Min);
		InitStat(UnitStatDefinition.E_Stat.CriticalPower, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.CriticalPower.Min);
		InitStat(UnitStatDefinition.E_Stat.Resistance, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.Resistance.Min);
		InitStat(UnitStatDefinition.E_Stat.HealthRegen, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.HealthRegen.Min);
		InitStat(UnitStatDefinition.E_Stat.Block, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.Block.Min);
		InitStat(UnitStatDefinition.E_Stat.Panic, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.Panic.Min);
		InitStat(UnitStatDefinition.E_Stat.OverallDamage, 0f);
		InitStat(UnitStatDefinition.E_Stat.InjuryDamageMultiplier, 100f);
		InitStat(UnitStatDefinition.E_Stat.EnemyEvolutionDamageMultiplier, 100f);
		InitStat(UnitStatDefinition.E_Stat.StunResistance, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.StunResistance.Min);
		InitStat(UnitStatDefinition.E_Stat.HealingReceived, 100f);
		InitStat(UnitStatDefinition.E_Stat.SkillRangeModifier, 0f);
		InitStat(UnitStatDefinition.E_Stat.Reliability, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.Reliability.Min);
		InitStat(UnitStatDefinition.E_Stat.Accuracy, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.Accuracy.Min);
		InitStat(UnitStatDefinition.E_Stat.StunChanceModifier, 0f);
		InitStat(UnitStatDefinition.E_Stat.ResistanceReduction, 0f);
		InitStat(UnitStatDefinition.E_Stat.MomentumAttacks, 0f);
		InitStat(UnitStatDefinition.E_Stat.OpportunisticAttacks, 100f);
		InitStat(UnitStatDefinition.E_Stat.IsolatedAttacks, 100f);
		InitStat(UnitStatDefinition.E_Stat.ArmorShreddingAttacks, 0f);
		InitStat(UnitStatDefinition.E_Stat.PoisonDamageModifier, 100f);
		InitStat(UnitStatDefinition.E_Stat.DamnedSoulsEarned, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.DamnedSoulsEarned.Min);
		InitStat(UnitStatDefinition.E_Stat.ExperienceGain, EnemyUnitStats.EnemyUnit.EnemyUnitTemplateDefinition.ExperienceGain.Min);
		InitStat(UnitStatDefinition.E_Stat.PoisonDurationModifier, 0f);
		InitStat(UnitStatDefinition.E_Stat.DebuffDurationModifier, 0f);
		InitStat(UnitStatDefinition.E_Stat.BuffDurationModifier, 0f);
		InitStat(UnitStatDefinition.E_Stat.PotionRangeModifier, 0f);
		InitStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction, 0f);
		InitStat(UnitStatDefinition.E_Stat.BonusSkillUses, 0f);
		InitStat(UnitStatDefinition.E_Stat.BonusUsableItemsUses, 0f);
		InitStat(UnitStatDefinition.E_Stat.StunDurationModifier, 0f);
		InitStat(UnitStatDefinition.E_Stat.ContagionDurationModifier, 0f);
		InitStat(UnitStatDefinition.E_Stat.GauntletRangeModifier, 0f);
		InitStat(UnitStatDefinition.E_Stat.BonusMainHandSkillUses, 0f);
	}
}
