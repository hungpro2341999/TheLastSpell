using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public abstract class BuildingUpgradeEffect
{
	public BuildingUpgrade BuildingUpgrade { get; private set; }

	public BuildingUpgradeEffectController BuildingUpgradeEffectController { get; private set; }

	public BuildingUpgradeEffectDefinition BuildingUpgradeEffectDefinition { get; private set; }

	public BuildingUpgradeEffect(BuildingUpgradeEffectDefinition definition, BuildingUpgradeEffectController controller, BuildingUpgrade buildingUpgrade)
	{
		BuildingUpgradeEffectDefinition = definition;
		BuildingUpgradeEffectController = controller;
		BuildingUpgrade = buildingUpgrade;
	}
}
