using TheLastStand.Controller.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.Module;
using TheLastStand.View.Building.BuildingGaugeEffect;

namespace TheLastStand.Model.Building.BuildingGaugeEffect;

public class OpenMagicSeal : BuildingGaugeEffect
{
	public OpenMagicSeal(ProductionModule productionBuilding, BuildingGaugeEffectDefinition buildingGaugeEffectDefinition, BuildingGaugeEffectController buildingGaugeEffectController, BuildingGaugeEffectView buildingGaugeEffectView)
		: base(productionBuilding, buildingGaugeEffectDefinition, buildingGaugeEffectController, buildingGaugeEffectView)
	{
	}
}
