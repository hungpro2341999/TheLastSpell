using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class EnemiesKilledFromWatchtowerTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "EnemiesKilledFromWatchtower";

	public EnemiesKilledFromWatchtowerTrophyDefinition EnemiesKilledFromWatchtowerDefinition => base.TrophyConditionDefinition as EnemiesKilledFromWatchtowerTrophyDefinition;

	public EnemiesKilledFromWatchtowerTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
