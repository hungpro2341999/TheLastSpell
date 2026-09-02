using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public class AfterXNightEndTrigger : PassiveTrigger
{
	public AfterXNightEndTriggerController AfterXNightEndTriggerController => base.PassiveTriggerController as AfterXNightEndTriggerController;

	public AfterXNightEndTriggerDefinition AfterXNightEndTriggerDefinition => base.PassiveTriggerDefinition as AfterXNightEndTriggerDefinition;

	public int NightEndBuffer { get; set; }

	public AfterXNightEndTrigger(SerializedAfterXNightEndTrigger container, AfterXNightEndTriggerDefinition passiveTriggerDefinition, AfterXNightEndTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
		Deserialize(container);
	}

	public AfterXNightEndTrigger(AfterXNightEndTriggerDefinition passiveTriggerDefinition, AfterXNightEndTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
	}

	public void Deserialize(SerializedAfterXNightEndTrigger container)
	{
		NightEndBuffer = container.NightEndSinceCreation;
	}

	public SerializedAfterXNightEndTrigger Serialize()
	{
		return new SerializedAfterXNightEndTrigger
		{
			NightEndSinceCreation = NightEndBuffer
		};
	}
}
