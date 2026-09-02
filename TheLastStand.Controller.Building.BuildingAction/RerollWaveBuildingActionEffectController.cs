using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller.Building.BuildingAction;

public class RerollWaveBuildingActionEffectController : BuildingActionEffectController
{
	public RerollWaveBuildingActionEffect RerollWaveBuildingActionEffect => base.BuildingActionEffect as RerollWaveBuildingActionEffect;

	public RerollWaveBuildingActionEffectController(RerollWaveBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new RerollWaveBuildingActionEffect(definition, this, productionBuilding);
	}

	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	public override void ExecuteActionEffect()
	{
		SpawnWaveManager.GenerateSpawnWave(isReroll: true);
		SpawnWaveManager.SpawnWaveView.Refresh();
	}
}
