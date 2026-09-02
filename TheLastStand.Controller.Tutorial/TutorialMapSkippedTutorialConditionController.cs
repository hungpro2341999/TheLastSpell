using TheLastStand.Definition.Tutorial;
using TheLastStand.Manager;

namespace TheLastStand.Controller.Tutorial;

public class TutorialMapSkippedTutorialConditionController : TutorialConditionController
{
	public TutorialMapSkippedTutorialConditionController(TutorialConditionDefinition conditionDefinition)
		: base(conditionDefinition)
	{
	}

	public override bool IsValid()
	{
		bool tutorialSkipped = ApplicationManager.Application.TutorialSkipped;
		if (!base.ConditionDefinition.Invert)
		{
			return tutorialSkipped;
		}
		return !tutorialSkipped;
	}
}
