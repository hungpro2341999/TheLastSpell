using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class ImprovePassive : BuildingUpgradeEffect
{
	public ImprovePassiveController ImprovePassiveController => base.BuildingUpgradeEffectController as ImprovePassiveController;

	public ImprovePassiveDefinition ImprovePassiveDefinition => base.BuildingUpgradeEffectDefinition as ImprovePassiveDefinition;

	public ImprovePassive(ImprovePassiveDefinition definition, ImprovePassiveController controller, BuildingUpgrade buildingUpgrade)
		: base(definition, controller, buildingUpgrade)
	{
	}
}
