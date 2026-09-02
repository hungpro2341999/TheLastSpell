using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

public class OnConstructionTriggerController : PassiveTriggerController
{
	public OnConstructionTrigger OnConstructionTrigger => base.PassiveTrigger as OnConstructionTrigger;

	public OnConstructionTriggerController(OnConstructionTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new OnConstructionTrigger(definition, this);
	}
}
