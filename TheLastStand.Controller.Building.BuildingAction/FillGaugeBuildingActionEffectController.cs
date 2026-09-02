using TPLib;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller.Building.BuildingAction;

public class FillGaugeBuildingActionEffectController : BuildingActionEffectController
{
	public FillGaugeBuildingActionEffect FillGaugeBuildingActionEffect => base.BuildingActionEffect as FillGaugeBuildingActionEffect;

	public FillGaugeBuildingActionEffectController(FillGaugeBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new FillGaugeBuildingActionEffect(definition, this, productionBuilding);
	}

	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	public override void ExecuteActionEffect()
	{
		TPSingleton<BuildingManager>.Instance.Log($"Filling building gauge for {base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id} by {FillGaugeBuildingActionEffect.FillGaugeBuildingActionDefinition.Amount} units.");
		base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingController.ProductionModuleController.AddProductionUnits(FillGaugeBuildingActionEffect.FillGaugeBuildingActionDefinition.Amount);
	}
}
