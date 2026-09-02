using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingAction;

public class HealManaBuildingActionEffect : BuildingActionEffect
{
	public HealManaBuildingActionEffectController HealManaBuildingActionController => base.BuildingActionEffectController as HealManaBuildingActionEffectController;

	public HealManaBuildingActionEffectDefinition HealManaBuildingActionDefinition => base.BuildingActionEffectDefinition as HealManaBuildingActionEffectDefinition;

	public HealManaBuildingActionEffect(HealManaBuildingActionEffectDefinition definition, HealManaBuildingActionEffectController controller, ProductionModule productionBuilding)
		: base(definition, controller, productionBuilding)
	{
	}
}
