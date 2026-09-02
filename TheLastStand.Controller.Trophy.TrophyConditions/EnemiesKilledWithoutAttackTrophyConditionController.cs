using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class EnemiesKilledWithoutAttackTrophyConditionController : ValueIntTrophyConditionController
{
	public override string Name => "EnemiesKilledWithoutAttack";

	public EnemiesKilledWithoutAttackTrophyDefinition EnemiesKilledWithoutAttackDefinition => base.TrophyConditionDefinition as EnemiesKilledWithoutAttackTrophyDefinition;

	public EnemiesKilledWithoutAttackTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
