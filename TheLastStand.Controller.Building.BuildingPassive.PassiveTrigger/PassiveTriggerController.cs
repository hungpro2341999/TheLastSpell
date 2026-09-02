using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

public abstract class PassiveTriggerController
{
	public TheLastStand.Model.Building.BuildingPassive.PassiveTrigger.PassiveTrigger PassiveTrigger { get; protected set; }

	public PassiveTriggerController(PassiveTriggerDefinition definition)
	{
	}

	public virtual bool UpdateAndCheckTrigger(bool OnLoad)
	{
		return true;
	}
}
