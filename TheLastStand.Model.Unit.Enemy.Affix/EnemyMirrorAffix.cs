using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Unit.Enemy.Affix;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public class EnemyMirrorAffix : EnemyAffix
{
	public EnemyMirrorAffixController EnemyMirrorAffixController => base.EnemyAffixController as EnemyMirrorAffixController;

	public EnemyMirrorAffixEffectDefinition EnemyMirrorAffixEffectDefinition => base.EnemyAffixDefinition.EnemyAffixEffectDefinition as EnemyMirrorAffixEffectDefinition;

	public EnemyMirrorAffix(EnemyMirrorAffixController enemyAffixController, EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
		: base(enemyAffixController, enemyAffixDefinition, enemyUnit)
	{
	}

	public float ComputeDamage()
	{
		return EnemyMirrorAffixEffectDefinition.DamageValue.EvalToFloat(Interpreter);
	}
}
