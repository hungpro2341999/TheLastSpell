using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Unit.Enemy.Affix;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public class EnemyMistyAffix : EnemyAffix, ILightFogSupplier
{
	public EnemyMistyAffixController EnemyMistyAffixController => base.EnemyAffixController as EnemyMistyAffixController;

	public EnemyMistyAffixEffectDefinition EnemyMistyAffixEffectDefinition => base.EnemyAffixDefinition.EnemyAffixEffectDefinition as EnemyMistyAffixEffectDefinition;

	public bool CanLightFogExistOnSelf => EnemyMistyAffixEffectDefinition.CanLightFogExistOnSelf;

	public bool IsLightFogSupplierMoving { get; set; }

	public LightFogSupplierMoveDatas LightFogSupplierMoveDatas { get; set; } = new LightFogSupplierMoveDatas();

	public bool CanLightFogSupplierMove => true;

	public int Range => EnemyMistyAffixEffectDefinition.Range.EvalToInt(Interpreter);

	public EnemyMistyAffix(EnemyMistyAffixController enemyAffixController, EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
		: base(enemyAffixController, enemyAffixDefinition, enemyUnit)
	{
	}
}
