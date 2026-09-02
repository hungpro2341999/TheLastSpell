using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

public class StartOfProductionTriggerController : PassiveTriggerController
{
	public StartOfProductionTrigger StartOfProductionTrigger => base.PassiveTrigger as StartOfProductionTrigger;

	public StartOfProductionTriggerController(StartOfProductionTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new StartOfProductionTrigger(definition, this);
	}
}
