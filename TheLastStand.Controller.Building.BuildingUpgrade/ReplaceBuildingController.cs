using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class ReplaceBuildingController : BuildingUpgradeEffectController
{
	public ReplaceBuilding ReplaceBuilding => base.BuildingUpgradeEffect as ReplaceBuilding;

	public ReplaceBuildingController(ReplaceBuildingDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ReplaceBuilding(definition, this, buildingUpgrade);
	}

	public override void TriggerEffect(bool onLoad = false)
	{
		BuildingManager.ReplaceBuilding(base.BuildingUpgradeEffect.BuildingUpgrade.Building.OriginTile, base.BuildingUpgradeEffect.BuildingUpgrade.Building, BuildingDatabase.BuildingDefinitions[ReplaceBuilding.ReplaceBuildingDefinition.NewBuildingId]);
	}
}
