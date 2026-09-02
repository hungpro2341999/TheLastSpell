using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class DodgesPerformedTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "DodgesPerformed";

	public DodgesPerformedTrophyDefinition DodgesPerformedDefinition => base.TrophyConditionDefinition as DodgesPerformedTrophyDefinition;

	public DodgesPerformedTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
