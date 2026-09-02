using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public class PermanentTrigger : PassiveTrigger
{
	public PermanentTriggerController PermanentTriggerController => base.PassiveTriggerController as PermanentTriggerController;

	public PermanentTriggerDefinition PermanentTriggerDeginition => base.PassiveTriggerDefinition as PermanentTriggerDefinition;

	public PermanentTrigger(PermanentTriggerDefinition passiveTriggerDefinition, PermanentTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
	}
}
