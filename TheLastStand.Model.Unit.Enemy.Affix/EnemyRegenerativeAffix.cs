using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy.Affix;
using UnityEngine;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public class EnemyRegenerativeAffix : EnemyAffix
{
	public EnemyRegenerativeAffixController EnemyRegenerativeAffixController => base.EnemyAffixController as EnemyRegenerativeAffixController;

	public EnemyRegenerativeAffixEffectDefinition EnemyRegenerativeAffixEffectDefinition => base.EnemyAffixDefinition.EnemyAffixEffectDefinition as EnemyRegenerativeAffixEffectDefinition;

	public float RegenerationValue => Mathf.RoundToInt(RegenerationPercentage / 100f * base.EnemyUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.HealthTotal).FinalClamped);

	public float RegenerationPercentage => EnemyRegenerativeAffixEffectDefinition.HealthTotalPercentage.EvalToFloat(Interpreter);

	public EnemyRegenerativeAffix(EnemyRegenerativeAffixController enemyAffixController, EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
		: base(enemyAffixController, enemyAffixDefinition, enemyUnit)
	{
	}
}
