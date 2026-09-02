using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class EnemiesKilledSingleAttackTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "EnemiesKilledSingleAttack";

	public EnemiesKilledSingleAttackTrophyDefinition EnemiesKilledSingleAttackTrophyDefinition => base.TrophyConditionDefinition as EnemiesKilledSingleAttackTrophyDefinition;

	public EnemiesKilledSingleAttackTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
