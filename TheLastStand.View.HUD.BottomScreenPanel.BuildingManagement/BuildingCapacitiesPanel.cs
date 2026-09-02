using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Yield;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingCapacitiesPanel : MonoBehaviour
{
	[SerializeField]
	private BuildingCapacitiesGroupPanel buildingDestroyGroup;

	[SerializeField]
	private BuildingCapacitiesGroupPanel buildingRepairGroup;

	[SerializeField]
	private BuildingCapacitiesGroupPanel buildingActionsGroup;

	[SerializeField]
	private BuildingCapacitiesGroupPanel buildingUpgradesGroup;

	[SerializeField]
	private BuildingCapacitiesGroupPanel buildingGlobalUpgradesGroup;

	[SerializeField]
	private BuildingCapacitiesGroupPanel buildingSkillsGroup;

	[SerializeField]
	private GameObject buildingSkillsGroupIcon;

	[SerializeField]
	private LayoutGroup buildingCapacitiesLayoutGroup;

	[SerializeField]
	private ContentSizeFitter contentSizeFitter;

	[SerializeField]
	private RectTransform buildingCapacitiesRectTransform;

	[SerializeField]
	private Scrollbar scrollbar;

	[SerializeField]
	private RectTransform scrollViewport;

	[SerializeField]
	private RectTransform scrollViewportParent;

	[SerializeField]
	private int scrollbarEnabledBottomPadding = 12;

	[SerializeField]
	private int scrollbarDisabledBottomPadding = -4;

	[SerializeField]
	private JoystickSkillBar joystickSkillBar;

	[SerializeField]
	private GameObject gamepadInputDisplay_PreviousSkill;

	[SerializeField]
	private GameObject gamepadInputDisplay_NextSkillScrollEnabled;

	[SerializeField]
	private GameObject gamepadInputDisplay_NextSkillScrollDisabled;

	private RectTransform rectTransform;

	private Vector3[] rectTransformWorldCorners = new Vector3[4];

	private List<BuildingActionPanel> buildingActionDisplays = new List<BuildingActionPanel>();

	private Tween openBuildingCapacityConfirmButtonTween;

	private Tween closeBuildingCapacityConfirmButtonTween;

	public BuildingCapacityPanel SelectedCapacityPanel { get; private set; }

	private TheLastStand.Model.Building.Building Building { get; set; }

	public JoystickSkillBar JoystickSkillBar => joystickSkillBar;

	public void ChangeSelectedCapacityPanel(BuildingCapacityPanel newSelectedPanel)
	{
		if (BuildingManager.SelectedBuildingAction != null && newSelectedPanel as BuildingActionPanel == null)
		{
			BuildingManager.SelectedBuildingAction = null;
		}
		if (SelectedCapacityPanel != null)
		{
			SelectedCapacityPanel.Select(select: false);
		}
		if (newSelectedPanel != null)
		{
			if (newSelectedPanel == SelectedCapacityPanel)
			{
				SelectedCapacityPanel = null;
				return;
			}
			SelectedCapacityPanel = newSelectedPanel;
			SelectedCapacityPanel.Select(select: true);
		}
		else
		{
			SelectedCapacityPanel = null;
		}
	}

	public void DeselectSkill()
	{
		for (int num = buildingSkillsGroup.BuildingCapacities.Count - 1; num >= 0; num--)
		{
			(buildingSkillsGroup.BuildingCapacities[num] as BuildingSkillPanel).DisplaySelector(display: false);
		}
	}

	public void HideAllGroups()
	{
		buildingDestroyGroup.Display(show: false);
		buildingRepairGroup.Display(show: false);
		buildingActionsGroup.Display(show: false);
		buildingUpgradesGroup.Display(show: false);
		buildingSkillsGroup.Display(show: false);
		buildingGlobalUpgradesGroup.Display(show: false);
	}

	public void Refresh(TheLastStand.Model.Building.Building building)
	{
		Building = building;
		buildingActionDisplays.Clear();
		((BuildingSkillDestroyPanel)buildingDestroyGroup.BuildingCapacities[0]).Building = building;
		((BuildingSkillRepairPanel)buildingRepairGroup.BuildingCapacities[0]).Building = building;
		buildingDestroyGroup.Display(TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production && building.ConstructionModule.IsDemolishable);
		buildingRepairGroup.Display(TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production && building.ConstructionModule.NeedRepair);
		if (building.ProductionModule?.BuildingActions == null)
		{
			buildingActionsGroup.Display(show: false);
		}
		else
		{
			bool show = false;
			string[] lockedBuildingActionsIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedBuildingActionsIds();
			int i = 0;
			for (int count = buildingActionsGroup.BuildingCapacities.Count; i < count; i++)
			{
				BuildingActionPanel buildingActionPanel = buildingActionsGroup.BuildingCapacities[i] as BuildingActionPanel;
				bool flag = building.ProductionModule.BuildingActions.Count > i && !lockedBuildingActionsIds.Contains(building.ProductionModule.BuildingActions[i].BuildingActionDefinition.Id) && (BuildingManager.DebugUseForceBuildingActionsAllPhases || building.ProductionModule.BuildingActions[i].BuildingActionController.GetActionCurrentState() == PhaseStates.E_PhaseState.Available);
				buildingActionPanel.Display(flag);
				if (flag)
				{
					buildingActionPanel.BuildingAction = building.ProductionModule.BuildingActions[i];
					buildingActionDisplays.Add(buildingActionPanel);
					show = true;
				}
			}
			buildingActionsGroup.Display(show);
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night || TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Deployment || building.UpgradeModule?.BuildingUpgrades == null)
		{
			buildingUpgradesGroup.Display(show: false);
		}
		else
		{
			bool flag2 = false;
			string[] lockedBuildingUpgradesIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedBuildingUpgradesIds();
			string[] lockedBuildingsIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedBuildingsIds();
			int j = 0;
			for (int count2 = buildingUpgradesGroup.BuildingCapacities.Count; j < count2; j++)
			{
				BuildingUpgradePanel buildingUpgradePanel = buildingUpgradesGroup.BuildingCapacities[j] as BuildingUpgradePanel;
				bool flag3 = building.UpgradeModule.BuildingUpgrades.Count > j && !lockedBuildingUpgradesIds.Contains(building.UpgradeModule.BuildingUpgrades[j].BuildingUpgradeDefinition.Id);
				if (flag3 && building.UpgradeModule.BuildingUpgrades[j].UpgradeLevel + 1 < building.UpgradeModule.BuildingUpgrades[j].BuildingUpgradeLevels.Count)
				{
					foreach (BuildingUpgradeEffect effect in building.UpgradeModule.BuildingUpgrades[j].BuildingUpgradeLevels[building.UpgradeModule.BuildingUpgrades[j].UpgradeLevel + 1].Effects)
					{
						if (effect is ReplaceBuilding replaceBuilding && lockedBuildingsIds.Contains(replaceBuilding.ReplaceBuildingDefinition.NewBuildingId))
						{
							flag3 = false;
							break;
						}
					}
				}
				flag2 = flag2 || flag3;
				BuildingUpgrade buildingUpgrade = (buildingUpgradePanel.BuildingUpgrade = (flag3 ? building.UpgradeModule.BuildingUpgrades[j] : null));
				buildingUpgradePanel.Display(buildingUpgrade != null);
			}
			buildingUpgradesGroup.Display(flag2);
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night || TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Deployment || building.UpgradeModule?.BuildingGlobalUpgrades == null)
		{
			buildingGlobalUpgradesGroup.Display(show: false);
		}
		else
		{
			bool flag4 = false;
			string[] lockedBuildingUpgradesIds2 = TPSingleton<MetaUpgradesManager>.Instance.GetLockedBuildingUpgradesIds();
			string[] lockedBuildingsIds2 = TPSingleton<MetaUpgradesManager>.Instance.GetLockedBuildingsIds();
			int k = 0;
			for (int count3 = buildingGlobalUpgradesGroup.BuildingCapacities.Count; k < count3; k++)
			{
				BuildingUpgradePanel buildingUpgradePanel2 = buildingGlobalUpgradesGroup.BuildingCapacities[k] as BuildingUpgradePanel;
				bool flag5 = building.UpgradeModule.BuildingGlobalUpgrades.Count > k && !lockedBuildingUpgradesIds2.Contains(building.UpgradeModule.BuildingGlobalUpgrades[k].BuildingUpgradeDefinition.Id);
				if (flag5 && building.UpgradeModule.BuildingGlobalUpgrades[k].UpgradeLevel + 1 < building.UpgradeModule.BuildingGlobalUpgrades[k].BuildingUpgradeLevels.Count)
				{
					foreach (BuildingUpgradeEffect effect2 in building.UpgradeModule.BuildingGlobalUpgrades[k].BuildingUpgradeLevels[building.UpgradeModule.BuildingGlobalUpgrades[k].UpgradeLevel + 1].Effects)
					{
						if (effect2 is ReplaceBuilding replaceBuilding2 && lockedBuildingsIds2.Contains(replaceBuilding2.ReplaceBuildingDefinition.NewBuildingId))
						{
							flag5 = false;
							break;
						}
					}
				}
				flag4 = flag4 || flag5;
				BuildingUpgrade buildingUpgrade3 = (buildingUpgradePanel2.BuildingUpgrade = (flag5 ? building.UpgradeModule.BuildingGlobalUpgrades[k] : null));
				buildingUpgradePanel2.Display(buildingUpgrade3 != null);
			}
			buildingGlobalUpgradesGroup.Display(flag4);
		}
		RefreshSkills();
		if ((buildingDestroyGroup.IsDisplayed() && buildingDestroyGroup.BuildingCapacities.Any((BuildingCapacityPanel capacity) => capacity.IsDisplayed())) || (buildingRepairGroup.IsDisplayed() && buildingRepairGroup.BuildingCapacities.Any((BuildingCapacityPanel capacity) => capacity.IsDisplayed())) || (buildingActionsGroup.IsDisplayed() && buildingActionsGroup.BuildingCapacities.Any((BuildingCapacityPanel capacity) => capacity.IsDisplayed())) || (buildingUpgradesGroup.IsDisplayed() && buildingUpgradesGroup.BuildingCapacities.Any((BuildingCapacityPanel capacity) => capacity.IsDisplayed())) || (buildingGlobalUpgradesGroup.IsDisplayed() && buildingGlobalUpgradesGroup.BuildingCapacities.Any((BuildingCapacityPanel capacity) => capacity.IsDisplayed())) || (buildingSkillsGroup.IsDisplayed() && buildingSkillsGroup.BuildingCapacities.Any((BuildingCapacityPanel capacity) => capacity.IsDisplayed())))
		{
			gamepadInputDisplay_PreviousSkill.SetActive(value: true);
			gamepadInputDisplay_NextSkillScrollEnabled.SetActive(value: true);
			gamepadInputDisplay_NextSkillScrollDisabled.SetActive(value: true);
		}
		else
		{
			gamepadInputDisplay_PreviousSkill.SetActive(value: false);
			gamepadInputDisplay_NextSkillScrollEnabled.SetActive(value: false);
			gamepadInputDisplay_NextSkillScrollDisabled.SetActive(value: false);
		}
		buildingCapacitiesLayoutGroup.enabled = true;
		contentSizeFitter.enabled = true;
		StartCoroutine(RefreshScrollingCoroutine());
	}

	public void OnCapacityHovered(BuildingCapacityPanel buildingCapacityPanel)
	{
		if (scrollbar.gameObject.activeSelf)
		{
			GUIHelpers.AdjustHorizontalScrollViewToFocusedItem(buildingCapacityPanel.transform as RectTransform, scrollViewport, scrollViewportParent, 0.05f);
		}
	}

	public void RefreshSkills()
	{
		if (Building == null)
		{
			return;
		}
		bool flag = Building.BattleModule?.Skills != null && Building.BattleModule.Skills.Count > 0;
		bool flag2 = Building.BattleModule != null && Building.BattleModule.BattleModuleDefinition.GoalsSkillsToDisplayGroupIds.Count > 0;
		if (flag || flag2)
		{
			int i = 0;
			if (flag)
			{
				foreach (TheLastStand.Model.Skill.Skill skill2 in Building.BattleModule.Skills)
				{
					BuildingSkillPanel obj = (BuildingSkillPanel)buildingSkillsGroup.BuildingCapacities[i];
					obj.SetIsGoalSkill(isGoalSkillValue: false);
					obj.Skill = skill2;
					obj.SkillOwner = Building;
					obj.Display(show: true);
					i++;
				}
			}
			if (flag2)
			{
				foreach (string goalsSkillsToDisplayGroupId in Building.BattleModule.BattleModuleDefinition.GoalsSkillsToDisplayGroupIds)
				{
					if (Building.BattleModule.Goals == null)
					{
						break;
					}
					TheLastStand.Model.Skill.Skill skill = null;
					Goal[] goals = Building.BattleModule.Goals;
					foreach (Goal goal in goals)
					{
						if (!(goal.Skill.SkillDefinition.GroupId != goalsSkillsToDisplayGroupId) && goal.GoalController.CheckPreconditionGroups())
						{
							skill = goal.Skill;
						}
					}
					if (skill != null)
					{
						BuildingSkillPanel obj2 = (BuildingSkillPanel)buildingSkillsGroup.BuildingCapacities[i];
						obj2.SetIsGoalSkill(isGoalSkillValue: true);
						obj2.Skill = skill;
						obj2.SkillOwner = Building;
						obj2.Display(show: true);
						i++;
					}
				}
			}
			for (; i < buildingSkillsGroup.BuildingCapacities.Count; i++)
			{
				((BuildingSkillPanel)buildingSkillsGroup.BuildingCapacities[i]).Display(show: false);
			}
			buildingSkillsGroupIcon.SetActive(flag);
			buildingSkillsGroup.Display();
		}
		else
		{
			buildingSkillsGroup.Display(show: false);
		}
	}

	public void SelectFirstBuildingCapacity()
	{
		RegisterAllCapacity();
		if (joystickSkillBar.NumberOfSkills != 0)
		{
			joystickSkillBar.SelectFirstSkill();
		}
	}

	public void SelectNextBuildingCapacity(bool next)
	{
		RegisterAllCapacity();
		if (joystickSkillBar.NumberOfSkills != 0)
		{
			joystickSkillBar.SelectNextSkill(next, 0);
		}
	}

	public void Submit()
	{
		((BuildingCapacityPanel)JoystickSkillBar.CurrentSkillSelected)?.SelectConfirmButton();
	}

	private void RegisterAllCapacity()
	{
		joystickSkillBar.Clear();
		if (buildingDestroyGroup.IsDisplayed())
		{
			joystickSkillBar.Register(buildingDestroyGroup.BuildingCapacities.Where((BuildingCapacityPanel capacity) => capacity.IsDisplayed()));
		}
		if (buildingRepairGroup.IsDisplayed())
		{
			joystickSkillBar.Register(buildingRepairGroup.BuildingCapacities.Where((BuildingCapacityPanel capacity) => capacity.IsDisplayed()));
		}
		if (buildingActionsGroup.IsDisplayed())
		{
			joystickSkillBar.Register(buildingActionsGroup.BuildingCapacities.Where((BuildingCapacityPanel capacity) => capacity.IsDisplayed()));
		}
		if (buildingUpgradesGroup.IsDisplayed())
		{
			joystickSkillBar.Register(buildingUpgradesGroup.BuildingCapacities.Where((BuildingCapacityPanel capacity) => capacity.IsDisplayed()));
		}
		if (buildingGlobalUpgradesGroup.IsDisplayed())
		{
			joystickSkillBar.Register(buildingGlobalUpgradesGroup.BuildingCapacities.Where((BuildingCapacityPanel capacity) => capacity.IsDisplayed()));
		}
		if (buildingSkillsGroup.IsDisplayed())
		{
			joystickSkillBar.Register(buildingSkillsGroup.BuildingCapacities.Where((BuildingCapacityPanel capacity) => capacity.IsDisplayed()));
		}
	}

	private void Awake()
	{
		for (int num = buildingSkillsGroup.BuildingCapacities.Count - 1; num >= 0; num--)
		{
			((BuildingSkillPanel)buildingSkillsGroup.BuildingCapacities[num]).Clicked += OnSkillPanelClicked;
		}
		TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent.AddListener(OnUiScaleSettingsChangeEvent);
		rectTransform = (RectTransform)base.transform;
	}

	private void OnUiScaleSettingsChangeEvent(float value)
	{
		StartCoroutine(RefreshScrollingCoroutine());
	}

	private IEnumerator RefreshScrollingCoroutine()
	{
		rectTransform.GetWorldCorners(rectTransformWorldCorners);
		RectTransform obj = GameView.BottomScreenPanel.BuildingManagementPanel.transform as RectTransform;
		float x = obj.sizeDelta.x;
		float x2 = obj.anchoredPosition.x;
		float num = (float)Screen.width * (1f / TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale);
		rectTransform.sizeDelta = new Vector2(num - x - x2, rectTransform.sizeDelta.y);
		int i = 0;
		while (i < 2)
		{
			yield return SharedYields.WaitForEndOfFrame;
			int num2 = i + 1;
			i = num2;
		}
		bool activeSelf = scrollbar.gameObject.activeSelf;
		buildingCapacitiesLayoutGroup.padding.bottom = (activeSelf ? scrollbarEnabledBottomPadding : scrollbarDisabledBottomPadding);
		if (gamepadInputDisplay_NextSkillScrollEnabled.activeSelf || gamepadInputDisplay_NextSkillScrollDisabled.activeSelf)
		{
			gamepadInputDisplay_NextSkillScrollEnabled.SetActive(activeSelf);
			gamepadInputDisplay_NextSkillScrollDisabled.SetActive(!activeSelf);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(buildingCapacitiesRectTransform);
	}

	private void OnDestroy()
	{
		for (int num = buildingSkillsGroup.BuildingCapacities.Count - 1; num >= 0; num--)
		{
			((BuildingSkillPanel)buildingSkillsGroup.BuildingCapacities[num]).Clicked -= OnSkillPanelClicked;
		}
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent.RemoveListener(OnUiScaleSettingsChangeEvent);
		}
	}

	private void OnSkillPanelClicked(BuildingSkillPanel sender)
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night || TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			for (int num = buildingSkillsGroup.BuildingCapacities.Count - 1; num >= 0; num--)
			{
				BuildingSkillPanel buildingSkillPanel = (BuildingSkillPanel)buildingSkillsGroup.BuildingCapacities[num];
				buildingSkillPanel.DisplaySelector(sender == buildingSkillPanel);
			}
			Game.E_State state = TPSingleton<GameManager>.Instance.Game.State;
			if (state == Game.E_State.Management || state == Game.E_State.BuildingPreparingSkill)
			{
				TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonClickAudioClip);
				BuildingManager.SelectedSkill = sender.Skill;
				Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
				SkillManager.RefreshSelectedSkillValidityOnTile(tile);
				BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionView.DisplayAreaOfEffect(tile);
			}
		}
	}
}
