using TPLib;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Manager;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

public class StartOfNightEnemyTurnTriggerController : PassiveTriggerController
{
	public StartOfNightEnemyTurnTrigger StartOfNightEnemyTurnTrigger => base.PassiveTrigger as StartOfNightEnemyTurnTrigger;

	public StartOfNightEnemyTurnTriggerController(StartOfNightEnemyTurnTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new StartOfNightEnemyTurnTrigger(definition, this);
	}

	public override bool UpdateAndCheckTrigger(bool OnLoad)
	{
		return TPSingleton<GameManager>.Instance.Game.CurrentNightHour == 1;
	}
}
