using TheLastStand.Definition.Tutorial;

namespace TheLastStand.Controller.Tutorial;

public abstract class TutorialConditionController
{
	protected TutorialConditionDefinition ConditionDefinition { get; }

	public TutorialConditionController(TutorialConditionDefinition conditionDefinition)
	{
		ConditionDefinition = conditionDefinition;
	}

	public abstract bool IsValid();
}
