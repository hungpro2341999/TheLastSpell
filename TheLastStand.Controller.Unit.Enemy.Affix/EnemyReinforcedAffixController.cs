using System.Collections.Generic;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;

namespace TheLastStand.Controller.Unit.Enemy.Affix;

public class EnemyReinforcedAffixController : EnemyAffixController
{
	public EnemyReinforcedAffix EnemyReinforcedAffix => base.EnemyAffix as EnemyReinforcedAffix;

	public EnemyReinforcedAffixController(EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
	{
		base.EnemyAffix = new EnemyReinforcedAffix(this, enemyAffixDefinition, enemyUnit);
	}

	public float GetBonusModifiers(UnitStatDefinition.E_Stat stat)
	{
		return EnemyReinforcedAffix.EnemyReinforcedAffixEffectDefinition.ModifiedStatsEveryXDay[stat].EvalToFloat(base.EnemyAffix.Interpreter);
	}

	public override void Trigger(E_EffectTime effectTime, object data = null)
	{
		if (effectTime == E_EffectTime.OnCreation || effectTime == E_EffectTime.OnStatsLoadStart)
		{
			ModifyAffixStatModifiers(data as EnemyUnitStatsController);
		}
	}

	private void ModifyAffixStatModifiers(EnemyUnitStatsController statsControllerOverride)
	{
		EnemyUnitStatsController enemyUnitStatsController = statsControllerOverride ?? base.EnemyAffix.EnemyUnit.EnemyUnitStatsController;
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, Node> item in EnemyReinforcedAffix.EnemyReinforcedAffixEffectDefinition.ModifiedStatsEveryXDay)
		{
			float bonusModifiers = GetBonusModifiers(item.Key);
			StatModifierDefinition affixStatModifier = enemyUnitStatsController.GetStat(item.Key).AffixStatModifier;
			if (affixStatModifier == null)
			{
				affixStatModifier = new StatModifierDefinition(bonusModifiers, 100f);
				enemyUnitStatsController.GetStat(item.Key).AffixStatModifier = affixStatModifier;
			}
			else
			{
				affixStatModifier.FlatModifier += bonusModifiers;
			}
		}
	}
}
