using TheLastStand.Controller.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.Module;
using TheLastStand.View.Building.BuildingGaugeEffect;
using UnityEngine;

namespace TheLastStand.Model.Building.BuildingGaugeEffect;

public class GainGold : BuildingGaugeEffect
{
	public GainGoldDefinition GainGoldDefinition => base.BuildingGaugeEffectDefinition as GainGoldDefinition;

	public int UpgradedBonusValue { get; set; }

	public GainGold(ProductionModule productionBuilding, BuildingGaugeEffectDefinition buildingGaugeEffectDefinition, BuildingGaugeEffectController buildingGaugeEffectController, BuildingGaugeEffectView buildingGaugeEffectView)
		: base(productionBuilding, buildingGaugeEffectDefinition, buildingGaugeEffectController, buildingGaugeEffectView)
	{
	}

	public override int GetOneLoopProductionValue()
	{
		return ComputeGoldValue();
	}

	public int ComputeGoldValue()
	{
		return Mathf.RoundToInt(GainGoldDefinition.GoldGain.EvalToFloat(new FormulaInterpreterContext()) + (float)UpgradedBonusValue);
	}
}
