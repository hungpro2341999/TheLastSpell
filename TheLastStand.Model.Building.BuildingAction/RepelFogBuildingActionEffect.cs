using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingAction;

public class RepelFogBuildingActionEffect : BuildingActionEffect
{
	public RepelFogBuildingActionEffectController RepelFogBuildingActionEffectController => base.BuildingActionEffectController as RepelFogBuildingActionEffectController;

	public RepelFogBuildingActionEffectDefinition RepelFogBuildingActionEffectDefinition => base.BuildingActionEffectDefinition as RepelFogBuildingActionEffectDefinition;

	public RepelFogBuildingActionEffect(RepelFogBuildingActionEffectDefinition definition, RepelFogBuildingActionEffectController controller, ProductionModule productionBuilding)
		: base(definition, controller, productionBuilding)
	{
	}
}
