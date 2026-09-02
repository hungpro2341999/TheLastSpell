using System;
using Sirenix.OdinInspector;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Skill.UI;

public class SkillDisplayButton : SerializedMonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IJoystickSelect
{
	private static class Constants
	{
		public const string SkillBarHotkeyFormatKey = "SkillBarHotkey_Format";
	}

	[SerializeField]
	private SkillDisplay skillDisplay;

	[SerializeField]
	private bool opensSkillInfoPanel;

	[SerializeField]
	private bool interactiveIfRequirementsNotMet = true;

	[SerializeField]
	private BetterButton button;

	[SerializeField]
	private GameObject selector;

	[SerializeField]
	private GameObject highlight;

	[SerializeField]
	private GameObject hotkeyContainer;

	[SerializeField]
	private TextMeshProUGUI hotkeyText;

	[SerializeField]
	private FollowElement.FollowDatas followData = new FollowElement.FollowDatas();

	private bool skillHasChanged;

	private TheLastStand.Model.Skill.Skill previousSkill;

	public bool HasFocus { get; private set; }

	public TheLastStand.Model.Skill.Skill Skill => skillDisplay.Skill;

	public ISkillCaster SkillOwner => skillDisplay.SkillOwner;

	public event Action<SkillDisplayButton> Clicked;

	public void OnDisplayTooltip(bool display)
	{
		DisplayInfoPanel(display);
	}

	public void OnSkillHover(bool select)
	{
		TheLastStand.Model.Unit.Unit selectedUnit = TileObjectSelectionManager.SelectedUnit;
		if (!(selectedUnit is PlayableUnit selectedUnit2))
		{
			if (selectedUnit is EnemyUnit && select)
			{
				if (InputManager.JoystickConfig.HUDNavigation.AlwaysShowTooltipOnPlayableSkillHovering || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
				{
					OnPointerEnter();
				}
				else if (!TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
				{
					ShowRangeTiles();
				}
			}
			else
			{
				OnPointerExit();
			}
		}
		else if (select)
		{
			bool flag = TPSingleton<PlayableUnitManager>.Instance.SelectSkill(Skill, selectedUnit2);
			if ((InputManager.JoystickConfig.HUDNavigation.AlwaysShowTooltipOnPlayableSkillHovering && !flag) || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
			{
				OnPointerEnter();
			}
			else if (!TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
			{
				ShowRangeTiles();
			}
			TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.OnSkillHovered(this);
		}
		else
		{
			OnPointerExit();
			if (PlayableUnitManager.SelectedSkill != null && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver)
			{
				PlayableUnitManager.SelectedSkill = null;
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData = null)
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingExecutingSkill && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitExecutingSkill)
		{
			ShowRangeTiles();
			DisplayInfoPanel(display: true);
		}
	}

	public void OnPointerExit(PointerEventData eventData = null)
	{
		HideRangeTiles();
		DisplayInfoPanel(display: false);
	}

	public void Refresh(bool fullRefresh = false)
	{
		RefreshSkillHotkey();
		skillDisplay.Refresh(fullRefresh);
	}

	public void SetSkill(TheLastStand.Model.Skill.Skill skill, ISkillCaster skillOwner)
	{
		skillHasChanged = skillDisplay.Skill != null;
		previousSkill = skillDisplay.Skill;
		skillDisplay.SkillOwner = skillOwner;
		skillDisplay.Skill = skill;
		SetButtonInteractivity();
		if (skill != null && HasFocus)
		{
			if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
			{
				DisplayInfoPanel(display: true);
			}
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management)
			{
				Skill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
				OnPointerEnter();
			}
		}
	}

	private void Awake()
	{
		button.onClick.AddListener(OnButtonClicked);
		button.OnPointerEnterEvent.AddListener(delegate
		{
			TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonHoverAudioClip);
		});
	}

	private void OnButtonClicked()
	{
		this.Clicked?.Invoke(this);
	}

	private void RefreshSkillHotkey()
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitExecutingSkill)
		{
			return;
		}
		if (InputManager.IsLastControllerJoystick || !TPSingleton<SettingsManager>.Instance.Settings.ShowSkillsHotkeys)
		{
			hotkeyContainer.SetActive(value: false);
			return;
		}
		string[] localizedHotkeysForAction = InputManager.GetLocalizedHotkeysForAction(TPSingleton<PlayableUnitManager>.Instance.GetSkillHotkey(skillDisplay.Skill));
		if (localizedHotkeysForAction != null && localizedHotkeysForAction.Length != 0)
		{
			hotkeyText.text = string.Format(Localizer.Get("SkillBarHotkey_Format"), localizedHotkeysForAction[0]);
			hotkeyContainer.SetActive(value: true);
		}
		else
		{
			hotkeyContainer.SetActive(value: false);
		}
	}

