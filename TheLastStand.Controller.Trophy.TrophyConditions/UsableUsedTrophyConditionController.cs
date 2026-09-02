using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class UsableUsedTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "UsableUsed";

	public UsableUsedTrophyDefinition UsableUsedDefinition => base.TrophyConditionDefinition as UsableUsedTrophyDefinition;

	public UsableUsedTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
