using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public class AfterXProductionPhasesTrigger : PassiveTrigger
{
	public AfterXProductionPhasesTriggerController AfterXProductionPhasesTriggerController => base.PassiveTriggerController as AfterXProductionPhasesTriggerController;

	public AfterXProductionPhasesTriggerDefinition AfterXProductionPhasesTriggerDefinition => base.PassiveTriggerDefinition as AfterXProductionPhasesTriggerDefinition;

	public int ProductionPhasesBuffer { get; set; }

	public AfterXProductionPhasesTrigger(SerializedAfterXProductionPhasesTrigger container, AfterXProductionPhasesTriggerDefinition passiveTriggerDefinition, AfterXProductionPhasesTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
		Deserialize(container);
	}

	public AfterXProductionPhasesTrigger(AfterXProductionPhasesTriggerDefinition passiveTriggerDefinition, AfterXProductionPhasesTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
	}

	public void Deserialize(SerializedAfterXProductionPhasesTrigger container)
	{
		ProductionPhasesBuffer = container.ProductionPhasesSinceCreation;
	}

	public SerializedAfterXProductionPhasesTrigger Serialize()
	{
		return new SerializedAfterXProductionPhasesTrigger
		{
			ProductionPhasesSinceCreation = ProductionPhasesBuffer
		};
	}
}
