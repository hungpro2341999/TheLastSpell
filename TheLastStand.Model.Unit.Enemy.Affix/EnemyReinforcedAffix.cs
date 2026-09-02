using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Unit.Enemy.Affix;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public class EnemyReinforcedAffix : EnemyAffix
{
	public EnemyReinforcedAffixController EnemyReinforcedAffixController => base.EnemyAffixController as EnemyReinforcedAffixController;

	public EnemyReinforcedAffixEffectDefinition EnemyReinforcedAffixEffectDefinition => base.EnemyAffixDefinition.EnemyAffixEffectDefinition as EnemyReinforcedAffixEffectDefinition;

	public EnemyReinforcedAffix(EnemyReinforcedAffixController enemyAffixController, EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
		: base(enemyAffixController, enemyAffixDefinition, enemyUnit)
	{
	}
}
