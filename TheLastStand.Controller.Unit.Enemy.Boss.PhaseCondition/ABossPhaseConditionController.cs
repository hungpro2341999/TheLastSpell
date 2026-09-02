using TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseCondition;

public abstract class ABossPhaseConditionController
{
	protected IBossPhaseConditionDefinition BossPhaseConditionDefinition { get; }

	public ABossPhaseConditionController(IBossPhaseConditionDefinition bossPhaseConditionDefinition)
	{
		BossPhaseConditionDefinition = bossPhaseConditionDefinition;
	}

	public abstract bool IsValid();
}
