using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingAction;

public class RerollWaveBuildingActionEffect : BuildingActionEffect
{
	public RerollWaveBuildingActionEffectController RerollWaveBuildingActionEffectController => base.BuildingActionEffectController as RerollWaveBuildingActionEffectController;

	public RerollWaveBuildingActionEffectDefinition RerollWaveBuildingActionEffectDefinition => base.BuildingActionEffectDefinition as RerollWaveBuildingActionEffectDefinition;

	public RerollWaveBuildingActionEffect(RerollWaveBuildingActionEffectDefinition definition, RerollWaveBuildingActionEffectController controller, ProductionModule productionBuilding)
		: base(definition, controller, productionBuilding)
	{
	}
}
