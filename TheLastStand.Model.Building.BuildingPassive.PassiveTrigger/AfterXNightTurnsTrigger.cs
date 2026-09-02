using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public class AfterXNightTurnsTrigger : PassiveTrigger
{
	public AfterXNightTurnsTriggerController AfterXNightTurnsTriggerController => base.PassiveTriggerController as AfterXNightTurnsTriggerController;

	public AfterXNightTurnsTriggerDefinition AfterXNightTurnsTriggerDefinition => base.PassiveTriggerDefinition as AfterXNightTurnsTriggerDefinition;

	public int NightTurnsBuffer { get; set; }

	public AfterXNightTurnsTrigger(SerializedAfterXNightTurnsTrigger container, AfterXNightTurnsTriggerDefinition passiveTriggerDefinition, AfterXNightTurnsTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
		Deserialize(container);
	}

	public AfterXNightTurnsTrigger(AfterXNightTurnsTriggerDefinition passiveTriggerDefinition, AfterXNightTurnsTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
	}

	public void Deserialize(SerializedAfterXNightTurnsTrigger container)
	{
		NightTurnsBuffer = container.NightTurnsSinceCreation;
	}

	public SerializedAfterXNightTurnsTrigger Serialize()
	{
		return new SerializedAfterXNightTurnsTrigger
		{
			NightTurnsSinceCreation = NightTurnsBuffer
		};
	}
}
