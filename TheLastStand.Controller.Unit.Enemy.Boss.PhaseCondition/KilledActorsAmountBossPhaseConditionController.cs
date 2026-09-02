using TPLib;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Manager.Unit;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseCondition;

public class KilledActorsAmountBossPhaseConditionController : ABossPhaseConditionController
{
	public KilledActorsAmountBossPhaseConditionDefinition KilledActorsAmountBossPhaseConditionDefinition => base.BossPhaseConditionDefinition as KilledActorsAmountBossPhaseConditionDefinition;

	public KilledActorsAmountBossPhaseConditionController(KilledActorsAmountBossPhaseConditionDefinition bossPhaseConditionDefinition)
		: base(bossPhaseConditionDefinition)
	{
	}

	public override bool IsValid()
	{
		if (TPSingleton<BossManager>.Instance.BossPhaseActorsKills.TryGetValue(KilledActorsAmountBossPhaseConditionDefinition.ActorId, out var value))
		{
			return value >= KilledActorsAmountBossPhaseConditionDefinition.Amount;
		}
		return false;
	}
}
