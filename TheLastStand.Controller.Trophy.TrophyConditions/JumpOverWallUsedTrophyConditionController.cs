using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class JumpOverWallUsedTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "JumpOverWallUsed";

	public JumpOverWallUsedTrophyDefinition JumpOverWallUsedTrophyDefinition => base.TrophyConditionDefinition as JumpOverWallUsedTrophyDefinition;

	public JumpOverWallUsedTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
