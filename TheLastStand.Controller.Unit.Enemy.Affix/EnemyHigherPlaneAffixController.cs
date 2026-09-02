using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;

namespace TheLastStand.Controller.Unit.Enemy.Affix;

public class EnemyHigherPlaneAffixController : EnemyAffixController
{
	public class TargetingValidity
	{
		public ISkillCaster caster;

		public bool validity;

		public TargetingValidity(ISkillCaster newCaster, bool newValidity)
		{
			caster = newCaster;
			validity = newValidity;
		}
	}

	public EnemyHigherPlaneAffix EnemyHigherPlaneAffix => base.EnemyAffix as EnemyHigherPlaneAffix;

	public EnemyHigherPlaneAffixController(EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
	{
		base.EnemyAffix = new EnemyHigherPlaneAffix(this, enemyAffixDefinition, enemyUnit);
	}

	public override void Trigger(E_EffectTime effectTime, object data = null)
	{
		if (effectTime == E_EffectTime.OnTargetingComputation)
		{
			CheckTargetingValidity(data as TargetingValidity);
		}
	}

	private void CheckTargetingValidity(TargetingValidity targetingValidity)
	{
		targetingValidity.validity = targetingValidity.caster is PlayableUnit;
	}
}
