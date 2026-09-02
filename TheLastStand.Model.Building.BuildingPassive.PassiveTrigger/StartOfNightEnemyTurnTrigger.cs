using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public class StartOfNightEnemyTurnTrigger : PassiveTrigger
{
	public StartOfNightEnemyTurnTriggerController StartOfNightEnemyTurnTriggerController => base.PassiveTriggerController as StartOfNightEnemyTurnTriggerController;

	public StartOfNightEnemyTurnTriggerDefinition StartOfNightEnemyTurnTriggerDefinition => base.PassiveTriggerDefinition as StartOfNightEnemyTurnTriggerDefinition;

	public StartOfNightEnemyTurnTrigger(StartOfNightEnemyTurnTriggerDefinition passiveTriggerDefinition, StartOfNightEnemyTurnTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
	}
}
