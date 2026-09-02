using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class BuildingsLostTrophyConditionController : ValueIntTrophyConditionController
{
	public override string Name => "BuildingsLost";

	public BuildingsLostTrophyDefinition BuildingsLostTrophyDefinition => base.TrophyConditionDefinition as BuildingsLostTrophyDefinition;

	public BuildingsLostTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
