using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class SpeedyKilledWithoutDodgingTrophyConditionController : ValueIntTrophyConditionController
{
	public override string Name => "SpeedyKilledWithoutDodging";

	public SpeedyKilledWithoutDodgingTrophyDefinition SpeedyKilledWithoutDodgingDefinition => base.TrophyConditionDefinition as SpeedyKilledWithoutDodgingTrophyDefinition;

	public SpeedyKilledWithoutDodgingTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 1)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type SpeedyKilledWithoutDodging need only 1 argument", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is EnemyUnit enemyUnit))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type SpeedyKilledWithoutDodging should have as first argument an EnemyUnit !", CLogLevel.DETAILED);
			return;
		}
		if (!enemyUnit.EnemyUnitController.EnemyUnit.HasDodged)
		{
			base.ValueProgression++;
		}
		OnValueChange();
	}
}
