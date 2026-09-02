using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingAction;

public class GainMaterialsBuildingActionEffect : BuildingActionEffect
{
	public GainMaterialsBuildingActionEffectController GainMaterialsBuildingActionController => base.BuildingActionEffectController as GainMaterialsBuildingActionEffectController;

	public GainMaterialsBuildingActionEffectDefinition GainMaterialsBuildingActionDefinition => base.BuildingActionEffectDefinition as GainMaterialsBuildingActionEffectDefinition;

	public GainMaterialsBuildingActionEffect(GainMaterialsBuildingActionEffectDefinition definition, GainMaterialsBuildingActionEffectController controller, ProductionModule productionBuilding)
		: base(definition, controller, productionBuilding)
	{
	}
}
