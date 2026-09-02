using TPLib;
using TheLastStand.Definition.Tutorial;
using TheLastStand.Manager;

namespace TheLastStand.Controller.Tutorial;

public class CurrentNightHourTutorialConditionController : TutorialConditionController
{
	protected CurrentNightHourTutorialConditionDefinition CurrentNightHourConditionDefinition => base.ConditionDefinition as CurrentNightHourTutorialConditionDefinition;

	public CurrentNightHourTutorialConditionController(TutorialConditionDefinition conditionDefinition)
		: base(conditionDefinition)
	{
	}

	public override bool IsValid()
	{
		bool flag = TPSingleton<GameManager>.Instance.Game.CurrentNightHour == CurrentNightHourConditionDefinition.NightHour;
		if (!base.ConditionDefinition.Invert)
		{
			return flag;
		}
		return !flag;
	}
}
