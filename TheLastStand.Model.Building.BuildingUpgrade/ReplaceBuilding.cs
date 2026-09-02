using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class ReplaceBuilding : BuildingUpgradeEffect
{
	public ReplaceBuildingController ReplaceBuildingController => base.BuildingUpgradeEffectController as ReplaceBuildingController;

	public ReplaceBuildingDefinition ReplaceBuildingDefinition => base.BuildingUpgradeEffectDefinition as ReplaceBuildingDefinition;

	public ReplaceBuilding(ReplaceBuildingDefinition definition, ReplaceBuildingController controller, BuildingUpgrade buildingUpgrade)
		: base(definition, controller, buildingUpgrade)
	{
	}
}
