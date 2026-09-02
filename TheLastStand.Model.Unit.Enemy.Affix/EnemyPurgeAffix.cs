using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Unit.Enemy.Affix;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public class EnemyPurgeAffix : EnemyAffix
{
	public EnemyPurgeAffixController EnemyPurgeAffixController => base.EnemyAffixController as EnemyPurgeAffixController;

	public EnemyPurgeAffixEffectDefinition EnemyPurgeAffixEffectDefinition => base.EnemyAffixDefinition.EnemyAffixEffectDefinition as EnemyPurgeAffixEffectDefinition;

	public EnemyPurgeAffix(EnemyPurgeAffixController enemyAffixController, EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
		: base(enemyAffixController, enemyAffixDefinition, enemyUnit)
	{
	}
}
