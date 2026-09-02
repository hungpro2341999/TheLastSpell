using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class EnemiesKilledByIsolatedTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "EnemiesKilledByIsolated";

	public EnemiesKilledByIsolatedTrophyDefinition EnemiesKilledByIsolatedTrophyDefinition => base.TrophyConditionDefinition as EnemiesKilledByIsolatedTrophyDefinition;

	public EnemiesKilledByIsolatedTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
