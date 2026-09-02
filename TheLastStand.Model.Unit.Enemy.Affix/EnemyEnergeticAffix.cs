using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Unit.Enemy.Affix;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public class EnemyEnergeticAffix : EnemyAffix
{
	public EnemyEnergeticAffixController EnemyEnergeticAffixController => base.EnemyAffixController as EnemyEnergeticAffixController;

	public EnemyEnergeticAffixEffectDefinition EnemyEnergeticAffixEffectDefinition => base.EnemyAffixDefinition.EnemyAffixEffectDefinition as EnemyEnergeticAffixEffectDefinition;

	public EnemyEnergeticAffix(EnemyEnergeticAffixController enemyAffixController, EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
		: base(enemyAffixController, enemyAffixDefinition, enemyUnit)
	{
		base.EnemyUnit.TurnsToSkipOnSpawn *= 2;
	}
}
