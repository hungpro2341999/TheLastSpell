using System.Linq;
using TPLib;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseCondition;

public class RemainingActorsBossPhaseConditionController : ABossPhaseConditionController
{
	public RemainingActorsBossPhaseConditionDefinition RemainingActorsBossPhaseConditionDefinition => base.BossPhaseConditionDefinition as RemainingActorsBossPhaseConditionDefinition;

	public RemainingActorsBossPhaseConditionController(RemainingActorsBossPhaseConditionDefinition remainingActorsBossPhaseConditionDefinition)
		: base(remainingActorsBossPhaseConditionDefinition)
	{
	}

	public override bool IsValid()
	{
		if (TPSingleton<BossManager>.Instance.BossPhaseActors.TryGetValue(RemainingActorsBossPhaseConditionDefinition.ActorId, out var value))
		{
			return value.Count(delegate(IBossPhaseActor x)
			{
				if (x is IDamageable damageable)
				{
					return !damageable.IsDead;
				}
				if (x is TheLastStand.Model.Building.Building building)
				{
					DamageableModule damageableModule = building.DamageableModule;
					return damageableModule == null || !damageableModule.IsDead;
				}
				return true;
			}) == RemainingActorsBossPhaseConditionDefinition.Amount;
		}
		return false;
	}
}
