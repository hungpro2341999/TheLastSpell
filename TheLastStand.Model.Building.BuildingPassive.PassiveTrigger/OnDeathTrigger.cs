using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public class OnDeathTrigger : PassiveTrigger
{
	public OnDeathTriggerController OnDeathTriggerController => base.PassiveTriggerController as OnDeathTriggerController;

	public OnDeathTriggerDefinition OnDeathTriggerDefinition => base.PassiveTriggerDefinition as OnDeathTriggerDefinition;

	public OnDeathTrigger(OnDeathTriggerDefinition passiveTriggerDefinition, OnDeathTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
	}
}
