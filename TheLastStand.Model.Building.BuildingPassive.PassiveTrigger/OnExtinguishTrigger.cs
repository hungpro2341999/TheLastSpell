using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public class OnExtinguishTrigger : PassiveTrigger
{
	public OnExtinguishTriggerController OnExtinguishTriggerController => base.PassiveTriggerController as OnExtinguishTriggerController;

	public OnExtinguishTriggerDefinition OnExtinguishTriggerDefinition => base.PassiveTriggerDefinition as OnExtinguishTriggerDefinition;

	public OnExtinguishTrigger(OnExtinguishTriggerDefinition passiveTriggerDefinition, OnExtinguishTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
	}
}
