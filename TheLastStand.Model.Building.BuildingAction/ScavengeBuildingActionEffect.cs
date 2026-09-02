using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingAction;

public class ScavengeBuildingActionEffect : BuildingActionEffect
{
	public ScavengeBuildingActionEffectController ScavengeBuildingActionController => base.BuildingActionEffectController as ScavengeBuildingActionEffectController;

	public ScavengeBuildingActionEffectDefinition ScavengeBuildingActionDefinition => base.BuildingActionEffectDefinition as ScavengeBuildingActionEffectDefinition;

	public ScavengeBuildingActionEffect(ScavengeBuildingActionEffectDefinition definition, ScavengeBuildingActionEffectController controller, ProductionModule productionBuilding)
		: base(definition, controller, productionBuilding)
	{
	}
}
