using TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Manager;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseCondition;

public class FinishWaveBossPhaseConditionController : ABossPhaseConditionController
{
	public FinishWaveBossPhaseConditionController(IBossPhaseConditionDefinition bossPhaseConditionDefinition)
		: base(bossPhaseConditionDefinition)
	{
	}

	public override bool IsValid()
	{
		return NightTurnsManager.IsSpawnWaveFinished(bossWaveVanquished: true);
	}
}
