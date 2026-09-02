using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class EnemiesDamagedByBoomerTrophyConditionController : ValueIntTrophyConditionController
{
	public override string Name => "EnemiesDamagedByBoomer";

	public EnemiesDamagedByBoomerTrophyDefinition EnemiesDamagedByBoomerTrophyDefinition => base.TrophyConditionDefinition as EnemiesDamagedByBoomerTrophyDefinition;

	public EnemiesDamagedByBoomerTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void SetValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 1)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDamagedByBoomer need only 1 argument", CLogLevel.DETAILED);
		}
		else if (!(args[0] is EnemyUnit enemyUnit))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDamagedByBoomer should have as first argument a Model.Unit.Enemy.EnemyUnit !", CLogLevel.DETAILED);
		}
		else if (enemyUnit.EnemyUnitController.EnemyUnit.AlliesDamaged > base.ValueProgression)
		{
			base.ValueProgression = enemyUnit.EnemyUnitController.EnemyUnit.AlliesDamaged;
			OnValueChange();
		}
	}

	public override string ToString()
	{
		if (base.ValueProgression < EnemiesDamagedByBoomerTrophyDefinition.Value)
		{
			return "No Boomer killed fulfilling the conditions.";
		}
		return $"A Boomer was killed and his explosion damaged at least {EnemiesDamagedByBoomerTrophyDefinition.Value} enemies !";
	}

	protected override void CheckCompleteState()
	{
		isCompleted = base.ValueProgression >= EnemiesDamagedByBoomerTrophyDefinition.Value;
	}
}
