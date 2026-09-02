using System;
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
using TheLastStand.Database;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.Apocalypse;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD;
using TheLastStand.View.NightReport;
using TheLastStand.View.SoulsReward;
using TheLastStand.View.Tooltip;
using TheLastStand.View.WorldMap;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View;

public class GameOverPanel : TPSingleton<GameOverPanel>, IOverlayUser
{
	[SerializeField]
	private float panelDeltaPosY = -20f;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private Image gameOverPanel;

	[SerializeField]
	private GameObject pageOne;

	[SerializeField]
	private GameObject pageTwo;

	[SerializeField]
	private GameObject skipButtonParent;

	[FormerlySerializedAs("continueButtonParent")]
	[SerializeField]
	private GameObject buttonParent;

	[SerializeField]
	private SoulsRewardPanel soulsRewardPanel;

	[SerializeField]
	private TextMeshProUGUI nextMessage;

	[FormerlySerializedAs("victoryBG")]
	[SerializeField]
	private Sprite victoryBGPageTwo;

	[SerializeField]
	private Sprite victoryBGPageOne;

	[FormerlySerializedAs("defeatBG")]
	[SerializeField]
	private Sprite defeatBGPageTwo;

	[SerializeField]
	private Sprite defeatBGPageOne;

	[SerializeField]
	private float blackBGFadeInDuration = 0.6f;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("From the end of easy mode and apocalypse appearing")]
	private float waitBeforeShowingPlayableReports;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("From the popup appearing")]
	private float waitBeforeShowingEasyModeAndApocalypse;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("From the end of playable reports appearing")]
	private float waitBeforeShowingStats;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("From the end of stats appearing")]
	private float waitBeforeShowingUnlocks;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("From the end of unlocks appearing")]
	private float waitBeforeShowingContinue;

	[SerializeField]
	private Ease unfoldEasing = Ease.InCubic;

	[SerializeField]
	[Range(0f, 5f)]
	private float unfoldDuration = 0.4f;

	[SerializeField]
	private TextMeshProUGUI gameOverTitle;

	[SerializeField]
	private TextMeshProUGUI cityTitle;

	[SerializeField]
	private Image cityPortrait;

	[SerializeField]
	private Image cityPortraitBox;

	[SerializeField]
	private Sprite cityPortraitBoxDefeat;

	[SerializeField]
	private Sprite cityPortraitBoxVictory;

	[SerializeField]
	private Sprite modifierBoxDefeat;

	[SerializeField]
	private Sprite modifierBoxVictory;

	[SerializeField]
	private RectTransform glyphsRectTransform;

	[SerializeField]
	private float glyphsUnfoldedPositionY;

	[SerializeField]
	private TextMeshProUGUI glyphsText;

	[SerializeField]
	private Image glyphsBox;

	[SerializeField]
	private TextMeshProUGUI glyphsCustomModeText;

	[SerializeField]
	private HUDJoystickTarget glyphsJoystickTarget;

	[SerializeField]
	private JoystickSelectable glyphsSelectable;

	[SerializeField]
	private JoystickSelectable weaponRestrictionsSelectable;

	[SerializeField]
	private RectTransform apocalypseRectTransform;

	[SerializeField]
	private ApocalypseLevelView chosenApocalypseView;

	[SerializeField]
	private float apocalypseUnfoldedPositionY;

	[SerializeField]
	private Image apocalypseModeBox;

	[SerializeField]
	private ApocalypseEffectsTooltip currentApocalypseTooltip;

	[SerializeField]
	private HUDJoystickTarget apocalypseJoystickTarget;

	[SerializeField]
	private JoystickSelectable apocalypseSelectable;

	[SerializeField]
	private PlayableReportDisplay playableReportPrefab;

	[SerializeField]
	private RectTransform playableReportParent;

	[SerializeField]
	private RectTransform playableBoardMask;

	[SerializeField]
	private CanvasGroup playableReportCanvasGroup;

	[SerializeField]
	private GameObject playableReportLeftButton;

	[SerializeField]
	private GameObject playableReportRightButton;

	[SerializeField]
	private HUDJoystickSimpleTarget playableJoystickTarget;

	[SerializeField]
	private LayoutNavigationInitializer playableNavigationInitializer;

	[SerializeField]
	private CanvasGroup statsCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI statsTitle;

	[SerializeField]
	private TextMeshProUGUI firstStatLine;

	[SerializeField]
	private TextMeshProUGUI secondStatLine;

	[SerializeField]
	private TextMeshProUGUI thirdStatLine;

	[SerializeField]
	private ApocalypseGameOverUnlockView apocalypseGameOverUnlockView;

	[SerializeField]
	private CanvasGroup unlocksCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI unlockTitle;

	[SerializeField]
	private GameObject unlockParticlesSquare;

	[SerializeField]
	private GameObject unlockParticlesRays;

	[SerializeField]
	private GameObject unlockApocalypseSystemContainer;

	[SerializeField]
	private GameObject unlockApocalypseRewardContainer;

	[SerializeField]
	private ApocalypseRewardAtLevelTooltipDisplayer apocalypseRewardTooltipDisplayer;

	[SerializeField]
	private HUDJoystickTarget unlockJoystickTarget;

	[SerializeField]
	private JoystickSelectable unlocksSelectable;

	[SerializeField]
	private AudioSource apocalypseUnlockAudioSource;

	[SerializeField]
	private AudioSource panelOpenAudioSource;

	[SerializeField]
	private CanvasGroup continueCanvasGroup;

	[SerializeField]
	private CanvasGroup backCanvasGroup;

	private Canvas canvas;

	private CanvasGroup canvasGroup;

	private bool firstTimeOpened = true;

	private bool isOpened;

	private Tween moveTween;

	private int pageIndex;

	private float posYInit;

	private List<PlayableReportDisplay> playableReports = new List<PlayableReportDisplay>();

	private float unlocksAlphaTarget = 1f;

	private bool alreadySeenPlayableReport;

	private bool haveUnlocksToShow;

	private List<int> apocalypseLevelsRewardsToDisplay = new List<int>();

	public int OverlaySortingOrder => TPSingleton<GameOverPanel>.Instance.canvas.sortingOrder - 2;

	public Canvas Canvas => TPSingleton<GameOverPanel>.Instance.canvas;

	public void Close(bool toAnotherPopup = false)
	{
		if (isOpened)
		{
			CLoggerManager.Log("GameOverPanel closed", this, LogType.Log, CLogLevel.DETAILED);
			if (!toAnotherPopup)
			{
				CameraView.AttenuateWorldForPopupFocus(null);
			}
			if (moveTween != null)
			{
				moveTween.Kill();
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f);
			}
			isOpened = false;
			moveTween = rectTransform.DOAnchorPosY(posYInit, 0.25f).SetEase(Ease.InBack).SetFullId("GameOverPanelClose", this)
				.OnComplete(delegate
				{
					canvas.enabled = false;
					soulsRewardPanel.ClearTrophies();
				});
			canvasGroup.blocksRaycasts = false;
			alreadySeenPlayableReport = false;
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		}
	}

	public void OnLeftPlayableReportButtonClick()
	{
		TPSingleton<GameOverPanel>.Instance.playableReportParent.localPosition = new Vector3(TPSingleton<GameOverPanel>.Instance.playableReportParent.localPosition.x + 219f, TPSingleton<GameOverPanel>.Instance.playableReportParent.localPosition.y, TPSingleton<GameOverPanel>.Instance.playableReportParent.localPosition.z);
	}

	public void OnMainMenuClick()
	{
		firstTimeOpened = true;
		GameController.GoBackToMainMenu();
	}

	public void OnNextClick()
	{
		SetPageIndex(pageIndex + 1);
	}

	public void OnBackClick()
	{
		if (pageIndex != 0)
		{
			SetPageIndex(pageIndex - 1);
		}
	}

	public void Open()
	{
		if (!isOpened)
		{
			CLoggerManager.Log("GameOverPanel opened", this, LogType.Log, CLogLevel.DETAILED);
			CameraView.AttenuateWorldForPopupFocus(this);
			if (moveTween != null)
			{
				moveTween.Kill();
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posYInit);
			}
			moveTween = rectTransform.DOAnchorPosY(panelDeltaPosY, 0.25f).SetEase(Ease.OutBack).SetFullId("GameOverPanelOpen", this);
			OnOpenRelease();
			simpleFontLocalizedParent?.RefreshChildren();
			RefreshText();
			canvas.enabled = true;
			canvasGroup.blocksRaycasts = true;
			isOpened = true;
			firstTimeOpened = false;
		}
	}

	public void OnRestartClick()
	{
		GameManager.DebugReloadGameScene();
	}

	public void OnRightPlayableReportButtonClick()
	{
		TPSingleton<GameOverPanel>.Instance.playableReportParent.localPosition = new Vector3(TPSingleton<GameOverPanel>.Instance.playableReportParent.localPosition.x - 219f, TPSingleton<GameOverPanel>.Instance.playableReportParent.localPosition.y, TPSingleton<GameOverPanel>.Instance.playableReportParent.localPosition.z);
	}

	public void RefreshPlayables()
	{
		foreach (PlayableReportDisplay playableReport in playableReports)
		{
			playableReport.RefreshView();
		}
	}

	public void SelectPlayablePanelJoystick()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(playableJoystickTarget.GetSelectionInfo());
		}
	}

	public void SkipAnimations()
	{
		Time.timeScale = 50f;
		skipButtonParent.SetActive(value: false);
	}

	public void OnTrophiesAnimationEnd()
	{
		Time.timeScale = 1f;
		skipButtonParent.SetActive(value: false);
		buttonParent.SetActive(value: true);
	}

	public void Update()
	{
		if (isOpened && Canvas.enabled)
		{
			if (InputManager.GetButtonDown(7))
			{
				OnNextClick();
			}
			else if (InputManager.GetButtonDown(80))
			{
				OnBackClick();
			}
		}
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshText();
		}
	}

	private void RefreshText()
	{
		string text = string.Empty;
		switch (TPSingleton<GameManager>.Instance.Game.GameOverCause)
		{
		case Game.E_GameOverCause.MagicSealsCompleted:
			text = Localizer.Get("GameOverPanel_Cause_MagicSealsCompleted");
			break;
		case Game.E_GameOverCause.MagicCircleDestroyed:
			text = Localizer.Get("GameOverPanel_Cause_MagicCircleDestroyed");
			break;
		case Game.E_GameOverCause.HeroesDeath:
			text = Localizer.Get("GameOverPanel_Cause_HeroesDeath");
			break;
		}
		cityTitle.text = $"{TPSingleton<WorldMapCityManager>.Instance.SelectedCity?.CityDefinition.Name} #{TPSingleton<WorldMapCityManager>.Instance.SelectedCity?.NumberOfRuns}";
		gameOverTitle.text = text;
		nextMessage.text = Localizer.Get("GameOverPanel_Next");
		glyphsText.text = Localizer.Get("Glyphs_Title");
		statsTitle.text = Localizer.Get("GameOverPanel_StatsTitle");
		firstStatLine.text = Localizer.Format("GameOverPanel_FirstStatLine", TPSingleton<GameManager>.Instance.Game.DayNumber);
		TimeSpan timeSpan = TimeSpan.FromSeconds(TPSingleton<GameManager>.Instance.TotalTimeSpent);
		secondStatLine.text = Localizer.Format("GameOverPanel_SecondStatLine", Math.Floor(timeSpan.TotalHours), timeSpan.Minutes, timeSpan.Seconds);
		thirdStatLine.text = Localizer.Format("GameOverPanel_ThirdStatLine", ApplicationManager.Application.RunsCompleted);
		unlockTitle.text = Localizer.Get("GameOverPanel_UnlocksTitle");
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		canvas = TPSingleton<GameOverPanel>.Instance.GetComponent<Canvas>();
		canvas.enabled = false;
		canvasGroup = TPSingleton<GameOverPanel>.Instance.GetComponent<CanvasGroup>();
		canvasGroup.blocksRaycasts = false;
		isOpened = false;
		posYInit = rectTransform.anchoredPosition.y;
		buttonParent.SetActive(value: false);
	}

	private bool CheckIfThereAreUnlocksToShow(out bool isSystemUnlock)
	{
		haveUnlocksToShow = false;
		apocalypseLevelsRewardsToDisplay.Clear();
		int highestLevelReached = ApocalypseManager.ApocalypseStateBeforeGameOver.HighestLevelReached;
		int highestApocalypseLevelReached = ApocalypseManager.GetHighestApocalypseLevelReached();
		if (!TPSingleton<GameManager>.Instance.Game.IsVictory || (!ApocalypseManager.ApocalypseStateBeforeGameOver.WasApocalypseUnlocked && ApocalypseManager.ApocalypseStateBeforeGameOver.WasApocalypseUnlocked == ApocalypseManager.IsApocalypseUnlocked) || (ApocalypseManager.CurrentApocalypseLevel > 0 && highestLevelReached == highestApocalypseLevelReached))
		{
			isSystemUnlock = false;
			return haveUnlocksToShow;
		}
		if (!ApocalypseManager.ApocalypseStateBeforeGameOver.WasApocalypseUnlocked && ApocalypseManager.IsApocalypseUnlocked)
		{
			haveUnlocksToShow = true;
			unlockApocalypseRewardContainer.SetActive(value: false);
			unlockApocalypseSystemContainer.SetActive(value: true);
			isSystemUnlock = true;
			return haveUnlocksToShow;
		}
		unlockApocalypseSystemContainer.SetActive(value: false);
		List<int> apocalypseLevelsForRewards = ApocalypseDatabase.GetApocalypseLevelsForRewards();
		for (int i = 0; i < apocalypseLevelsForRewards.Count; i++)
		{
			int num = apocalypseLevelsForRewards[i];
			if (num <= highestApocalypseLevelReached && highestLevelReached < num)
			{
				apocalypseLevelsRewardsToDisplay.Add(num);
				haveUnlocksToShow = true;
			}
		}
		if (apocalypseLevelsRewardsToDisplay.Count > 0)
		{
			unlockApocalypseRewardContainer.SetActive(value: true);
			apocalypseRewardTooltipDisplayer.Init(null, null, apocalypseLevelsRewardsToDisplay.ToArray());
		}
		isSystemUnlock = false;
		return haveUnlocksToShow;
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnOpenRelease()
	{
		bool isVictory = TPSingleton<GameManager>.Instance.Game.IsVictory;
		cityPortrait.sprite = ResourcePooler<Sprite>.LoadOnce("View/Sprites/UI/Cities/Portraits/WorldMap_CityPortrait_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + (isVictory ? "02" : "03"));
		cityPortraitBox.sprite = (isVictory ? cityPortraitBoxVictory : cityPortraitBoxDefeat);
		glyphsBox.sprite = (isVictory ? modifierBoxVictory : modifierBoxDefeat);
		apocalypseModeBox.sprite = (isVictory ? modifierBoxVictory : modifierBoxDefeat);
		gameOverPanel.sprite = (isVictory ? victoryBGPageTwo : defeatBGPageOne);
		SetPageIndex(0);
		if (!isVictory)
		{
			CanvasFadeManager.FadeIn(blackBGFadeInDuration, canvas.sortingOrder - 1);
		}
		soulsRewardPanel.RefreshPosition();
		soulsRewardPanel.Display(firstTimeOpenedThisNight: true);
		chosenApocalypseView.Init(ApocalypseManager.CurrentApocalypseLevel);
		currentApocalypseTooltip.SetApocalypseModifierStepDefinitions(ApocalypseManager.CurrentApocalypseModifierStepDefinitions);
		if (CheckIfThereAreUnlocksToShow(out var isSystemUnlock))
		{
			apocalypseGameOverUnlockView.Init(hasUnlock: true, isSystemUnlock);
			unlocksAlphaTarget = 1f;
			unlockJoystickTarget.NavigationEnabled = true;
		}
		else
		{
			apocalypseGameOverUnlockView.Init(hasUnlock: false, isSystemUnlock);
			unlocksCanvasGroup.blocksRaycasts = false;
			unlockJoystickTarget.NavigationEnabled = false;
			unlocksAlphaTarget = 0.5f;
		}
		if (firstTimeOpened)
		{
			playableReportCanvasGroup.alpha = 0f;
			statsCanvasGroup.alpha = 0f;
			unlocksCanvasGroup.alpha = 0f;
			continueCanvasGroup.alpha = 0f;
			continueCanvasGroup.interactable = false;
			StartCoroutine(ShowSoulsRewardPanel());
			playableReportLeftButton.SetActive(playableReportParent.sizeDelta.x > playableBoardMask.sizeDelta.x);
			playableReportRightButton.SetActive(playableReportParent.sizeDelta.x > playableBoardMask.sizeDelta.x);
		}
	}

	private void SetPageIndex(int value)
	{
		int num = pageIndex;
		pageIndex = value;
		TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		switch (pageIndex)
		{
		case 0:
			pageOne.SetActive(value: true);
			gameOverPanel.sprite = (TPSingleton<GameManager>.Instance.Game.IsVictory ? victoryBGPageOne : defeatBGPageOne);
			backCanvasGroup.gameObject.SetActive(value: false);
			pageTwo.SetActive(value: false);
			if (num == 1)
			{
				soulsRewardPanel.Display(firstTimeOpenedThisNight: false);
			}
			break;
		case 1:
			soulsRewardPanel.Hide();
			pageOne.SetActive(value: false);
			gameOverPanel.sprite = (TPSingleton<GameManager>.Instance.Game.IsVictory ? victoryBGPageTwo : defeatBGPageTwo);
			backCanvasGroup.gameObject.SetActive(value: true);
			pageTwo.SetActive(value: true);
			if (!alreadySeenPlayableReport)
			{
				StartCoroutine(ShowPlayableReports());
				break;
			}
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			}
			SelectPlayablePanelJoystick();
			break;
		default:
			TileObjectSelectionManager.DeselectAll();
			firstTimeOpened = true;
			if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsLastMap && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.NumberOfWins == 1)
			{
				ApplicationManager.Application.ApplicationController.SetState("Credits");
			}
			else
			{
				GameController.GoToMetaShops();
			}
			break;
		}
	}

	private IEnumerator ShowSoulsRewardPanel()
	{
		yield return soulsRewardPanel.ShowTrophiesPanel(TPSingleton<GameManager>.Instance.Game.IsDefeat);
		OnTrophiesAnimationEnd();
		StartCoroutine(ShowEasyModeAndApocalypse());
	}

	private IEnumerator ShowEasyModeAndApocalypse()
	{
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Count > 0 || ApocalypseManager.CurrentApocalypseLevel > 0)
		{
			yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingEasyModeAndApocalypse);
			bool flag = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CurrentGlyphPoints > 0;
			bool flag2 = !TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.AreAllUnlockedFamiliesSelected();
			bool flag3 = ApocalypseManager.CurrentApocalypseLevel > 0;
			if (flag || flag2)
			{
				glyphsJoystickTarget.NavigationEnabled = true;
				glyphsRectTransform.DOAnchorPosY(glyphsUnfoldedPositionY, unfoldDuration).SetEase(unfoldEasing).SetFullId("easyModeUnfoldTween", this);
				glyphsSelectable.gameObject.SetActive(flag);
				if (flag)
				{
					glyphsSelectable.SetMode(Navigation.Mode.Explicit);
					glyphsSelectable.SetSelectOnDown(soulsRewardPanel.GetFirstSelectableTrophy());
					glyphsSelectable.SetSelectOnRight(flag2 ? weaponRestrictionsSelectable : (flag3 ? apocalypseSelectable : null));
					apocalypseSelectable.SetSelectOnLeft(glyphsSelectable);
				}
				weaponRestrictionsSelectable.gameObject.SetActive(flag2);
				if (flag2)
				{
					weaponRestrictionsSelectable.SetMode(Navigation.Mode.Explicit);
					weaponRestrictionsSelectable.SetSelectOnDown(soulsRewardPanel.GetFirstSelectableTrophy());
					weaponRestrictionsSelectable.SetSelectOnLeft(flag ? glyphsSelectable : null);
					weaponRestrictionsSelectable.SetSelectOnRight(flag3 ? apocalypseSelectable : null);
					apocalypseSelectable.SetSelectOnLeft(weaponRestrictionsSelectable);
				}
				if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled)
				{
					glyphsCustomModeText.enabled = true;
					glyphsCustomModeText.text = $"+{TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GetCustomModeBonusPoints()}";
				}
			}
			else
			{
				glyphsJoystickTarget.NavigationEnabled = false;
			}
			if (flag3)
			{
				apocalypseRectTransform.DOAnchorPosY(apocalypseUnfoldedPositionY, unfoldDuration).SetEase(unfoldEasing).SetFullId("apocalypseUnfoldTween", this);
				apocalypseJoystickTarget.NavigationEnabled = true;
				apocalypseSelectable.SetMode(Navigation.Mode.Explicit);
				apocalypseSelectable.SetSelectOnDown(soulsRewardPanel.GetFirstSelectableTrophy());
			}
			else
			{
				apocalypseJoystickTarget.NavigationEnabled = false;
			}
			if (flag || flag2 || flag3)
			{
				panelOpenAudioSource.Play();
			}
		}
		else
		{
			glyphsJoystickTarget.NavigationEnabled = false;
			apocalypseJoystickTarget.NavigationEnabled = false;
		}
		StartCoroutine(ShowStats());
	}

	private IEnumerator ShowPlayableReports()
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingPlayableReports);
		int count = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count;
		int i = 0;
		for (int j = playableReports.Count; j < count; j++)
		{
			PlayableReportDisplay playableReportDisplay = UnityEngine.Object.Instantiate(TPSingleton<GameOverPanel>.Instance.playableReportPrefab, TPSingleton<GameOverPanel>.Instance.playableReportParent);
			playableReportDisplay.Init(playableReportParent, playableBoardMask);
			playableReports.Add(playableReportDisplay);
		}
		for (; i < count; i++)
		{
			playableReports[i].gameObject.SetActive(value: true);
			playableReports[i].Refresh(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i], isDead: false, isEndGamePanel: true);
		}
		int num = 0;
		foreach (KeyValuePair<int, List<PlayableUnit>> deadPlayableUnit in TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits)
		{
			for (int k = playableReports.Count; k < i + num + deadPlayableUnit.Value.Count; k++)
			{
				PlayableReportDisplay playableReportDisplay2 = UnityEngine.Object.Instantiate(TPSingleton<GameOverPanel>.Instance.playableReportPrefab, TPSingleton<GameOverPanel>.Instance.playableReportParent);
				playableReportDisplay2.Init(playableReportParent, playableBoardMask);
				playableReports.Add(playableReportDisplay2);
			}
			foreach (PlayableUnit item in deadPlayableUnit.Value)
			{
				playableReports[i + num].gameObject.SetActive(value: true);
				playableReports[i + num].Refresh(item, isDead: true, isEndGamePanel: true, deadPlayableUnit.Key);
				num++;
			}
		}
		for (int l = i + num; l < playableReports.Count; l++)
		{
			playableReports[l].gameObject.SetActive(value: false);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)playableNavigationInitializer.transform);
		playableNavigationInitializer.InitNavigation();
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			playableJoystickTarget.AddSelectables(playableReports.Select((PlayableReportDisplay x) => x.Selectable));
			SelectPlayablePanelJoystick();
		}
		playableReportCanvasGroup.DOFade(1f, 1f).SetEase(Ease.OutCubic).SetFullId("PlayableReportFadeIn", this);
		alreadySeenPlayableReport = true;
	}

	private IEnumerator ShowStats()
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingStats);
		statsCanvasGroup.DOFade(1f, 1f).SetEase(Ease.OutCubic).SetFullId("StatsFadeIn", this);
		StartCoroutine(ShowUnlocks());
	}

	private IEnumerator ShowUnlocks()
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingUnlocks);
		unlockParticlesSquare.SetActive(haveUnlocksToShow);
		unlockParticlesRays.SetActive(haveUnlocksToShow);
		unlocksCanvasGroup.DOFade(unlocksAlphaTarget, 1f).SetEase(Ease.OutCubic).SetFullId("UnlocksFadeIn", this);
		if (haveUnlocksToShow)
		{
			apocalypseUnlockAudioSource.Play();
		}
		unlocksSelectable.SetMode(Navigation.Mode.Explicit);
		unlocksSelectable.SetSelectOnUp(soulsRewardPanel.GetFirstSelectableTrophy());
		StartCoroutine(ShowContinueButton());
	}

	private IEnumerator ShowContinueButton()
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingContinue);
		continueCanvasGroup.DOFade(1f, 1f).SetEase(Ease.OutCubic).SetFullId("ContinueFadeIn", this)
			.OnComplete(delegate
			{
				continueCanvasGroup.interactable = true;
			});
	}

	[ContextMenu("Open")]
	public void DebugOpen()
	{
		if (!UnityEngine.Application.isPlaying)
		{
			TPDebug.LogError("Unable to use this context menu when the application is not running", this);
		}
		else if (!TPSingleton<GameOverPanel>.Instance.isOpened)
		{
			Open();
		}
	}

	[ContextMenu("Close")]
	public void DebugClose()
	{
		if (!UnityEngine.Application.isPlaying)
		{
			TPDebug.LogError("Unable to use this context menu when the application is not running", this);
		}
		else if (TPSingleton<GameOverPanel>.Instance.isOpened)
		{
			Close();
		}
	}
}