	private void DisplayInfoPanel(bool display)
	{
		if (opensSkillInfoPanel && TPSingleton<SkillManager>.Exist())
		{
			if (display)
			{
				SkillManager.SkillInfoPanel.FollowElement.ChangeFollowDatas(followData);
				SkillManager.SkillInfoPanel.SetContent(Skill, SkillOwner);
				SkillManager.SkillInfoPanel.DisplayInvalidityPanel = true;
			}
			else
			{
				SkillManager.SkillInfoPanel.SetContent(null);
			}
			SkillManager.SkillInfoPanel.CompendiumFollowRight = TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet;
			SkillManager.SkillInfoPanel.Display();
		}
	}

	private void HideRangeTiles()
	{
		HasFocus = false;
		highlight?.SetActive(value: false);
		EventSystem.current.SetSelectedGameObject(null);
		if (EnemyUnitManager.PreviewedSkill != null)
		{
			EnemyUnitManager.PreviewedSkill = null;
		}
		if (TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution != null)
		{
			if (TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution.Skill != PlayableUnitManager.SelectedSkill)
			{
				TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution.SkillExecutionController.HideSkillTargetingForTargets();
				if (TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution.Skill.SkillAction is ResupplySkillAction resupplySkillAction && (resupplySkillAction.HasEffect("ResupplyCharges") || resupplySkillAction.HasEffect("ResupplyOverallUses")))
				{
					resupplySkillAction.ResupplySkillActionExecution.ResupplySkillActionExecutionView.HideDisplayedHUD();
				}
			}
			TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution = null;
		}
		if (PlayableUnitManager.SelectedSkill != Skill && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingExecutingSkill && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitExecutingSkill)
		{
			Skill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
			if (PlayableUnitManager.SelectedSkill != null)
			{
				PlayableUnitManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(SkillOwner);
				PlayableUnitManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.RestoreTargets();
			}
			else if (TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
			}
		}
	}

	private void OnDestroy()
	{
		button.onClick.RemoveListener(OnButtonClicked);
		button.OnPointerEnterEvent.RemoveAllListeners();
	}

	private void OnDisable()
	{
		if (HasFocus)
		{
			DisplayInfoPanel(display: false);
			Skill?.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
		}
		HasFocus = false;
	}

	public void Select(bool isSelected)
	{
		if (isSelected)
		{
			Skill.SkillAction.SkillActionExecution.SkillExecutionController.ForgetSavedTargets();
		}
		selector.SetActive(isSelected);
	}

	private void SetButtonInteractivity()
	{
		if (Skill == null)
		{
			return;
		}
		if (skillDisplay.SkillOwner is PlayableUnit playableUnit)
		{
			bool interactable = interactiveIfRequirementsNotMet || (!playableUnit.IsDead && !playableUnit.PreventedSkillsIds.Contains(Skill.SkillDefinition.Id) && Skill.SkillController.CanExecuteSkill(playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ActionPoints).FinalClamped, playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.MovePoints).FinalClamped, playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Mana).FinalClamped, playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Health).FinalClamped, playableUnit.IsStunned));
			button.Interactable = interactable;
			if (Skill.SkillDefinition.IsContextual)
			{
				button.Interactable &= Skill.SkillController.ComputeTargetsAndValidity(playableUnit);
			}
		}
		else if (skillDisplay.SkillOwner is EnemyUnit)
		{
			button.Interactable = !skillDisplay.SkillOwner.PreventedSkillsIds.Contains(Skill.SkillDefinition.Id);
		}
	}

	private void ShowRangeTiles()
	{
		HasFocus = true;
		highlight?.SetActive(!button.Interactable);
		if (InputManager.IsLastControllerJoystick && TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.JoystickSkillBar.IsOnSkillBarSelection())
		{
			EventSystem.current.SetSelectedGameObject(button.gameObject);
		}
		if ((TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitPreparingSkill) && TileObjectSelectionManager.HasPlayableUnitSelected && PlayableUnitManager.SelectedSkill != Skill)
		{
			TileMapView.ClearTiles(TileMapView.ReachableTilesTilemap);
			if (PlayableUnitManager.SelectedSkill != null)
			{
				if (PlayableUnitManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.HasTargets)
				{
					PlayableUnitManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.SaveTargets();
				}
				PlayableUnitManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
			}
			if (skillHasChanged && previousSkill != null)
			{
				skillHasChanged = false;
				previousSkill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
			}
			Skill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(SkillOwner);
			TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution = Skill.SkillAction.SkillActionExecution;
		}
		else if (TileObjectSelectionManager.HasEnemyUnitSelected)
		{
			Skill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(SkillOwner);
			EnemyUnitManager.PreviewedSkill = Skill;
			Skill.SkillAction.SkillActionExecution.SkillExecutionController.DisplayInRangeTiles();
		}
	}
}
