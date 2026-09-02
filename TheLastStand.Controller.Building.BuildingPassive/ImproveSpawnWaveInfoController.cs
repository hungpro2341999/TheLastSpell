using TPLib;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class ImproveSpawnWaveInfoController : BuildingPassiveEffectController
{
	public ImproveSpawnWaveInfo ImproveSpawnWaveInfo => base.BuildingPassiveEffect as ImproveSpawnWaveInfo;

	public ImproveSpawnWaveInfoController(PassivesModule buildingPassivesModule, ImproveSpawnWaveInfoDefinition buildingDefinition)
	{
		base.BuildingPassiveEffect = new ImproveSpawnWaveInfo(buildingPassivesModule, buildingDefinition, this);
	}

	public override void Apply()
	{
		TPSingleton<SpawnWaveManager>.Instance.OnSeerBuiltOrDestroyed(built: true);
		SpawnWaveManager.CurrentSpawnWave?.SpawnWaveView.Refresh();
	}

	public override void ImproveEffect(int bonus)
	{
		ImproveSpawnWaveInfo.UpgradedBonusValue += bonus;
		SpawnWaveManager.CurrentSpawnWave?.SpawnWaveView.Refresh();
	}

	public override void Unapply()
	{
		TPSingleton<SpawnWaveManager>.Instance.OnSeerBuiltOrDestroyed(built: false);
	}
}
