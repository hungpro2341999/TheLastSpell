using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class ImproveGaugeEffectController : BuildingUpgradeEffectController
{
	public ImproveGaugeEffect ImproveGaugeEffect => base.BuildingUpgradeEffect as ImproveGaugeEffect;

	public ImproveGaugeEffectController(ImproveGaugeEffectDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ImproveGaugeEffect(definition, this, buildingUpgrade);
	}

	public override void TriggerEffect(bool onLoad = false)
	{
		switch (base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectDefinition.Id)
		{
		case "GainGold":
			(base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingGaugeEffect as GainGold).UpgradedBonusValue += ImproveGaugeEffect.ImproveGaugeEffectDefinition.Value;
			break;
		case "GainMaterials":
			(base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingGaugeEffect as GainMaterials).UpgradedBonusValue += ImproveGaugeEffect.ImproveGaugeEffectDefinition.Value;
			break;
		}
	}
}
