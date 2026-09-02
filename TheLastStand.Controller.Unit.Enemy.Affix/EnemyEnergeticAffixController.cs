using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;

namespace TheLastStand.Controller.Unit.Enemy.Affix;

public class EnemyEnergeticAffixController : EnemyAffixController
{
	public EnemyEnergeticAffix EnemyEnergeticAffix => base.EnemyAffix as EnemyEnergeticAffix;

	public EnemyEnergeticAffixController(EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
	{
		base.EnemyAffix = new EnemyEnergeticAffix(this, enemyAffixDefinition, enemyUnit);
	}

	public override void Trigger(E_EffectTime effectTime, object data = null)
	{
	}
}
