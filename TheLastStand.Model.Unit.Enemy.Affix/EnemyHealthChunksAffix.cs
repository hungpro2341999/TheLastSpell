using System.Collections.Generic;
using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Unit.Enemy.Affix;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public class EnemyHealthChunksAffix : EnemyAffix
{
	public EnemyHealthChunksAffixController EnemyHealthChunksAffixController => base.EnemyAffixController as EnemyHealthChunksAffixController;

	public EnemyHealthChunksAffixEffectDefinition EnemyHealthChunksAffixEffectDefinition => base.EnemyAffixDefinition.EnemyAffixEffectDefinition as EnemyHealthChunksAffixEffectDefinition;

	public float StepPercentage => EnemyHealthChunksAffixEffectDefinition.StepPercentage.EvalToFloat(Interpreter);

	public List<int> Steps { get; set; }

	public EnemyHealthChunksAffix(EnemyHealthChunksAffixController enemyAffixController, EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
		: base(enemyAffixController, enemyAffixDefinition, enemyUnit)
	{
	}
}
