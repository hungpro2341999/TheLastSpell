using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Unit.Enemy.Affix;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public class EnemyRevengeAffix : EnemyAffix
{
	public EnemyRevengeAffixController EnemyRevengeAffixController => base.EnemyAffixController as EnemyRevengeAffixController;

	public EnemyRevengeAffixEffectDefinition EnemyRevengeAffixEffectDefinition => base.EnemyAffixDefinition.EnemyAffixEffectDefinition as EnemyRevengeAffixEffectDefinition;

	public EnemyRevengeAffix(EnemyRevengeAffixController enemyAffixController, EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
		: base(enemyAffixController, enemyAffixDefinition, enemyUnit)
	{
	}
}
