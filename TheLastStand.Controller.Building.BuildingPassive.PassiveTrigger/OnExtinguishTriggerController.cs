using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

public class OnExtinguishTriggerController : PassiveTriggerController
{
	public OnDeathTrigger OnDeathTrigger => base.PassiveTrigger as OnDeathTrigger;

	public OnExtinguishTriggerController(OnExtinguishTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new OnExtinguishTrigger(definition, this);
	}
}
