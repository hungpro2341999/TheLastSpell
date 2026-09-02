using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingAction;

public class GainGoldBuildingActionEffect : BuildingActionEffect
{
	public GainGoldBuildingActionEffectController GainGoldBuildingActionController => base.BuildingActionEffectController as GainGoldBuildingActionEffectController;

	public GainGoldBuildingActionEffectDefinition GainGoldBuildingActionDefinition => base.BuildingActionEffectDefinition as GainGoldBuildingActionEffectDefinition;

	public GainGoldBuildingActionEffect(GainGoldBuildingActionEffectDefinition definition, GainGoldBuildingActionEffectController controller, ProductionModule productionBuilding)
		: base(definition, controller, productionBuilding)
	{
	}
}
