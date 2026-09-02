using TheLastStand.Controller.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.Module;
using TheLastStand.View.Building.BuildingGaugeEffect;

namespace TheLastStand.Model.Building.BuildingGaugeEffect;

public class UpgradeStatGaugeEffect : BuildingGaugeEffect
{
	public UpgradeStatGaugeEffectDefinition UpgradeStatGaugeEffectDefinition => base.BuildingGaugeEffectDefinition as UpgradeStatGaugeEffectDefinition;

	public UpgradeStatGaugeEffect(ProductionModule productionBuilding, BuildingGaugeEffectDefinition buildingGaugeEffectDefinition, BuildingGaugeEffectController buildingGaugeEffectController, BuildingGaugeEffectView buildingGaugeEffectView)
		: base(productionBuilding, buildingGaugeEffectDefinition, buildingGaugeEffectController, buildingGaugeEffectView)
	{
	}

	public override int GetOneLoopProductionValue()
	{
		return UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Bonus;
	}
}
