using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy.Affix;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public class EnemyAuraAffix : EnemyAffix
{
	public EnemyAuraAffixController EnemyAuraAffixController => base.EnemyAffixController as EnemyAuraAffixController;

	public EnemyAuraAffixEffectDefinition EnemyAuraAffixEffectDefinition => base.EnemyAffixDefinition.EnemyAffixEffectDefinition as EnemyAuraAffixEffectDefinition;

	public int Range => EnemyAuraAffixEffectDefinition.Range.EvalToInt(Interpreter);

	public EnemyAuraAffix(EnemyAuraAffixController enemyAffixController, EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
		: base(enemyAffixController, enemyAffixDefinition, enemyUnit)
	{
	}

	public float GetBonusModifier(UnitStatDefinition.E_Stat stat)
	{
		return EnemyAuraAffixEffectDefinition.StatModifiers[stat].EvalToFloat(Interpreter);
	}
}
