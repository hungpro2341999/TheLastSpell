using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class DefensesLostTrophyConditionController : ValueIntTrophyConditionController
{
	public override string Name => "DefensesLost";

	public DefensesLostTrophyDefinition DefensesLostDefinition => base.TrophyConditionDefinition as DefensesLostTrophyDefinition;

	public DefensesLostTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
