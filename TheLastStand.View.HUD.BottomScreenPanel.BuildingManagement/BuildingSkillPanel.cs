using System;
using TMPro;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Skill;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingSkillPanel : BuildingCapacityPanel
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private BuildingSkillHighlight highlightRaycaster;

	[SerializeField]
	private RectTransform joystickHighlighterRectTransform;

	[SerializeField]
	private Sprite skillButtonDisabledSprite;

	[SerializeField]
	private RectTransform skillJoystickHighlighterRectTransform;

	[SerializeField]
	private Sprite goalSkillButtonDisabledSprite;

	[SerializeField]
	private E_GamepadButtonType goalSkillButtonGamepadButtonTypes;

	[SerializeField]
	private RectTransform goalSkillJoystickHighlighterRectTransform;

	[SerializeField]
	private GameObject nightUsesContainer;

	[SerializeField]
	private TextMeshProUGUI nightUsesCountText;

	[SerializeField]
	private GameObject usesRemainingContainer;

	[SerializeField]
	private TextMeshProUGUI usesRemainingLeftText;

	[SerializeField]
	private DataColor noUsesRemainingColor;

	private bool displayed;

	private Color usesRemainingBaseColor;

	private bool isGoalSkill;

	public BetterButton Button => button;

	public TheLastStand.Model.Skill.Skill Skill { get; set; }

	public TheLastStand.Model.Building.Building SkillOwner { get; set; }

	public event Action<BuildingSkillPanel> Clicked;

	public void Display(bool show)
	{
		displayed = show;
		base.BuildingCapacityRect.gameObject.SetActive(show);
	}

	public override void DisplayTooltip(bool show)
	{
		if (show)
		{
			SkillTooltip skillInfoPanel = SkillManager.SkillInfoPanel;
			skillInfoPanel.FollowElement.ChangeTarget(base.transform);
			skillInfoPanel.SetContent(Skill, SkillOwner.BattleModule);
			skillInfoPanel.DisplayInvalidityPanel = true;
			skillInfoPanel.Display();
		}
		else
		{
			SkillManager.SkillInfoPanel.Hide();
		}
	}

	public override void OnSkillPanelHovered(bool hover)
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Management && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingSkill)
		{
			return;
		}
		if (hover)
		{
			DisplayTooltip(show: true);
			TileMapView.ClearTiles(TileMapView.ReachableTilesTilemap);
			if (BuildingManager.SelectedSkill != null)
			{
				if (BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.HasTargets)
				{
					BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.SaveTargets();
				}
				BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
			}
			if (!isGoalSkill)
			{
				Skill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(SkillOwner.BattleModule);
			}
			else
			{
				SkillOwner.BuildingView.HideSkillRangeIfNeeded();
				SkillOwner.BuildingView.DisplaySkillRangeIfNeeded(displayHandledSkill: false, Skill);
			}
			BuildingManager.PreviewedSkill = Skill;
			return;
		}
		if (isGoalSkill)
		{
			SkillOwner.BuildingView.HideSkillRangeIfNeeded();
		}
		if (BuildingManager.PreviewedSkill != null)
		{
			BuildingManager.PreviewedSkill = null;
		}
		if (BuildingManager.SelectedSkill != Skill)
		{
			Skill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
		}
		if (BuildingManager.SelectedSkill != null)
		{
			BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(SkillOwner.BattleModule);
			BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.RestoreTargets();
		}
		DisplayTooltip(show: false);
	}

	public override void OnSkillHover(bool select)
	{
		base.OnSkillHover(select);
		bool flag = TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips || InputManager.JoystickConfig.HUDNavigation.AlwaysShowTooltipOnBuildingSkill;
		if (select)
		{
			if (!flag && isGoalSkill)
			{
				SkillOwner.BuildingView.HideSkillRangeIfNeeded();
				SkillOwner.BuildingView.DisplaySkillRangeIfNeeded(displayHandledSkill: false, Skill);
			}
			if (Button.Interactable)
			{
				BuildingManager.SelectedSkill = Skill;
				Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
				SkillManager.RefreshSelectedSkillValidityOnTile(tile);
				BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionView.DisplayAreaOfEffect(tile);
			}
		}
		else
		{
			if (!flag && isGoalSkill)
			{
				SkillOwner.BuildingView.HideSkillRangeIfNeeded();
			}
			BuildingManager.SelectedSkill = null;
		}
	}

	public override void Refresh()
	{
		if (displayed)
		{
			if (!isGoalSkill)
			{
				Button.Interactable = Skill.SkillController.CanExecuteSkill(-1f, -1f, -1f, -1f, isStun: false);
			}
			else
			{
				Button.Interactable = false;
			}
			highlightRaycaster.gameObject.SetActive(!Button.Interactable);
			icon.sprite = SkillView.GetIconSprite(Skill.SkillDefinition.ArtId);
			nightUsesContainer.SetActive(Skill.OverallUses > 0);
			if (nightUsesContainer.activeSelf)
			{
				nightUsesCountText.text = Skill.OverallUsesRemaining.ToString();
				nightUsesCountText.color = ((Skill.OverallUsesRemaining == 0) ? noUsesRemainingColor._Color : usesRemainingBaseColor);
			}
			usesRemainingContainer.SetActive(Skill.SkillDefinition.UsesPerTurnCount > -1);
			if (usesRemainingContainer.activeSelf)
			{
				usesRemainingLeftText.text = $"{Skill.UsesPerTurnRemaining}/{Skill.SkillDefinition.UsesPerTurnCount}";
			}
		}
	}

	public void SetIsGoalSkill(bool isGoalSkillValue)
	{
		isGoalSkill = isGoalSkillValue;
		RefreshButtonSettings();
	}

	private void Button_Clicked()
	{
		TPSingleton<BuildingManager>.Instance.Log("Building skill " + Skill.SkillDefinition.Id + " was clicked.", CLogLevel.DETAILED);
		this.Clicked?.Invoke(this);
	}

	private void Button_OnPointerEnter()
	{
		TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonHoverAudioClip);
	}

	private void Awake()
	{
		if (Button != null)
		{
			Button.onClick.AddListener(Button_Clicked);
			Button.OnPointerEnterEvent.AddListener(Button_OnPointerEnter);
		}
		usesRemainingBaseColor = nightUsesCountText.color;
	}

	private void OnDestroy()
	{
		if (Button != null)
		{
			Button.onClick.RemoveListener(Button_Clicked);
			Button.OnPointerEnterEvent.RemoveAllListeners();
		}
	}

	private void RefreshButtonSettings()
	{
		SpriteState spriteState = Button.spriteState;
		if (isGoalSkill)
		{
			spriteState.disabledSprite = goalSkillButtonDisabledSprite;
			joystickHighlighter.SetOverridenGamepadButtonTypes(goalSkillButtonGamepadButtonTypes);
			joystickHighlighterRectTransform.anchoredPosition = goalSkillJoystickHighlighterRectTransform.anchoredPosition;
		}
		else
		{
			spriteState.disabledSprite = skillButtonDisabledSprite;
			joystickHighlighter.ResetOverridenGamepadButtonTypes();
			joystickHighlighterRectTransform.anchoredPosition = skillJoystickHighlighterRectTransform.anchoredPosition;
		}
		Button.spriteState = spriteState;
	}
}
