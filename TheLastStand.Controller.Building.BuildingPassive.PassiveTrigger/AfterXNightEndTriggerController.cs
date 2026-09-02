using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

public class AfterXNightEndTriggerController : PassiveTriggerController
{
	public AfterXNightEndTrigger AfterXNightEndTrigger => base.PassiveTrigger as AfterXNightEndTrigger;

	public AfterXNightEndTriggerController(SerializedAfterXNightEndTrigger container, AfterXNightEndTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new AfterXNightEndTrigger(container, definition, this);
	}

	public AfterXNightEndTriggerController(AfterXNightEndTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new AfterXNightEndTrigger(definition, this);
	}

	public override bool UpdateAndCheckTrigger(bool onLoad)
	{
		AfterXNightEndTrigger.NightEndBuffer++;
		return AfterXNightEndTrigger.NightEndBuffer == AfterXNightEndTrigger.AfterXNightEndTriggerDefinition.NumberOfNightEnd;
	}
}
