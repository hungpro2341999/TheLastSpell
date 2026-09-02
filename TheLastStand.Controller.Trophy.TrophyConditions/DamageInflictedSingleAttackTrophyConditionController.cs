using TPLib;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class DamageInflictedSingleAttackTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "DamageInflictedSingleAttack";

	public DamageInflictedSingleAttackTrophyDefinition DamageInflictedSingleAttackTrophyDefinition => base.TrophyConditionDefinition as DamageInflictedSingleAttackTrophyDefinition;

	public DamageInflictedSingleAttackTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		TPSingleton<TrophyManager>.Instance.LogWarning("Tried to call AppendValue within " + Name + ". By design, this should not happen! Skipping");
	}
}
