using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class BodyArmorBuffUsedTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "BodyArmorBuffUsed";

	public BodyArmorBuffUsedTrophyDefinition BodyArmorBuffUsedDefinition => base.TrophyConditionDefinition as BodyArmorBuffUsedTrophyDefinition;

	public BodyArmorBuffUsedTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
