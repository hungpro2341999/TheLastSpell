using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public class EndOfProductionTrigger : PassiveTrigger
{
	public EndOfProductionTriggerController EndOfProductionTriggerController => base.PassiveTriggerController as EndOfProductionTriggerController;

	public EndOfProductionTriggerDefinition EndOfProductionTriggerDeginition => base.PassiveTriggerDefinition as EndOfProductionTriggerDefinition;

	public EndOfProductionTrigger(EndOfProductionTriggerDefinition passiveTriggerDefinition, EndOfProductionTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
	}
}
