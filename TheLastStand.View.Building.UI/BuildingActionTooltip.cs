using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Building.UI;

public class BuildingActionTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI actionName;

	[SerializeField]
	private TextMeshProUGUI actionDescription;

	[SerializeField]
	private TextMeshProUGUI loreDescription;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private GameObject invalidBuildingActionPanel;

	[SerializeField]
	private TextMeshProUGUI invalidBuildingActionText;

	private bool fetchDefaultValues;

	public BuildingAction BuildingAction { get; private set; }

	public BuildingActionDefinition BuildingActionDefinition { get; private set; }

	public void Init(BuildingAction buildingAction = null, BuildingActionDefinition buildingActionDefinition = null, bool newFetchDefaultValues = false)
	{
		BuildingAction = buildingAction;
		BuildingActionDefinition = buildingAction?.BuildingActionDefinition ?? buildingActionDefinition;
		fetchDefaultValues = newFetchDefaultValues;
	}

	protected override bool CanBeDisplayed()
	{
		return BuildingActionDefinition != null;
	}

	protected override void RefreshContent()
	{
		actionName.text = BuildingActionDefinition.Name;
		actionDescription.text = BuildingActionDefinition.GetDescription(GetFillEffectUnitsThreshold(), GetProductionValue());
		loreDescription.text = BuildingActionDefinition.LoreDescription;
		costText.text = ComputeCostText();
		RefreshInvalidPanels();
	}

	private int GetProductionValue()
	{
		if (BuildingAction == null)
		{
			return 0;
		}
		return BuildingAction.ProductionBuilding.BuildingGaugeEffect?.GetOneLoopProductionValue() ?? 0;
	}

	private int GetFillEffectUnitsThreshold()
	{
		return BuildingAction?.ProductionBuilding.BuildingGaugeEffect?.UnitsThreshold ?? (-1);
	}

	private string ComputeCostText()
	{
		string text = string.Empty;
		int num = (fetchDefaultValues ? BuildingActionDefinition.WorkersCost : ResourceManager.GetModifiedWorkersCost(BuildingActionDefinition));
		if (num > 0)
		{
			text += $"<color=#d29a6e><sprite name=\"Workers\"><style=\"Number\">{num}</style></color>";
		}
		if (BuildingActionDefinition.PhaseStates.ProductionState == PhaseStates.E_PhaseState.Available && BuildingActionDefinition.UsesPerTurnCount > 0)
		{
			text = text + "\n" + Localizer.Format("BuildingUpgradeTooltipDescription_UsesPerProduction", BuildingActionDefinition.UsesPerTurnCount);
		}
		return text;
	}

	private void RefreshInvalidPanels()
	{
		if (BuildingActionDefinition.ContainsRepelFogEffect && !TPSingleton<FogManager>.Instance.Fog.CanDecreaseFogDensity)
		{
			invalidBuildingActionPanel.SetActive(value: true);
			invalidBuildingActionText.text = Localizer.Get("InvalidBuildingAction_RepelFog");
		}
		else
		{
			invalidBuildingActionPanel.SetActive(value: false);
		}
	}
}
