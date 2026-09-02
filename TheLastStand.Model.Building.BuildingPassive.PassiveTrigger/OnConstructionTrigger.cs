using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public class OnConstructionTrigger : PassiveTrigger
{
	public OnConstructionTriggerController OnConstructionTriggerController => base.PassiveTriggerController as OnConstructionTriggerController;

	public OnConstructionTriggerDefinition OnConstructionTriggerDefinition => base.PassiveTriggerDefinition as OnConstructionTriggerDefinition;

	public OnConstructionTrigger(OnConstructionTriggerDefinition passiveTriggerDefinition, OnConstructionTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
	}
}
