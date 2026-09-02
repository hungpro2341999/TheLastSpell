using TMPro;
using TPLib;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingSkillRepairPanel : BuildingCapacityPanel
{
	private const string RepairSkillId = "Repair";

	[SerializeField]
	private Image skillCostIconImage;

	[SerializeField]
	private TextMeshProUGUI skillCostText;

	public TheLastStand.Model.Building.Building Building { get; set; }

	public override void OnSkillPanelHovered(bool hover)
	{
		if (hover)
		{
			BuildingManager.BuildingSkillTooltip.SetContent("Repair", Building);
			BuildingManager.BuildingSkillTooltip.FollowTarget = confirmButton.transform;
			BuildingManager.BuildingSkillTooltip.Display();
		}
		else
		{
			BuildingManager.BuildingSkillTooltip.Hide();
		}
	}

	public void OnRepairButtonClick()
	{
		buildingCapacitiesPanel.ChangeSelectedCapacityPanel(this);
	}

	public void OnRepairConfirmButtonClick(BetterButton button)
	{
		button.interactable = false;
		OnConfirmButtonClick();
		buildingCapacitiesPanel.ChangeSelectedCapacityPanel(null);
		ConstructionManager.RepairBuilding(Building);
		BuildingManager.BuildingSkillTooltip.Hide();
		GameView.BottomScreenPanel.BuildingManagementPanel.Refresh();
		buildingCapacitiesPanel.JoystickSkillBar.DeselectCurrentSkill();
	}

	public override void Refresh()
	{
		bool interactable = ConstructionManager.CanRepairBuilding(Building.ConstructionModule);
		button.Interactable = interactable;
		skillCostIconImage.sprite = Building.BuildingView.GetBuildingCostIconSprite();
		skillCostText.text = Building.BuildingView.GetBuildingSkillCostString("Repair");
		skillCostText.color = TPSingleton<ResourceManager>.Instance.GetResourceColor(Building.ConstructionModule.CostsMaterials ? "Materials" : "Gold");
	}
}
