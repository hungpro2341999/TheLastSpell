using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Unit.Enemy.Affix;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public class EnemyBarrierAffix : EnemyAffix
{
	public EnemyBarrierAffixController EnemyBarrierAffixController => base.EnemyAffixController as EnemyBarrierAffixController;

	public EnemyBarrierAffixEffectDefinition EnemyBarrierAffixEffectDefinition => base.EnemyAffixDefinition.EnemyAffixEffectDefinition as EnemyBarrierAffixEffectDefinition;

	public EnemyBarrierAffix(EnemyBarrierAffixController enemyAffixController, EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
		: base(enemyAffixController, enemyAffixDefinition, enemyUnit)
	{
	}
}
