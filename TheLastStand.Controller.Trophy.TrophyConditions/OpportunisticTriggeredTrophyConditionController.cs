using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class OpportunisticTriggeredTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "OpportunisticTriggered";

	public OpportunisticTriggeredTrophyDefinition OpportunisticTriggeredTrophyDefinition => base.TrophyConditionDefinition as OpportunisticTriggeredTrophyDefinition;

	public OpportunisticTriggeredTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
