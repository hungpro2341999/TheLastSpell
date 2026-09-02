using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Unit.Enemy.Affix;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public class EnemyHigherPlaneAffix : EnemyAffix
{
	public EnemyHigherPlaneAffixController EnemyHigherPlaneAffixController => base.EnemyAffixController as EnemyHigherPlaneAffixController;

	public EnemyHigherPlaneAffixEffectDefinition EnemyHigherPlaneAffixEffectDefinition => base.EnemyAffixDefinition.EnemyAffixEffectDefinition as EnemyHigherPlaneAffixEffectDefinition;

	public EnemyHigherPlaneAffix(EnemyHigherPlaneAffixController enemyAffixController, EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
		: base(enemyAffixController, enemyAffixDefinition, enemyUnit)
	{
	}
}
