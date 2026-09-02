using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

public class EndOfProductionTriggerController : PassiveTriggerController
{
	public EndOfProductionTrigger EndOfProductionTrigger => base.PassiveTrigger as EndOfProductionTrigger;

	public EndOfProductionTriggerController(EndOfProductionTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new EndOfProductionTrigger(definition, this);
	}
}
