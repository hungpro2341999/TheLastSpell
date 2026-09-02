using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class FillEffectGaugeController : BuildingPassiveEffectController
{
	public FillEffectGauge FillEffectGauge => base.BuildingPassiveEffect as FillEffectGauge;

	public FillEffectGaugeController(PassivesModule buildingPassivesModule, FillEffectGaugeDefinition fillEffectGaugeDefinition)
	{
		base.BuildingPassiveEffect = new FillEffectGauge(buildingPassivesModule, fillEffectGaugeDefinition, this);
	}

	public override void Apply()
	{
		if (base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.ProductionModule?.BuildingGaugeEffect == null)
		{
			TPSingleton<BuildingManager>.Instance.LogError("The building " + base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.BuildingDefinition.Id + " has no gauge effect", CLogLevel.MAJOR);
			return;
		}
		int num = ComputePassiveValue();
		if (num > 0)
		{
			base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.BuildingController.ProductionModuleController.AddProductionUnits(num, useRandomDelay: true);
			TPSingleton<BuildingManager>.Instance.Log($"({base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.BuildingDefinition.Id}) Fill effect gauge (+{num}, total {base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.ProductionModule.BuildingGaugeEffect.Units})", CLogLevel.MAJOR);
		}
	}

	public override void ImproveEffect(int bonus)
	{
		FillEffectGauge.UpgradedBonusValue += bonus;
	}

	public int ComputePassiveValue()
	{
		int num = FillEffectGauge.FillEffectGaugeDefinition.Value.EvalToInt(new BuildingInterpreterContext());
		int num2 = 0;
		num2 += FillEffectGauge.UpgradedBonusValue;
		if (!MetaUpgradesManager.IsThisBuildingUnlockedByDefault(base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.Id) && MetaUpgradeEffectsController.TryGetEffectsOfType<BuildingModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int num3 = effects.Length - 1; num3 >= 0; num3--)
			{
				if (effects[num3].BuildingId == base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.Id)
				{
					num2 += Mathf.RoundToInt((float)(num * effects[num3].PassiveProductionBonus) / 100f);
				}
			}
		}
		return num + num2;
	}
}
