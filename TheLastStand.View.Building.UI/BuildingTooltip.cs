using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Building;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class BuildingTooltip : TooltipBase
{
	[SerializeField]
	private Text titleText;

	[SerializeField]
	private Image portrait;

	[SerializeField]
	private TextMeshProUGUI descText;

	[SerializeField]
	private GameObject materialCostPanel;

	[SerializeField]
	private Text materialCostText;

	[SerializeField]
	private Text unsellableText;

	[SerializeField]
	private GameObject repairMaterialCostPanel;

	[SerializeField]
	private Text repairMaterialCostText;

	[SerializeField]
	private Text unrepairableText;

	[SerializeField]
	private Image healthGauge;

	[SerializeField]
	private Text healthText;

	[SerializeField]
	private Image effectGauge;

	[SerializeField]
	private Text effectGaugeThresholdText;

	[SerializeField]
	private TextMeshProUGUI magesText;

	public TheLastStand.Model.Building.Building Building { get; set; }

	public BuildingDefinition BuildingDefinition { get; set; }

	public TheLastStand.Model.Building.Building PreviousUpgradeBuilding { get; set; }

	public void SetContent(BuildingDefinition buildingDefinition, TheLastStand.Model.Building.Building building = null, TheLastStand.Model.Building.Building previousUpgradeBuilding = null)
	{
		BuildingDefinition = buildingDefinition;
		Building = building;
		PreviousUpgradeBuilding = previousUpgradeBuilding;
	}

	protected override bool CanBeDisplayed()
	{
		if (BuildingDefinition != null)
		{
			return TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.FinalBossDeath;
		}
		return false;
	}

	protected override void RefreshContent()
	{
		if (titleText != null)
		{
			titleText.text = BuildingDefinition.Id;
		}
		if (portrait != null)
		{
			portrait.sprite = BuildingView.GetPortraitSprite(BuildingDefinition.Id);
		}
		if (descText != null)
		{
			descText.text = Localizer.Get("BuildingDescription_" + BuildingDefinition.Id);
		}
		int cost = Building.ConstructionModule.Cost;
		if (materialCostText != null)
		{
			if (cost > 0)
			{
				materialCostPanel.SetActive(value: true);
				if (Building == null)
				{
					materialCostText.text = ((PreviousUpgradeBuilding == null) ? $"Buy for: {cost}" : $"Upgrade for: {cost - PreviousUpgradeBuilding.ConstructionModule.Cost}");
				}
			}
			else
			{
				materialCostPanel.SetActive(value: false);
			}
		}
		if (unsellableText != null)
		{
			unsellableText.gameObject.SetActive(cost <= 0);
		}
		if (repairMaterialCostText != null)
		{
			if (cost > 0 && Building.DamageableModule.Health < Building.DamageableModule.HealthTotal)
			{
				repairMaterialCostPanel.SetActive(value: true);
				if (Building != null)
				{
					repairMaterialCostText.text = $"Repair for: {Building.ConstructionModule.RepairCost}";
				}
				else
				{
					repairMaterialCostText.text = string.Empty;
				}
			}
			else
			{
				repairMaterialCostPanel.SetActive(value: false);
			}
		}
		if (unrepairableText != null)
		{
			unrepairableText.gameObject.SetActive(Building == null || Building.ConstructionModule.RepairCost <= 0 || Building.DamageableModule.Health == Building.DamageableModule.HealthTotal);
		}
		if (healthText != null)
		{
			healthText.text = ((BuildingDefinition.DamageableModuleDefinition == null || BuildingDefinition.DamageableModuleDefinition.HealthTotal <= 0f) ? "Indestructible" : ((Building != null) ? $"{Building.DamageableModule.Health}/{Building.DamageableModule.HealthTotal}" : $"{((PreviousUpgradeBuilding != null) ? ((float)(int)(PreviousUpgradeBuilding.DamageableModule.Health / PreviousUpgradeBuilding.DamageableModule.HealthTotal * BuildingDefinition.DamageableModuleDefinition.HealthTotal)) : BuildingDefinition.DamageableModuleDefinition.HealthTotal)}/{BuildingDefinition.DamageableModuleDefinition.HealthTotal}"));
		}
		if (healthGauge != null)
		{
			if (PreviousUpgradeBuilding != null && BuildingDefinition.DamageableModuleDefinition != null)
			{
				healthGauge.fillAmount = PreviousUpgradeBuilding.DamageableModule.Health / PreviousUpgradeBuilding.DamageableModule.HealthTotal * BuildingDefinition.DamageableModuleDefinition.HealthTotal / BuildingDefinition.DamageableModuleDefinition.HealthTotal;
			}
			else
			{
				healthGauge.fillAmount = ((Building != null && Building.DamageableModule.HealthTotal > 0f) ? (Building.DamageableModule.Health / Building.DamageableModule.HealthTotal) : 1f);
			}
		}
		if (effectGauge != null)
		{
			effectGauge.transform.parent.gameObject.SetActive(BuildingDefinition.ProductionModuleDefinition?.BuildingGaugeEffectDefinition != null);
			if (Building != null && Building.ProductionModule?.BuildingGaugeEffect != null)
			{
				effectGauge.fillAmount = (float)Building.ProductionModule.BuildingGaugeEffect.Units / (float)Building.ProductionModule.BuildingGaugeEffect.UnitsThreshold;
				effectGaugeThresholdText.text = Building.ProductionModule.BuildingGaugeEffect.UnitsThreshold.ToString();
			}
			else
			{
				effectGauge.fillAmount = 0f;
				effectGaugeThresholdText.text = string.Empty;
			}
		}
		magesText.text = ((Building != null && Building is MagicCircle magicCircle) ? $"Mages conjuring: {magicCircle.MageCount}/{magicCircle.MageSlots}" : string.Empty);
	}
}
