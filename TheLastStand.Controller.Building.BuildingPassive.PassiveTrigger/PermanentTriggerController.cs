using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

public class PermanentTriggerController : PassiveTriggerController
{
	public PermanentTrigger PermanentTrigger => base.PassiveTrigger as PermanentTrigger;

	public PermanentTriggerController(PermanentTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new PermanentTrigger(definition, this);
	}

	public override bool UpdateAndCheckTrigger(bool OnLoad)
	{
		if (!OnLoad || PermanentTrigger.PermanentTriggerDeginition.TriggerOnLoad)
		{
			return base.UpdateAndCheckTrigger(OnLoad);
		}
		return false;
	}
}
