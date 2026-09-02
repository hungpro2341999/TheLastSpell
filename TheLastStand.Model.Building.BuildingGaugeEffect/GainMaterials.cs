using TheLastStand.Controller.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.Module;
using TheLastStand.View.Building.BuildingGaugeEffect;
using UnityEngine;

namespace TheLastStand.Model.Building.BuildingGaugeEffect;

public class GainMaterials : BuildingGaugeEffect
{
	public GainMaterialsDefinition GainMaterialsDefinition => base.BuildingGaugeEffectDefinition as GainMaterialsDefinition;

	public int UpgradedBonusValue { get; set; }

	public GainMaterials(ProductionModule productionBuilding, BuildingGaugeEffectDefinition buildingGaugeEffectDefinition, BuildingGaugeEffectController buildingGaugeEffectController, BuildingGaugeEffectView buildingGaugeEffectView)
		: base(productionBuilding, buildingGaugeEffectDefinition, buildingGaugeEffectController, buildingGaugeEffectView)
	{
	}

	public override int GetOneLoopProductionValue()
	{
		return ComputeMaterialsValue();
	}

	public int ComputeMaterialsValue()
	{
		return Mathf.RoundToInt(GainMaterialsDefinition.MaterialsGain.EvalToFloat(new FormulaInterpreterContext()) + (float)UpgradedBonusValue);
	}
}
