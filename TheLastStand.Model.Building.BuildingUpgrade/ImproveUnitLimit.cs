using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class ImproveUnitLimit : BuildingUpgradeEffect
{
	public ImproveUnitLimitController ImproveUnitLimitController => base.BuildingUpgradeEffectController as ImproveUnitLimitController;

	public ImproveUnitLimitDefinition ImproveUnitLimitDefinition => base.BuildingUpgradeEffectDefinition as ImproveUnitLimitDefinition;

	public ImproveUnitLimit(ImproveUnitLimitDefinition definition, ImproveUnitLimitController controller, BuildingUpgrade buildingUpgrade)
		: base(definition, controller, buildingUpgrade)
	{
	}
}
