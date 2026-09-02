using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.Unit;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Animation;
using TheLastStand.Model.Unit;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Unit;

public class UnitLevelUpView : TPSingleton<UnitLevelUpView>, IOverlayUser
{
	public enum E_LevelUpShownStat
	{
		Main,
		Secondary
	}

	public static class Constants
	{
		public const string RerollAudioClipsFolderPath = "Sounds/SFX/UI_Reroll/UI_Reroll_LevelUp";
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private GenericTooltipDisplayer tooltipDisplayer;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private TextMeshProUGUI mainAttributesTitle;

	[SerializeField]
	private RectTransform mainStatBoxParent;

	[SerializeField]
	private TextMeshProUGUI secondaryAttributesTitle;

	[SerializeField]
	private RectTransform secondaryStatBoxParent;

	[SerializeField]
	private Image statBoxesImageMask;

	[SerializeField]
	private Mask statBoxesMask;

	[SerializeField]
	private UnitLevelUpStatView statBoxPrefab;

	[SerializeField]
	private BetterButton showMainStatButton;

	[SerializeField]
	private BetterButton showSecondaryStatButton;

	[SerializeField]
	private CanvasGroup rerollCanvasGroup;

	[SerializeField]
	private BetterButton rerollButton;

	[SerializeField]
	private TextMeshProUGUI rerollCountText;

	[SerializeField]
	private CanvasGroup rerollSinkCanvasGroup;

	[SerializeField]
	private BetterButton rerollSinkButton;

	[SerializeField]
	private TextMeshProUGUI rerollSinkCostText;

	[SerializeField]
	private GameObject damnedSoulsCountContainer;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsCountText;

	[SerializeField]
	private Vector2TweenAnimation toggleMainStatTweenDatas = new Vector2TweenAnimation();

	[SerializeField]
	private Vector2TweenAnimation toggleMainStatTitleTweenDatas = new Vector2TweenAnimation();

	[SerializeField]
	private Vector2TweenAnimation toggleSecondaryStatTweenDatas = new Vector2TweenAnimation();

	[SerializeField]
	private Vector2TweenAnimation toggleSecondaryStatTitleTweenDatas = new Vector2TweenAnimation();

	[SerializeField]
	private float validateCloseDelay = 1f;

	[SerializeField]
	private AudioSource levelUpAudioSource;

	[SerializeField]
	private AudioClip[] levelUpClips;

	[SerializeField]
	private DataColor validRemainingRerollColor;

	[SerializeField]
	private DataColor invalidRemainingRerollColor;

	[SerializeField]
	private DataColor damnedSoulsColor;

	[SerializeField]
	private LayoutNavigationInitializer mainStatsNavigationInitializer;

	[SerializeField]
	private LayoutNavigationInitializer secondaryStatsNavigationInitializer;

	[SerializeField]
	private HUDJoystickSimpleTarget hudTarget;

	[SerializeField]
	private JoystickSelectableDynamic traitsToSecondaryAttributesLeft;

	[SerializeField]
	private JoystickSelectableDynamic traitsToSecondaryAttributesRight;

	[SerializeField]
	private JoystickSelectableDynamic leftBottomToRightPanel;

	private bool initialized;

	private Tween fadeTween;

	private RectTransform mainAttributesTitleTransform;

	private RectTransform secondaryAttributesTitleTransform;

	private readonly List<UnitLevelUpStatView> mainStatBoxes = new List<UnitLevelUpStatView>();

	private readonly List<UnitLevelUpStatView> secondaryStatBoxes = new List<UnitLevelUpStatView>();

	private ToggleGroup mainStatBoxToggleGroup;

	private ToggleGroup secondStatBoxToggleGroup;

	private float moveDuration;

	private bool isChangingBox;

	private bool avoidRefresh;

	private bool changedFromButton;

	private int lastUnlockPerkClipIndex = -1;

	private AudioClip[] rerollAudioClips;

	public HUDJoystickSimpleTarget HudTarget => hudTarget;

	public E_LevelUpShownStat CurrentLevelUpShownStat { get; private set; }

	public bool IsOpened { get; private set; }

