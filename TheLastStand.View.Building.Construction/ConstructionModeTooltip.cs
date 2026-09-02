using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Building.Construction;

public class ConstructionModeTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI actionName;

	[SerializeField]
	private TextMeshProUGUI actionDescription;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private GameObject unusableCausePanel;

	[SerializeField]
	private TextMeshProUGUI unusableCauseText;

	protected override bool CanBeDisplayed()
	{
		if (!(TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton != null))
		{
			return TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton != null;
		}
		return true;
	}

	protected override void RefreshContent()
	{
		TheLastStand.Model.Building.Construction.E_UnusableActionCause e_UnusableActionCause = TheLastStand.Model.Building.Construction.E_UnusableActionCause.None;
		if (TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton != null)
		{
			ConstructionModeButton hoveredConstructionModeButton = TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton;
			if (!(hoveredConstructionModeButton is RepairModeButton))
			{
				if (hoveredConstructionModeButton is DestroyModeButton)
				{
					RefreshDestroyMode();
				}
			}
			else
			{
				RefreshRepairMode();
			}
			e_UnusableActionCause = TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton.UnusableActionCause;
		}
		else if (TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton != null)
		{
			RefreshRepairCategory();
			unusableCausePanel.SetActive(value: false);
			e_UnusableActionCause = TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton.UnusableActionCause;
		}
		if (e_UnusableActionCause == TheLastStand.Model.Building.Construction.E_UnusableActionCause.None)
		{
			unusableCausePanel.SetActive(value: false);
			return;
		}
		unusableCauseText.text = Localizer.Get("Construction_UnusableActionCause_" + e_UnusableActionCause);
		unusableCausePanel.SetActive(value: true);
	}

	private void RefreshRepairMode()
	{
		if (TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton is RepairModeButton repairModeButton)
		{
			actionName.text = Localizer.Get("RepairModeName_" + repairModeButton.RepairMode);
			actionDescription.text = Localizer.Get("RepairModeDescription_" + repairModeButton.RepairMode);
			costText.enabled = false;
		}
	}

	private void RefreshRepairCategory()
	{
		RepairCategoryButton hoveredRepairCategoryButton = TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton;
		actionName.text = Localizer.Get("RepairCategoryName_" + hoveredRepairCategoryButton.Id);
		actionDescription.text = Localizer.Get("RepairCategoryDescription_" + hoveredRepairCategoryButton.Id);
		costText.enabled = true;
		string arg = (hoveredRepairCategoryButton.IsGold ? "Gold" : "Materials");
		costText.text = $"<style={arg}><style=Number>{hoveredRepairCategoryButton.Cost}</style></style>";
	}

	private void RefreshDestroyMode()
	{
		if (TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton is DestroyModeButton destroyModeButton)
		{
			string text = "DestroyModeDescription_" + destroyModeButton.DestroyMode;
			string value;
			string text2 = ((InputManager.IsLastControllerJoystick && Localizer.TryGet(text + "_Gamepad", out value)) ? value : Localizer.Get(text));
			actionDescription.text = text2;
			actionName.text = Localizer.Get("DestroyModeName_" + destroyModeButton.DestroyMode);
			costText.enabled = false;
		}
	}
}
