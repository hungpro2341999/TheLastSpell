using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingAction;

public class HealBuildingActionEffect : BuildingActionEffect
{
	public HealBuildingActionEffectController HealBuildingActionController => base.BuildingActionEffectController as HealBuildingActionEffectController;

	public HealBuildingActionEffectDefinition HealBuildingActionDefinition => base.BuildingActionEffectDefinition as HealBuildingActionEffectDefinition;

	public HealBuildingActionEffect(HealBuildingActionEffectDefinition definition, HealBuildingActionEffectController controller, ProductionModule productionBuilding)
		: base(definition, controller, productionBuilding)
	{
	}
}
