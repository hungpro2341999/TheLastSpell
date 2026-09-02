using TMPro;
using TPLib;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Building;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingSkillDestroyPanel : BuildingCapacityPanel
{
	public const string DestroySkillId = "Destroy";

	[SerializeField]
	private GameObject skillCostPanel;

	[SerializeField]
	private Image skillCostIconImage;

	[SerializeField]
	private TextMeshProUGUI skillCostText;

	public TheLastStand.Model.Building.Building Building { get; set; }

	public override void OnSkillPanelHovered(bool hover)
	{
		if (hover)
		{
			BuildingManager.BuildingSkillTooltip.SetContent("Destroy", Building);
			BuildingManager.BuildingSkillTooltip.FollowTarget = confirmButton.transform;
			BuildingManager.BuildingSkillTooltip.Display();
			if (Building.BattleModule?.Goals != null && buildingCapacitiesPanel.SelectedCapacityPanel != this)
			{
				SkillManager.CompendiumPanel.HideWithoutClearingData();
			}
		}
		else
		{
			BuildingManager.BuildingSkillTooltip.Hide();
			if (Building.BattleModule?.Goals != null && buildingCapacitiesPanel.SelectedCapacityPanel == null && !TPSingleton<SettingsManager>.Instance.Settings.HideCompendium)
			{
				SkillManager.CompendiumPanel.Display();
			}
		}
	}

	public void OnDestroyButtonClick()
	{
		buildingCapacitiesPanel.ChangeSelectedCapacityPanel(this);
	}

	public void OnDestroyConfirmButtonClick(BetterButton button)
	{
		button.interactable = false;
		OnConfirmButtonClick();
		buildingCapacitiesPanel.ChangeSelectedCapacityPanel(null);
		Building.BuildingController.DamageableModuleController.Demolish();
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
		}
		buildingCapacitiesPanel.JoystickSkillBar.DeselectCurrentSkill();
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
	}

	public override void Refresh()
	{
		button.Interactable = Building.ConstructionModule.IsDemolishable;
		skillCostIconImage.sprite = Building.BuildingView.GetBuildingCostIconSprite();
		string buildingSkillCostString = Building.BuildingView.GetBuildingSkillCostString("Destroy");
		skillCostPanel?.SetActive(!string.IsNullOrEmpty(buildingSkillCostString));
		skillCostText.text = buildingSkillCostString;
		skillCostText.color = TPSingleton<ResourceManager>.Instance.GetResourceColor(Building.ConstructionModule.CostsMaterials ? "Materials" : "Gold");
	}
}
