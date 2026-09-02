using TPLib;
using TheLastStand.Definition.Tutorial;
using TheLastStand.Manager;

namespace TheLastStand.Controller.Tutorial;

public class DuringDayTurnTutorialConditionController : TutorialConditionController
{
	protected DuringDayTurnTutorialConditionDefinition DuringDayTurnConditionDefinition => base.ConditionDefinition as DuringDayTurnTutorialConditionDefinition;

	public DuringDayTurnTutorialConditionController(TutorialConditionDefinition conditionDefinition)
		: base(conditionDefinition)
	{
	}

	public override bool IsValid()
	{
		bool flag = TPSingleton<GameManager>.Instance.Game.DayTurn == DuringDayTurnConditionDefinition.DayTurn;
		if (!base.ConditionDefinition.Invert)
		{
			return flag;
		}
		return !flag;
	}
}
