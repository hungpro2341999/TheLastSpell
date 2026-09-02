using TheLastStand.Controller.Building;
using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Building.Module;
using UnityEngine;

namespace TheLastStand.Model.Building.BuildingPassive;

public class FillEffectGauge : BuildingPassiveEffect
{
	public int CurrentValue
	{
		get
		{
			int num = FillEffectGaugeDefinition.Value.EvalToInt(new BuildingInterpreterContext());
			int num2 = 0;
			num2 += UpgradedBonusValue;
			if (!MetaUpgradesManager.IsThisBuildingUnlockedByDefault(base.BuildingPassivesModule.BuildingParent.Id) && MetaUpgradeEffectsController.TryGetEffectsOfType<BuildingModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
			{
				for (int num3 = effects.Length - 1; num3 >= 0; num3--)
				{
					if (effects[num3].BuildingId == base.BuildingPassivesModule.BuildingParent.Id)
					{
						num2 += Mathf.RoundToInt((float)(num * effects[num3].PassiveProductionBonus) / 100f);
					}
				}
			}
			return num + num2;
		}
	}

	public int UpgradedBonusValue { get; set; }

	public FillEffectGaugeController FillEffectGaugeController => base.BuildingPassiveEffectController as FillEffectGaugeController;

	public FillEffectGaugeDefinition FillEffectGaugeDefinition => base.BuildingPassiveEffectDefinition as FillEffectGaugeDefinition;

	public FillEffectGauge(PassivesModule buildingPassivesModule, FillEffectGaugeDefinition buildingPassiveDefinition, FillEffectGaugeController buildingPassiveController)
		: base(buildingPassivesModule, buildingPassiveDefinition, buildingPassiveController)
	{
	}
}
