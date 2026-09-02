using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

public class AfterXProductionPhasesTriggerController : PassiveTriggerController
{
	public AfterXProductionPhasesTrigger AfterXProductionPhasesTrigger => base.PassiveTrigger as AfterXProductionPhasesTrigger;

	public AfterXProductionPhasesTriggerController(SerializedAfterXProductionPhasesTrigger container, AfterXProductionPhasesTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new AfterXProductionPhasesTrigger(container, definition, this);
	}

	public AfterXProductionPhasesTriggerController(AfterXProductionPhasesTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new AfterXProductionPhasesTrigger(definition, this);
	}

	public override bool UpdateAndCheckTrigger(bool onLoad)
	{
		AfterXProductionPhasesTrigger.ProductionPhasesBuffer++;
		return AfterXProductionPhasesTrigger.ProductionPhasesBuffer == AfterXProductionPhasesTrigger.AfterXProductionPhasesTriggerDefinition.NumberOfProductionPhases;
	}
}
