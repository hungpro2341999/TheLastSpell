using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public abstract class PassiveTrigger
{
	public PassiveTriggerController PassiveTriggerController { get; protected set; }

	public PassiveTriggerDefinition PassiveTriggerDefinition { get; protected set; }

	public PassiveTrigger(PassiveTriggerDefinition passiveTriggerDefinition, PassiveTriggerController passiveTriggerController)
	{
		PassiveTriggerDefinition = passiveTriggerDefinition;
		PassiveTriggerController = passiveTriggerController;
	}
}
