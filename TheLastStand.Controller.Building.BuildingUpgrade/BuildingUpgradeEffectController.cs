using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public abstract class BuildingUpgradeEffectController
{
	public BuildingUpgradeEffect BuildingUpgradeEffect { get; protected set; }

	public abstract void TriggerEffect(bool onLoad = false);
}
