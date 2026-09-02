using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class TilesMovedUsingSkillsTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "TilesMovedUsingSkills";

	public TilesMovedUsingSkillsTrophyDefinition TilesMovedUsingSkillsTrophyDefinition => base.TrophyConditionDefinition as TilesMovedUsingSkillsTrophyDefinition;

	public TilesMovedUsingSkillsTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
