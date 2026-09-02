using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization;
using TheLastStand.View.Building.BuildingGaugeEffect;

namespace TheLastStand.Controller.Building.BuildingGaugeEffect;

public class OpenMagicSealController : BuildingGaugeEffectController
{
	public OpenMagicSealController(SerializedGaugeEffect container, ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new OpenMagicSeal(productionBuilding, definition, this, new OpenMagicSealView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
		base.BuildingGaugeEffect.Deserialize(container);
	}

	public OpenMagicSealController(ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new OpenMagicSeal(productionBuilding, definition, this, new OpenMagicSealView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
	}

	public override bool CanTriggerEffect()
	{
		return false;
	}
}
