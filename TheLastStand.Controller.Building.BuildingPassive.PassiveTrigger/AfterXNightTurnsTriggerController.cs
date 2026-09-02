using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

public class AfterXNightTurnsTriggerController : PassiveTriggerController
{
	public AfterXNightTurnsTrigger AfterXNightTurnsTrigger => base.PassiveTrigger as AfterXNightTurnsTrigger;

	public AfterXNightTurnsTriggerController(SerializedAfterXNightTurnsTrigger container, AfterXNightTurnsTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new AfterXNightTurnsTrigger(container, definition, this);
	}

	public AfterXNightTurnsTriggerController(AfterXNightTurnsTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new AfterXNightTurnsTrigger(definition, this);
	}

	public override bool UpdateAndCheckTrigger(bool onLoad)
	{
		AfterXNightTurnsTrigger.NightTurnsBuffer++;
		return AfterXNightTurnsTrigger.NightTurnsBuffer == AfterXNightTurnsTrigger.AfterXNightTurnsTriggerDefinition.NumberOfNightTurns;
	}
}
