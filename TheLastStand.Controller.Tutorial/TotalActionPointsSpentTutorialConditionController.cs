using System.Linq;
using TPLib;
using TheLastStand.Definition.Tutorial;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit;

namespace TheLastStand.Controller.Tutorial;

public class TotalActionPointsSpentTutorialConditionController : TutorialConditionController
{
	protected TotalActionPointsSpentTutorialConditionDefinition TotalActionPointsSpentConditionDefinition => base.ConditionDefinition as TotalActionPointsSpentTutorialConditionDefinition;

	public TotalActionPointsSpentTutorialConditionController(TutorialConditionDefinition conditionDefinition)
		: base(conditionDefinition)
	{
	}

	public override bool IsValid()
	{
		int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Sum((PlayableUnit o) => o.ActionPointsSpentThisTurn);
		if (!base.ConditionDefinition.Invert)
		{
			return num >= TotalActionPointsSpentConditionDefinition.Value;
		}
		return num < TotalActionPointsSpentConditionDefinition.Value;
	}
}
