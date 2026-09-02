using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class PunchUsedTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "PunchUsed";

	public PunchUsedTrophyDefinition PunchUsedTrophyDefinition => base.TrophyConditionDefinition as PunchUsedTrophyDefinition;

	public PunchUsedTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
