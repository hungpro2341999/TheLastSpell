using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingAction;

public class FillGaugeBuildingActionEffect : BuildingActionEffect
{
	public FillGaugeBuildingActionEffectController FillGaugeBuildingActionController => base.BuildingActionEffectController as FillGaugeBuildingActionEffectController;

	public FillGaugeBuildingActionEffectDefinition FillGaugeBuildingActionDefinition => base.BuildingActionEffectDefinition as FillGaugeBuildingActionEffectDefinition;

	public FillGaugeBuildingActionEffect(FillGaugeBuildingActionEffectDefinition definition, FillGaugeBuildingActionEffectController controller, ProductionModule productionBuilding)
		: base(definition, controller, productionBuilding)
	{
	}
}
