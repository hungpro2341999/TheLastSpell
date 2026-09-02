using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class ImproveGaugeEffect : BuildingUpgradeEffect
{
	public ImproveGaugeEffectController ImproveGaugeEffectController => base.BuildingUpgradeEffectController as ImproveGaugeEffectController;

	public ImproveGaugeEffectDefinition ImproveGaugeEffectDefinition => base.BuildingUpgradeEffectDefinition as ImproveGaugeEffectDefinition;

	public ImproveGaugeEffect(ImproveGaugeEffectDefinition definition, ImproveGaugeEffectController controller, BuildingUpgrade buildingUpgrade)
		: base(definition, controller, buildingUpgrade)
	{
	}
}
