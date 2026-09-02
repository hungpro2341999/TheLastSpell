using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public class StartOfProductionTrigger : PassiveTrigger
{
	public StartOfProductionTriggerController StartOfProductionTriggerController => base.PassiveTriggerController as StartOfProductionTriggerController;

	public StartOfProductionTriggerDefinition StartOfProductionTriggerDeginition => base.PassiveTriggerDefinition as StartOfProductionTriggerDefinition;

	public StartOfProductionTrigger(StartOfProductionTriggerDefinition passiveTriggerDefinition, StartOfProductionTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
	}
}
