using System.Collections.Generic;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Stat;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Controller.Unit.Stat;

public class EliteEnemyUnitStatsController : EnemyUnitStatsController
{
	public EliteEnemyUnitStats EliteEnemyUnitStats => base.UnitStats as EliteEnemyUnitStats;

	public EliteEnemyUnitStatsController(EliteEnemyUnit eliteEnemyUnit)
		: base(eliteEnemyUnit)
	{
	}

	public EliteEnemyUnitStatsController(SerializedEliteEnemyUnitStats serializedUnitStats, EliteEnemyUnit eliteEnemyUnit, int saveVersion)
		: base(eliteEnemyUnit)
	{
		eliteEnemyUnit.EliteEnemyUnitController.TriggerAffixes(E_EffectTime.OnStatsLoadStart, this);
		EliteEnemyUnitStats.Deserialize(serializedUnitStats, saveVersion);
	}

	public override void Init()
	{
		base.Init();
		ComputeEliteModifiers();
	}

	public new EliteEnemyUnitStat GetStat(UnitStatDefinition.E_Stat stat)
	{
		return base.GetStat(stat) as EliteEnemyUnitStat;
	}

	public override void InitStat(UnitStatDefinition.E_Stat stat, float value)
	{
		if (!base.UnitStats.Stats.ContainsKey(stat))
		{
			base.UnitStats.Stats.Add(stat, new EliteEnemyUnitStat(this, stat, UnitDatabase.UnitStatDefinitions[stat].Boundaries[base.UnitStats.UnitType]));
		}
		SetBaseStat(stat, value);
	}

	protected override void ComputeProgressionValue()
	{
		ComputeProgressionValue(EliteEnemyUnitStats.EliteEnemyUnit.EliteEnemyUnitTemplateDefinition.Id);
	}

	protected override void CreateModel(TheLastStand.Model.Unit.Unit unit)
	{
		base.UnitStats = new EliteEnemyUnitStats(this, unit as EliteEnemyUnit);
	}

	private void ComputeEliteModifiers()
	{
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, StatModifierDefinition> modifiedStat in EliteEnemyUnitStats.EliteEnemyUnit.EliteEnemyUnitTemplateDefinition.ModifiedStats)
		{
			GetStat(modifiedStat.Key).AffixStatModifier = new StatModifierDefinition(modifiedStat.Value.FlatModifier, modifiedStat.Value.PercentageModifier);
		}
	}
}
