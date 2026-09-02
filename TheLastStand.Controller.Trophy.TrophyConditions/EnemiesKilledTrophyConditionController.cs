using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class EnemiesKilledTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "EnemiesKilled";

	public EnemiesKilledTrophyDefinition EnemiesKilledDefinition => base.TrophyConditionDefinition as EnemiesKilledTrophyDefinition;

	public EnemiesKilledTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
