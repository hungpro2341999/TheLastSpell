using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class EnemiesKilledByPropagationTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "EnemiesKilledByPropagation";

	public EnemiesKilledByPropagationTrophyDefinition EnemiesKilledByPropagationTrophyDefinition => base.TrophyConditionDefinition as EnemiesKilledByPropagationTrophyDefinition;

	public EnemiesKilledByPropagationTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
