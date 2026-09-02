using System.Collections;
using TMPro;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Framework;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.View.ToDoList;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingUpgradePanel : BuildingCapacityPanel
{
	public static class Constants
	{
		public const string UpgradeIconDefaultPath = "View/Sprites/UI/Buildings/Upgrades/Default";

		public const string UpgradeIconPathPrefix = "View/Sprites/UI/Buildings/Upgrades/";
	}

	[SerializeField]
	private GameObject goldParent;

	[SerializeField]
	private TextMeshProUGUI goldText;

	[SerializeField]
	private GameObject materialsParent;

	[SerializeField]
	private TextMeshProUGUI materialsText;

	[SerializeField]
	private GameObject maxLevelParent;

	[SerializeField]
	private Image upgradeIcon;

	[SerializeField]
	private Image upgradeLevelIcon;

	[SerializeField]
	private DataSpriteTable upgradeLevelIcons;

	[SerializeField]
	private Image maxLevelIcon;

	[SerializeField]
	private Animator upgradeLevelFXAnimator;

	[SerializeField]
	[Range(0f, 1f)]
	private float refreshDelayAfterLevelUp = 0.3f;

	private Coroutine refreshCoroutine;

	public BuildingUpgrade BuildingUpgrade { get; set; }

	public void Display(bool show)
	{
		base.BuildingCapacityRect.gameObject.SetActive(show);
	}

	public override void OnSkillPanelHovered(bool hover)
	{
		if (hover && BuildingUpgrade != null)
		{
			BuildingManager.BuildingUpgradeTooltip.SetContent(BuildingUpgrade);
			BuildingManager.BuildingUpgradeTooltip.FollowTarget = tooltipAnchor?.transform ?? confirmButton?.transform;
			BuildingManager.BuildingUpgradeTooltip.Display();
		}
		else
		{
			BuildingManager.BuildingUpgradeTooltip.Hide();
		}
	}

	public void OnUpgradeButtonClick()
	{
		TPSingleton<BuildingManager>.Instance.Log("Building upgrade " + BuildingUpgrade.BuildingUpgradeDefinition.Id + " was clicked.", CLogLevel.DETAILED);
		buildingCapacitiesPanel.ChangeSelectedCapacityPanel(this);
	}

	public void OnUpgradeConfirmButtonClick(BetterButton button)
	{
		button.interactable = false;
		if (refreshCoroutine != null)
		{
			StopCoroutine(refreshCoroutine);
		}
		OnConfirmButtonClick();
		BuildingUpgrade.Building.BuildingView.BuildingHUD.CompleteCurrentHealthAnimation();
		refreshCoroutine = TPSingleton<BuildingManager>.Instance.StartCoroutine(WaitRefresh());
		BuildingUpgrade.BuildingUpgradeController.UnlockUpgrade(freeUpgrade: false, playFx: true, sendAnalytics: true);
		upgradeLevelFXAnimator.SetTrigger("levelUp");
		buildingCapacitiesPanel.ChangeSelectedCapacityPanel(null);
		buildingCapacitiesPanel.JoystickSkillBar.DeselectCurrentSkill();
		GameView.BottomScreenPanel.BuildingManagementPanel.Refresh();
		TPSingleton<ToDoListView>.Instance.RefreshWorkersNotification();
		if (BuildingUpgrade != null && BuildingUpgrade.UpgradeLevel + 1 >= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions.Count)
		{
			base.button.Interactable = false;
		}
	}

	public override void Refresh()
	{
		if (BuildingUpgrade != null && refreshCoroutine == null)
		{
			int num = BuildingUpgrade.UpgradeLevel + 1;
			bool flag = num >= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions.Count;
			goldParent.SetActive(!flag && BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].GoldCost > 0);
			materialsParent.SetActive(!flag && BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].MaterialCost > 0);
			maxLevelParent.SetActive(flag);
			if (!flag && BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].GoldCost > 0)
			{
				goldText.text = $"{BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].GoldCost}";
				goldText.color = ((TPSingleton<ResourceManager>.Instance.Gold >= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].GoldCost) ? TPSingleton<ResourceManager>.Instance.GetResourceColor("Gold") : Color.red);
			}
			if (!flag && BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].MaterialCost > 0)
			{
				materialsText.text = $"{BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].MaterialCost}";
				materialsText.color = ((TPSingleton<ResourceManager>.Instance.Materials >= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].MaterialCost) ? TPSingleton<ResourceManager>.Instance.GetResourceColor("Materials") : Color.red);
			}
			RefreshUpgradeLevelIcon();
			maxLevelIcon.enabled = flag;
			button.Interactable = !flag && TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night && TPSingleton<ResourceManager>.Instance.Gold >= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].GoldCost && TPSingleton<ResourceManager>.Instance.Materials >= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].MaterialCost;
			upgradeIcon.sprite = ResourcePooler<Sprite>.LoadOnce(string.Format("{0}{1}{2}", "View/Sprites/UI/Buildings/Upgrades/", BuildingUpgrade.BuildingUpgradeDefinition.Id, flag ? BuildingUpgrade.UpgradeLevel : num));
			if (upgradeIcon.sprite == null)
			{
				upgradeIcon.sprite = ResourcePooler<Sprite>.LoadOnce("View/Sprites/UI/Buildings/Upgrades/Default");
			}
		}
	}

	public override void SelectConfirmButton()
	{
		if (refreshCoroutine == null)
		{
			base.SelectConfirmButton();
		}
	}

	private void RefreshUpgradeLevelIcon()
	{
		upgradeLevelIcon.enabled = BuildingUpgrade.UpgradeLevel != -1;
		if (BuildingUpgrade.UpgradeLevel >= 0)
		{
			upgradeLevelIcon.sprite = upgradeLevelIcons.GetSpriteAt(Mathf.Min(upgradeLevelIcons._Sprites.Length - 1, BuildingUpgrade.UpgradeLevel));
		}
	}

	private IEnumerator WaitRefresh()
	{
		yield return SharedYields.WaitForSeconds(refreshDelayAfterLevelUp);
		refreshCoroutine = null;
		Refresh();
	}
}
