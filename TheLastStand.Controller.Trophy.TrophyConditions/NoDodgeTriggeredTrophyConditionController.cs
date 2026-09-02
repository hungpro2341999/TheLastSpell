using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class NoDodgeTriggeredTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "NoDodgeTriggered";

	public NoDodgeTriggeredTrophyDefinition NoDodgeTriggeredDefinition => base.TrophyConditionDefinition as NoDodgeTriggeredTrophyDefinition;

	public NoDodgeTriggeredTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
