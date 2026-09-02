using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Model;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;

namespace TheLastStand.Controller.Unit.Enemy.Affix;

public class EnemyBarrierAffixController : EnemyAffixController
{
	public class StatusValidity
	{
		public TheLastStand.Model.Status.Status.E_StatusType statusType;

		public bool validity;

		public StatusValidity(TheLastStand.Model.Status.Status.E_StatusType newStatusType, bool newValidity)
		{
			statusType = newStatusType;
			validity = newValidity;
		}
	}

	public EnemyBarrierAffix EnemyBarrierAffix => base.EnemyAffix as EnemyBarrierAffix;

	public EnemyBarrierAffixController(EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
	{
		base.EnemyAffix = new EnemyBarrierAffix(this, enemyAffixDefinition, enemyUnit);
	}

	public override void Trigger(E_EffectTime effectTime, object data = null)
	{
		if (effectTime == E_EffectTime.OnStatusImmunityComputation)
		{
			CheckStatusValidity(data as StatusValidity);
		}
	}

	private void CheckStatusValidity(StatusValidity statusValidity)
	{
		statusValidity.validity = (statusValidity.statusType & EnemyBarrierAffix.EnemyBarrierAffixEffectDefinition.StatusType) != statusValidity.statusType;
	}
}
