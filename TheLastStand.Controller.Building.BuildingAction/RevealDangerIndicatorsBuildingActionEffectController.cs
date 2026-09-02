using TPLib;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller.Building.BuildingAction;

public class RevealDangerIndicatorsBuildingActionEffectController : BuildingActionEffectController
{
	public RevealDangerIndicatorsBuildingActionEffect RevealWaveEnemiesRatioBuildingActionEffect => base.BuildingActionEffect as RevealDangerIndicatorsBuildingActionEffect;

	public RevealDangerIndicatorsBuildingActionEffectController(RevealDangerIndicatorsBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new RevealDangerIndicatorsBuildingActionEffect(definition, this, productionBuilding);
	}

	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	public override void ExecuteActionEffect()
	{
		TPSingleton<SpawnWaveManager>.Instance.SetDetailedSpawnWaveArrows(state: true);
	}
}
