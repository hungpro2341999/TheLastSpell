using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class OpportunismDamageInflictedTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "OpportunismDamageInflicted";

	public OpportunismDamageInflictedTrophyDefinition OpportunismDamageInflictedTrophyDefinition => base.TrophyConditionDefinition as OpportunismDamageInflictedTrophyDefinition;

	public OpportunismDamageInflictedTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