	public bool IsProceedingToALevelUp { get; private set; }

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public UnitLevelUp UnitLevelUp { get; set; }

	public void Init()
	{
		if (!TPSingleton<UnitLevelUpView>.Instance.initialized)
		{
			initialized = true;
			canvas.enabled = false;
			canvasGroup.blocksRaycasts = false;
			IsOpened = false;
			mainStatBoxToggleGroup = mainStatBoxParent.GetComponent<ToggleGroup>();
			secondStatBoxToggleGroup = secondaryStatBoxParent.GetComponent<ToggleGroup>();
			mainAttributesTitleTransform = mainAttributesTitle.GetComponent<RectTransform>();
			secondaryAttributesTitleTransform = secondaryAttributesTitle.GetComponent<RectTransform>();
			rerollAudioClips = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/UI_Reroll/UI_Reroll_LevelUp");
			HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
		}
	}

	public void Close(bool instant = false, bool fromLastPointUsed = false)
	{
		if (!IsOpened)
		{
			return;
		}
		CLoggerManager.Log("UnitLevelUpView closed", this, LogType.Log, CLogLevel.DETAILED);
		avoidRefresh = false;
		if (UnitLevelUpController.CanCloseUnitLevelUpView)
		{
			GameController.SetState(Game.E_State.CharacterSheet);
			fadeTween?.Kill();
			ToggleStatTooltipDisplayers(state: false);
			mainStatBoxParent.gameObject.SetActive(value: true);
			secondaryStatBoxParent.gameObject.SetActive(value: true);
			TransformExtensions.DestroyChildren(mainStatBoxParent);
			TransformExtensions.DestroyChildren(secondaryStatBoxParent);
			mainStatBoxes.Clear();
			secondaryStatBoxes.Clear();
			IsOpened = false;
			if (instant)
			{
				canvasGroup.alpha = 0f;
				OnPanelClosed();
			}
			else
			{
				fadeTween = canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.InBack).OnComplete(OnPanelClosed);
			}
			canvasGroup.interactable = false;
			TPSingleton<CharacterSheetPanel>.Instance.RefreshCharacterDetailsNotif(TileObjectSelectionManager.SelectedPlayableUnit);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshUnitHeader(TileObjectSelectionManager.SelectedPlayableUnit);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshOpenedPage();
		}
		void OnPanelClosed()
		{
			canvas.enabled = false;
			canvasGroup.blocksRaycasts = false;
			if (fromLastPointUsed && InputManager.IsLastControllerJoystick)
			{
				PlayableUnit playableUnit = UnitLevelUp.PlayableUnit;
				if (playableUnit != null && playableUnit.PerksPoints > 0)
				{
					TPSingleton<CharacterSheetPanel>.Instance.OpenPerkTree();
					TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<CharacterSheetPanel>.Instance.PerksJoystickTarget.GetSelectionInfo());
				}
				else
				{
					TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(GameView.CharacterDetailsView.SecondaryAttributesHUDJoystickTarget.GetSelectionInfo());
				}
			}
		}
	}

	public void Open()
	{
		if (IsOpened)
		{
			TPSingleton<CharacterSheetPanel>.Instance.OpenUnitDetails();
		}
		else
		{
			if (UnitLevelUp == null)
			{
				return;
			}
			CLoggerManager.Log("UnitLevelUpView opened", this, LogType.Log, CLogLevel.DETAILED);
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CharacterSheet)
			{
				TPSingleton<CharacterSheetPanel>.Instance.Open();
				GameController.SetState(Game.E_State.CharacterSheet);
				TPSingleton<CharacterSheetPanel>.Instance.RefreshTabs();
			}
			if (simpleFontLocalizedParent != null)
			{
				simpleFontLocalizedParent.RefreshChildren();
			}
			TPSingleton<CharacterSheetPanel>.Instance.OpenUnitDetails();
			fadeTween?.Kill();
			IsOpened = true;
			canvas.enabled = true;
			canvasGroup.interactable = true;
			canvasGroup.alpha = 0f;
			if (InputManager.IsLastControllerJoystick)
			{
				EventSystem.current.SetSelectedGameObject(null);
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			}
			fadeTween = canvasGroup.DOFade(1f, 0.25f).OnComplete(delegate
			{
				ToggleStatTooltipDisplayers(state: true);
				if (InputManager.IsLastControllerJoystick)
				{
					TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(HudTarget.GetSelectionInfo());
				}
			});
			tooltipDisplayer.LocalizationArguments = new object[1] { UnitLevelUp.UnitLevelUpDefinition.MaxAmountOfReroll };
			Reinitialize();
			RefreshJoystickNavigation();
		}
	}

	public void OnCloseButtonClick()
	{
		Close();
	}

	public void OnRerollButtonClick()
	{
		SoundManager.PlayAudioClip(rerollAudioClips.PickRandom());
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
			UnitLevelUp.UnitLevelUpController.DrawAvailableMainStats();
			break;
		case E_LevelUpShownStat.Secondary:
			UnitLevelUp.UnitLevelUpController.DrawAvailableSecondaryStats();
			break;
		}
		Refresh();
		if (!InputManager.IsLastControllerJoystick)
		{
			return;
		}
		if (!rerollButton.Interactable)
		{
			if (!UnitLevelUp.UnitLevelUpController.CanReroll())
			{
				EventSystem.current.SetSelectedGameObject((CurrentLevelUpShownStat == E_LevelUpShownStat.Main) ? mainStatBoxes[0].gameObject : secondaryStatBoxes[0].gameObject);
			}
			else if (rerollSinkButton.Interactable)
			{
				EventSystem.current.SetSelectedGameObject(rerollSinkButton.gameObject);
			}
		}
		StartCoroutine(OnJoystickRerollCoroutine());
	}

	public void OnToggleStatButtonClick()
	{
		if (!isChangingBox)
		{
			changedFromButton = true;
			StartCoroutine(ToggleMainAndSecondaryStatBoxes());
		}
	}

	public void Reinitialize()
	{
		if (UnitLevelUp == null)
		{
			Close();
			return;
		}
		UnitLevelUp.UnitLevelUpController.DeselectStat();
		TPSingleton<SinkManager>.Instance.AttributeSinkData.Rerolls = UnitLevelUp.SinkNbReroll;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.interactable = true;
		Refresh(forceToggleToMainStat: true);
	}

	public void RefreshRerollButton()
	{
		bool flag = UnitLevelUp.UnitLevelUpController.IsInFreeRerollMode();
		rerollCanvasGroup.Display(flag);
		rerollSinkCanvasGroup.Display(!flag);
		rerollButton.Interactable = UnitLevelUp.UnitLevelUpController.CanRerollFree();
		rerollSinkButton.Interactable = UnitLevelUp.UnitLevelUpController.CanRerollWithEssence();
		if (flag)
		{
			rerollCountText.text = $"x{UnitLevelUp.CommonNbReroll}";
			rerollCountText.color = ((UnitLevelUp.CommonNbReroll > 0) ? validRemainingRerollColor._Color : invalidRemainingRerollColor._Color);
		}
		else
		{
			rerollSinkCostText.text = $"{UnitLevelUp.DamnedSoulsRerollCost} <style=\"DamnedSouls\">";
			rerollSinkCostText.color = (UnitLevelUp.UnitLevelUpController.CanRerollWithEssence() ? damnedSoulsColor._Color : invalidRemainingRerollColor._Color);
		}
		damnedSoulsCountContainer.SetActive(!flag);
		if (!flag)
		{
			damnedSoulsCountText.text = $"{ApplicationManager.Application.DamnedSouls} <style=\"DamnedSouls\">";
		}
	}

	private void InitializeLevelUpStat(UnitLevelUp.SelectedStatToLevelUp stat, bool isMainStat, bool isFirst)
	{
		UnitLevelUpStatView unitLevelUpStatView = Object.Instantiate(statBoxPrefab, isMainStat ? mainStatBoxParent : secondaryStatBoxParent);
		unitLevelUpStatView.InitializeToggle();
		if (isMainStat)
		{
			mainStatBoxes.Add(unitLevelUpStatView);
		}
		else
		{
			secondaryStatBoxes.Add(unitLevelUpStatView);
		}
		ImprovedToggle component = unitLevelUpStatView.GetComponent<ImprovedToggle>();
		component.group = (isMainStat ? mainStatBoxToggleGroup : secondStatBoxToggleGroup);
		unitLevelUpStatView.OnConfirmBoxClicked.AddListener(OnValidateButtonClick);
		component.OnBeforePointerClickEvent.AddListener(delegate(PointerEventData eventData)
		{
			OnBeforePointerClickedStatBox(eventData, unitLevelUpStatView);
		});
		component.onValueChanged.AddListener(delegate(bool value)
		{
			OnStatBoxSelectedChanged(unitLevelUpStatView, value);
		});
		unitLevelUpStatView.StatBonus = stat;
		unitLevelUpStatView.TargetUnit = UnitLevelUp.PlayableUnit;
		if (isFirst)
		{
			if (isMainStat)
			{
				traitsToSecondaryAttributesLeft.Selectables[0] = component;
				traitsToSecondaryAttributesRight.Selectables[0] = component;
				if (leftBottomToRightPanel != null)
				{
					leftBottomToRightPanel.Selectables[0] = component;
				}
			}
			else
			{
				traitsToSecondaryAttributesLeft.Selectables[1] = component;
				traitsToSecondaryAttributesRight.Selectables[1] = component;
				if (leftBottomToRightPanel != null)
				{
					leftBottomToRightPanel.Selectables[1] = component;
				}
			}
		}
		unitLevelUpStatView.Refresh();
	}

	private IEnumerator OnJoystickRerollCoroutine()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		yield return SharedYields.WaitForSeconds(InputManager.JoystickConfig.HUDNavigation.HighlightTweenDuration);
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
	}

	private void OnTooltipsToggled(bool state)
	{
		if (!IsOpened)
		{
			return;
		}
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
		{
			for (int num2 = mainStatBoxes.Count - 1; num2 >= 0; num2--)
			{
				if (EventSystem.current.currentSelectedGameObject == mainStatBoxes[num2].gameObject)
				{
					mainStatBoxes[num2].StatTooltipDisplayer.DisplayTooltip(state);
					break;
				}
			}
			break;
		}
		case E_LevelUpShownStat.Secondary:
		{
			for (int num = secondaryStatBoxes.Count - 1; num >= 0; num--)
			{
				if (EventSystem.current.currentSelectedGameObject == secondaryStatBoxes[num].gameObject)
				{
					secondaryStatBoxes[num].StatTooltipDisplayer.DisplayTooltip(state);
					break;
				}
			}
			break;
		}
		}
	}

	private void RefreshJoystickNavigation()
	{
		mainStatsNavigationInitializer.InitNavigation();
		secondaryStatsNavigationInitializer.InitNavigation();
		foreach (UnitLevelUpStatView mainStatBox in mainStatBoxes)
		{
			mainStatBox.Selectable.SetMode(Navigation.Mode.Explicit);
			if (rerollButton.Interactable)
			{
				mainStatBox.Selectable.SetSelectOnLeft(rerollButton);
			}
			else if (rerollSinkButton.Interactable)
			{
				mainStatBox.Selectable.SetSelectOnLeft(rerollSinkButton);
			}
		}
		foreach (UnitLevelUpStatView secondaryStatBox in secondaryStatBoxes)
		{
			secondaryStatBox.Selectable.SetMode(Navigation.Mode.Explicit);
			if (rerollButton.Interactable)
			{
				secondaryStatBox.Selectable.SetSelectOnLeft(rerollButton);
			}
			else if (rerollSinkButton.Interactable)
			{
				secondaryStatBox.Selectable.SetSelectOnLeft(rerollSinkButton);
			}
		}
		RefreshRerollButtonJoystickNavigation();
		HudTarget.ClearMissingSelectables();
		HudTarget.AddSelectables(mainStatBoxes.Select((UnitLevelUpStatView o) => o.Selectable));
		HudTarget.AddSelectables(secondaryStatBoxes.Select((UnitLevelUpStatView o) => o.Selectable));
	}

	private void RefreshRerollButtonJoystickNavigation()
	{
		rerollButton.SetMode(Navigation.Mode.Explicit);
		rerollButton.SetSelectOnRight((CurrentLevelUpShownStat == E_LevelUpShownStat.Main) ? mainStatBoxes[0].Selectable : secondaryStatBoxes[0].Selectable);
		rerollSinkButton.SetMode(Navigation.Mode.Explicit);
		rerollSinkButton.SetSelectOnRight((CurrentLevelUpShownStat == E_LevelUpShownStat.Main) ? mainStatBoxes[0].Selectable : secondaryStatBoxes[0].Selectable);
	}

	private void Update()
	{
		if (IsOpened)
		{
			if (InputManager.GetButtonDown(91) && (!fadeTween.IsActive() || !fadeTween.IsPlaying()) && !TPSingleton<CharacterSheetPanel>.Instance.IsInventoryOpened && !TPSingleton<CharacterSheetPanel>.Instance.IsPerksPanelOpened && UnitLevelUp.UnitLevelUpController.CanReroll())
			{
				OnRerollButtonClick();
				EventSystem.current.SetSelectedGameObject((CurrentLevelUpShownStat == E_LevelUpShownStat.Main) ? mainStatBoxes[0].gameObject : secondaryStatBoxes[0].gameObject);
			}
			if ((InputManager.GetButtonDown(92) && CurrentLevelUpShownStat == E_LevelUpShownStat.Main && UnitLevelUp.PlayableUnit.SecondaryStatsPoints > 0) || (InputManager.GetButtonDown(93) && CurrentLevelUpShownStat == E_LevelUpShownStat.Secondary && UnitLevelUp.PlayableUnit.MainStatsPoints > 0))
			{
				EventSystem.current.SetSelectedGameObject(null);
				OnToggleStatButtonClick();
			}
			if (InputManager.GetButtonDown(29) && IsOpened)
			{
				OnCloseButtonClick();
			}
		}
	}

	private void MoveStatsBox()
	{
		toggleSecondaryStatTweenDatas.StatusTransitionTween.Kill();
		toggleMainStatTweenDatas.StatusTransitionTween.Kill();
		toggleSecondaryStatTitleTweenDatas.StatusTransitionTween.Kill();
		toggleMainStatTitleTweenDatas.StatusTransitionTween.Kill();
		bool flag = CurrentLevelUpShownStat == E_LevelUpShownStat.Main;
		moveDuration = Mathf.Max(toggleSecondaryStatTweenDatas.TransitionDuration, toggleMainStatTweenDatas.TransitionDuration, toggleSecondaryStatTitleTweenDatas.TransitionDuration, toggleMainStatTitleTweenDatas.TransitionDuration);
		toggleSecondaryStatTweenDatas.StatusTransitionTween = secondaryStatBoxParent.DOAnchorPos(flag ? toggleSecondaryStatTweenDatas.StatusTwo : toggleSecondaryStatTweenDatas.StatusOne, toggleSecondaryStatTweenDatas.TransitionDuration).SetEase(toggleSecondaryStatTweenDatas.TransitionEase);
		toggleMainStatTweenDatas.StatusTransitionTween = mainStatBoxParent.DOAnchorPos(flag ? toggleMainStatTweenDatas.StatusOne : toggleMainStatTweenDatas.StatusTwo, toggleMainStatTweenDatas.TransitionDuration).SetEase(toggleMainStatTweenDatas.TransitionEase);
		toggleSecondaryStatTitleTweenDatas.StatusTransitionTween = secondaryAttributesTitleTransform.DOAnchorPos(flag ? toggleSecondaryStatTitleTweenDatas.StatusTwo : toggleSecondaryStatTitleTweenDatas.StatusOne, toggleSecondaryStatTitleTweenDatas.TransitionDuration).SetEase(toggleSecondaryStatTitleTweenDatas.TransitionEase);
		toggleMainStatTitleTweenDatas.StatusTransitionTween = mainAttributesTitleTransform.DOAnchorPos(flag ? toggleMainStatTitleTweenDatas.StatusOne : toggleMainStatTitleTweenDatas.StatusTwo, toggleMainStatTitleTweenDatas.TransitionDuration).SetEase(toggleMainStatTitleTweenDatas.TransitionEase);
	}

	private void OnBeforePointerClickedStatBox(PointerEventData e, UnitLevelUpStatView unitLevelUpStatView)
	{
		if (canvasGroup.interactable)
		{
			if (unitLevelUpStatView.StatBoxToggle.isOn && e.button == PointerEventData.InputButton.Left)
			{
				unitLevelUpStatView.StatBoxToggle.ShouldExecuteOnPointerClickEvent = false;
				OnValidateButtonClick();
			}
			else
			{
				unitLevelUpStatView.StatBoxToggle.ShouldExecuteOnPointerClickEvent = true;
			}
		}
	}

	private void OnDestroy()
	{
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	private void OnStatBoxSelectedChanged(UnitLevelUpStatView sender, bool selected)
	{
		if (InputManager.IsLastControllerJoystick && EventSystem.current.currentSelectedGameObject == sender.gameObject && !selected)
		{
			OnValidateButtonClick();
			return;
		}
		if (selected)
		{
			UnitLevelUp.UnitLevelUpController.SelectStat(sender.StatBonus);
		}
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
		{
			foreach (UnitLevelUpStatView mainStatBox in mainStatBoxes)
			{
				mainStatBox.Select(selected && sender == mainStatBox, selected);
			}
			break;
		}
		case E_LevelUpShownStat.Secondary:
		{
			foreach (UnitLevelUpStatView secondaryStatBox in secondaryStatBoxes)
			{
				secondaryStatBox.Select(selected && sender == secondaryStatBox, selected);
			}
			break;
		}
		}
	}

	private void OnValidateButtonClick()
	{
		if (!canvasGroup.interactable)
		{
			return;
		}
		IsProceedingToALevelUp = true;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.interactable = false;
		int num = UnityEngine.Random.Range(0, levelUpClips.Length);
		if (levelUpClips.Length > 1)
		{
			while (num == lastUnlockPerkClipIndex)
			{
				num = UnityEngine.Random.Range(0, levelUpClips.Length);
			}
		}
		lastUnlockPerkClipIndex = num;
		levelUpAudioSource.PlayOneShot(levelUpClips[num]);
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
		{
			UnitLevelUp.UnitLevelUpController.ValidateStatToIncrease(isValidatingMainStat: true);
			int j = 0;
			for (int count2 = mainStatBoxes.Count; j < count2; j++)
			{
				mainStatBoxes[j].Validate(UnitLevelUp.HasSelectedStat && mainStatBoxes[j].StatBonus.Equals(UnitLevelUp.SelectedStat));
			}
			break;
		}
		case E_LevelUpShownStat.Secondary:
		{
			UnitLevelUp.UnitLevelUpController.ValidateStatToIncrease(isValidatingMainStat: false);
			int i = 0;
			for (int count = secondaryStatBoxes.Count; i < count; i++)
			{
				secondaryStatBoxes[i].Validate(UnitLevelUp.HasSelectedStat && secondaryStatBoxes[i].StatBonus.Equals(UnitLevelUp.SelectedStat));
			}
			break;
		}
		default:
			TPSingleton<PlayableUnitManager>.Instance.LogError("Tried to validate a level up stat which is nor main nor secondary!");
			break;
		}
		avoidRefresh = true;
		TPSingleton<CharacterSheetPanel>.Instance.RefreshStats();
		StartCoroutine(WaitCloseAfterValidate());
	}

	private void Refresh(bool forceToggleToMainStat = false)
	{
		mainAttributesTitle.text = Localizer.Format((UnitLevelUp.PlayableUnit.MainStatsPoints > 1) ? "UnitLevelUpPanel_MainTitle_SeveralLevels" : "UnitLevelUpPanel_MainTitle", UnitLevelUp.PlayableUnit.MainStatsPoints);
		secondaryAttributesTitle.text = Localizer.Format((UnitLevelUp.PlayableUnit.SecondaryStatsPoints > 1) ? "UnitLevelUpPanel_SecondaryTitle_SeveralLevels" : "UnitLevelUpPanel_SecondaryTitle", UnitLevelUp.PlayableUnit.SecondaryStatsPoints);
		if (avoidRefresh)
		{
			avoidRefresh = false;
			return;
		}
		if (UnitLevelUp.AvailableMainStats.Count == 0 && UnitLevelUp.PlayableUnit.MainStatsPoints > 0)
		{
			UnitLevelUp.UnitLevelUpController.DrawAvailableStats();
		}
		if (UnitLevelUp.AvailableSecondaryStats.Count == 0 && UnitLevelUp.PlayableUnit.SecondaryStatsPoints > 0)
		{
			UnitLevelUp.UnitLevelUpController.DrawAvailableStats(isDrawingMainStat: false);
		}
		statBoxesImageMask.enabled = true;
		statBoxesMask.enabled = true;
		mainStatBoxParent.gameObject.SetActive(value: true);
		secondaryStatBoxParent.gameObject.SetActive(value: true);
		TransformExtensions.DestroyChildren(mainStatBoxParent);
		TransformExtensions.DestroyChildren(secondaryStatBoxParent);
		mainStatBoxes.Clear();
		secondaryStatBoxes.Clear();
		int i = 0;
		for (int count = UnitLevelUp.AvailableMainStats.Count; i < count; i++)
		{
			InitializeLevelUpStat(UnitLevelUp.AvailableMainStats[i], isMainStat: true, i == 0);
		}
		int j = 0;
		for (int count2 = UnitLevelUp.AvailableSecondaryStats.Count; j < count2; j++)
		{
			InitializeLevelUpStat(UnitLevelUp.AvailableSecondaryStats[j], isMainStat: false, j == 0);
		}
		RefreshRerollButton();
		if ((CurrentLevelUpShownStat == E_LevelUpShownStat.Main && UnitLevelUp.AvailableMainStats.Count == 0) || (CurrentLevelUpShownStat == E_LevelUpShownStat.Secondary && UnitLevelUp.AvailableSecondaryStats.Count == 0) || (forceToggleToMainStat && CurrentLevelUpShownStat == E_LevelUpShownStat.Secondary && UnitLevelUp.AvailableMainStats.Count != 0))
		{
			StartCoroutine(ToggleMainAndSecondaryStatBoxes());
		}
		showMainStatButton.gameObject.SetActive(CurrentLevelUpShownStat == E_LevelUpShownStat.Secondary && UnitLevelUp.PlayableUnit.MainStatsPoints > 0);
		showSecondaryStatButton.gameObject.SetActive(CurrentLevelUpShownStat == E_LevelUpShownStat.Main && UnitLevelUp.PlayableUnit.SecondaryStatsPoints > 0);
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
			secondaryStatBoxParent.gameObject.SetActive(value: false);
			break;
		case E_LevelUpShownStat.Secondary:
			mainStatBoxParent.gameObject.SetActive(value: false);
			break;
		}
		statBoxesImageMask.enabled = false;
		statBoxesMask.enabled = false;
		RefreshJoystickNavigation();
	}

	private IEnumerator ToggleMainAndSecondaryStatBoxes()
	{
		isChangingBox = true;
		canvasGroup.interactable = false;
		if (changedFromButton)
		{
			switch (CurrentLevelUpShownStat)
			{
			case E_LevelUpShownStat.Main:
				mainStatBoxToggleGroup.SetAllTogglesOff();
				break;
			case E_LevelUpShownStat.Secondary:
				secondStatBoxToggleGroup.SetAllTogglesOff();
				break;
			}
		}
		changedFromButton = false;
		statBoxesImageMask.enabled = true;
		statBoxesMask.enabled = true;
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
			secondaryStatBoxParent.gameObject.SetActive(value: true);
			break;
		case E_LevelUpShownStat.Secondary:
			mainStatBoxParent.gameObject.SetActive(value: true);
			break;
		}
		CurrentLevelUpShownStat = ((CurrentLevelUpShownStat == E_LevelUpShownStat.Main) ? E_LevelUpShownStat.Secondary : E_LevelUpShownStat.Main);
		MoveStatsBox();
		ToggleStatTooltipDisplayers(state: false);
		RefreshRerollButtonJoystickNavigation();
		showMainStatButton.gameObject.SetActive(CurrentLevelUpShownStat == E_LevelUpShownStat.Secondary && UnitLevelUp.PlayableUnit.MainStatsPoints > 0);
		showSecondaryStatButton.gameObject.SetActive(CurrentLevelUpShownStat == E_LevelUpShownStat.Main && UnitLevelUp.PlayableUnit.SecondaryStatsPoints > 0);
		canvasGroup.interactable = true;
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject(null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
		yield return null;
		yield return SharedYields.WaitForSeconds(moveDuration);
		ToggleStatTooltipDisplayers(state: true);
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
			secondaryStatBoxParent.gameObject.SetActive(value: false);
			break;
		case E_LevelUpShownStat.Secondary:
			mainStatBoxParent.gameObject.SetActive(value: false);
			break;
		}
		if (InputManager.IsLastControllerJoystick)
		{
			yield return SharedYields.WaitForEndOfFrame;
			switch (CurrentLevelUpShownStat)
			{
			case E_LevelUpShownStat.Main:
				if (mainStatBoxes.Count > 0)
				{
					EventSystem.current.SetSelectedGameObject(mainStatBoxes[0].gameObject);
				}
				break;
			case E_LevelUpShownStat.Secondary:
				if (secondaryStatBoxes.Count > 0)
				{
					EventSystem.current.SetSelectedGameObject(secondaryStatBoxes[0].gameObject);
				}
				break;
			}
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ForcePositionUpdate();
			RefreshJoystickNavigation();
		}
		statBoxesImageMask.enabled = false;
		statBoxesMask.enabled = false;
		isChangingBox = false;
	}

	private void ToggleStatTooltipDisplayers(bool state)
	{
		List<UnitLevelUpStatView> list = new List<UnitLevelUpStatView>(mainStatBoxes);
		list.AddRange(secondaryStatBoxes);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			list[num].StatTooltipDisplayer.gameObject.SetActive(state);
			if (!state)
			{
				list[num].StatTooltipDisplayer.DisplayTooltip(display: false);
			}
		}
	}

	private IEnumerator WaitCloseAfterValidate()
	{
		yield return SharedYields.WaitForSeconds(validateCloseDelay);
		UnitLevelUp unitLevelUp = UnitLevelUp;
		if (unitLevelUp != null && unitLevelUp.PlayableUnit?.StatsPoints > 0)
		{
			switch (CurrentLevelUpShownStat)
			{
			case E_LevelUpShownStat.Main:
			{
				UnitLevelUp unitLevelUp3 = UnitLevelUp;
				if (unitLevelUp3 != null && unitLevelUp3.PlayableUnit?.MainStatsPoints > 0)
				{
					UnitLevelUp.UnitLevelUpController.DrawAvailableStats();
					avoidRefresh = false;
					Refresh();
					canvasGroup.interactable = true;
					if (InputManager.IsLastControllerJoystick)
					{
						yield return SharedYields.WaitForEndOfFrame;
						EventSystem.current.SetSelectedGameObject(mainStatBoxes[0].gameObject);
					}
				}
				else
				{
					avoidRefresh = false;
					StartCoroutine(ToggleMainAndSecondaryStatBoxes());
				}
				break;
			}
			case E_LevelUpShownStat.Secondary:
			{
				UnitLevelUp unitLevelUp2 = UnitLevelUp;
				if (unitLevelUp2 != null && unitLevelUp2.PlayableUnit?.SecondaryStatsPoints > 0)
				{
					UnitLevelUp.UnitLevelUpController.DrawAvailableStats(isDrawingMainStat: false);
					avoidRefresh = false;
					Refresh();
					canvasGroup.interactable = true;
					if (InputManager.IsLastControllerJoystick)
					{
						yield return SharedYields.WaitForEndOfFrame;
						EventSystem.current.SetSelectedGameObject(secondaryStatBoxes[0].gameObject);
					}
				}
				else
				{
					avoidRefresh = false;
					StartCoroutine(ToggleMainAndSecondaryStatBoxes());
				}
				break;
			}
			}
		}
		else
		{
			UnitLevelUp.CommonNbReroll = 0;
			avoidRefresh = false;
			Close(instant: false, fromLastPointUsed: true);
		}
		IsProceedingToALevelUp = false;
	}
}
