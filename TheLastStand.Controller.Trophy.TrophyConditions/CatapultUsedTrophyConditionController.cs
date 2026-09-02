using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class CatapultUsedTrophyConditionController : ValueIntTrophyConditionController
{
	public override string Name => "CatapultUsed";

	public CatapultUsedTrophyDefinition CatapultUsedTrophyDefinition => base.TrophyConditionDefinition as CatapultUsedTrophyDefinition;

	public CatapultUsedTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
