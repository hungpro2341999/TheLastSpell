using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingPassive;

public class ImproveSpawnWaveInfo : BuildingPassiveEffect
{
	public ImproveSpawnWaveInfoDefinition ImproveSpawnWaveInfoDefinition => base.BuildingPassiveEffectDefinition as ImproveSpawnWaveInfoDefinition;

	public int UpgradedBonusValue { get; set; }

	public ImproveSpawnWaveInfo(PassivesModule buildingPassivesModule, ImproveSpawnWaveInfoDefinition buildingPassiveDefinition, ImproveSpawnWaveInfoController buildingPassiveController)
		: base(buildingPassivesModule, buildingPassiveDefinition, buildingPassiveController)
	{
	}
}
