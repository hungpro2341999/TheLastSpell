using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

public class OnDeathTriggerController : PassiveTriggerController
{
	public OnDeathTrigger OnDeathTrigger => base.PassiveTrigger as OnDeathTrigger;

	public OnDeathTriggerController(OnDeathTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new OnDeathTrigger(definition, this);
	}
}
