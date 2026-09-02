using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class ManaSpentTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "ManaSpent";

	public ManaSpentTrophyDefinition ManaSpentDefinition => base.TrophyConditionDefinition as ManaSpentTrophyDefinition;

	public ManaSpentTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
