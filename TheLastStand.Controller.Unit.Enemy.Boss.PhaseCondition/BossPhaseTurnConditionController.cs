using TPLib;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseCondition;

public class BossPhaseTurnConditionController : ABossPhaseConditionController
{
	public BossPhaseTurnConditionDefinition BossPhaseTurnConditionDefinition => base.BossPhaseConditionDefinition as BossPhaseTurnConditionDefinition;

	public BossPhaseTurnConditionController(BossPhaseTurnConditionDefinition definition)
		: base(definition)
	{
	}

	public override bool IsValid()
	{
		return BossPhaseTurnConditionDefinition.Expression.EvalToInt(TPSingleton<BossManager>.Instance.CurrentBossPhase) == TPSingleton<GameManager>.Instance.Game.CurrentNightHour;
	}
}
