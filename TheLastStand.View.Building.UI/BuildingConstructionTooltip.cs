using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Building;
using TheLastStand.Manager.Building;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Building.UI;

public class BuildingConstructionTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI buildingName;

	[SerializeField]
	private TextMeshProUGUI buildingDescription;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private TextMeshProUGUI buildLimitText;

	[SerializeField]
	private TextMeshProUGUI maxHealthText;

	[SerializeField]
	private TextMeshProUGUI chargesText;

	[SerializeField]
	private GameObject chargesIcon;

	private bool useDefaultValues;

	private BuildingDefinition buildingDefinition;

	private Color buildLimitTextDefaultColor;

	public void Init(BuildingDefinition newBuildingDefinition, bool newUseDefaultValues = false)
	{
		buildingDefinition = newBuildingDefinition;
		useDefaultValues = newUseDefaultValues;
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		buildLimitTextDefaultColor = buildLimitText.color;
	}

	protected override bool CanBeDisplayed()
	{
		return buildingDefinition != null;
	}

	protected override void RefreshContent()
	{
		RefreshText();
		RefreshLimitDisplay();
	}

	private void RefreshText()
	{
		buildingName.text = buildingDefinition.Name;
		buildingDescription.text = buildingDefinition.Description;
		bool num = buildingDefinition.ConstructionModuleDefinition.NativeGoldCost > 0;
		int num2 = BuildingManager.ComputeBuildingCost(buildingDefinition.ConstructionModuleDefinition, useDefaultValues);
		buildLimitText.text = buildingDefinition.ConstructionModuleDefinition.GetLocalizedBuildLimit(useDefaultValues);
		buildLimitText.color = ((!TPSingleton<ConstructionManager>.Exist() || ConstructionManager.IsUnderBuildLimit(buildingDefinition.ConstructionModuleDefinition)) ? buildLimitTextDefaultColor : GameView.NegativeColor);
		string arg = (num ? "Gold" : "Materials");
		costText.text = $"<style=\"{arg}\"><style=\"Number\">{num2}</style></style>";
		if (buildingDefinition.DamageableModuleDefinition == null || BuildingManager.ComputeBuildingTotalHealth(buildingDefinition.DamageableModuleDefinition, useDefaultValues) == 0f)
		{
			maxHealthText.gameObject.SetActive(value: false);
			if (buildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Trap) && buildingDefinition.BattleModuleDefinition != null)
			{
				chargesIcon.SetActive(value: true);
				chargesText.gameObject.SetActive(value: true);
				chargesText.text = $"<style=Number><style=RemainingCharges>{buildingDefinition.BattleModuleDefinition.MaximumTrapCharges}</style></style>";
			}
			else
			{
				chargesIcon.SetActive(value: false);
				chargesText.gameObject.SetActive(value: false);
			}
		}
		else
		{
			maxHealthText.gameObject.SetActive(value: true);
			chargesIcon.SetActive(value: false);
			chargesText.gameObject.SetActive(value: false);
			maxHealthText.text = $"<sprite name=\"Health\"><style=\"Number\">{BuildingManager.ComputeBuildingTotalHealth(buildingDefinition.DamageableModuleDefinition, useDefaultValues)}</style>";
		}
	}

	private void RefreshLimitDisplay()
	{
		buildLimitText.gameObject.SetActive(!buildingDefinition.ConstructionModuleDefinition.IsUnlimited(useDefaultValues));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshText();
		}
	}
}
