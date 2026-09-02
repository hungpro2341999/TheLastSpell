using TPLib;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Camera;

namespace TheLastStand.Controller.Building.BuildingAction;

public class RepelFogBuildingActionEffectController : BuildingActionEffectController
{
	public RepelFogBuildingActionEffect RepelFogBuildingActionEffect => base.BuildingActionEffect as RepelFogBuildingActionEffect;

	public RepelFogBuildingActionEffectController(RepelFogBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new RepelFogBuildingActionEffect(definition, this, productionBuilding);
	}

	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	public override void ExecuteActionEffect()
	{
		ACameraView.AllowUserPan = false;
		ACameraView.AllowUserZoom = false;
		TPSingleton<FogManager>.Instance.StartCoroutine(TPSingleton<FogManager>.Instance.MoveCameraToNearestFogWithWave(RepelFogAfterCameraMove, 1f));
	}

	private void RepelFogAfterCameraMove()
	{
		FogController.DecreaseDensity(refreshFog: true, RepelFogBuildingActionEffect.RepelFogBuildingActionEffectDefinition.Amount);
		TPSingleton<FogManager>.Instance.Fog.HasBeenRepelled = true;
		SpawnWaveManager.SpawnWaveView.Refresh();
		ACameraView.AllowUserPan = true;
		ACameraView.AllowUserZoom = true;
	}
}
