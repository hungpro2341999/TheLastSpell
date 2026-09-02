using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class TilesMovedBeforeMomentumTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "TilesMovedBeforeMomentum";

	public TilesMovedBeforeMomentumTrophyDefinition TilesMovedBeforeMomentumTrophyDefinition => base.TrophyConditionDefinition as TilesMovedBeforeMomentumTrophyDefinition;

	public TilesMovedBeforeMomentumTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
