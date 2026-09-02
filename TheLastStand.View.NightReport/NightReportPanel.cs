using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Controller.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.ProductionReport;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD;
using TheLastStand.View.Panic;
using TheLastStand.View.SoulsReward;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.NightReport;

public class NightReportPanel : TPSingleton<NightReportPanel>, IOverlayUser
{
	public static class Constants
	{
		public const string CommonLocalizedFontChildren = "Common";

		public const string PageOneLocalizedFontChildren = "PageOne";

		public const string PageTwoLocalizedFontChildren = "PageTwo";
	}

	[SerializeField]
	private float panelDeltaPosY = -20f;

	[SerializeField]
	private Animator titleAnimator;

	[SerializeField]
	private DataSpriteTable smallRankStamp;

	[SerializeField]
	private DataSpriteTable bigRankStamp;

	[SerializeField]
	private ComplexFontLocalizedParent complexFontLocalizedParent;

	[SerializeField]
	private Image page1Cache;

	[SerializeField]
	private CanvasGroup xpPanelCanvasGroup;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitAfterTitleAppear = 1f;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitBeforeShowingAllKillsReports = 0.5f;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitBetweenEachKillsReportAppear = 0.3f;

	[SerializeField]
	private KillReportDisplay killReportPrefab;

	[SerializeField]
	private RectTransform killReportParent;

	[SerializeField]
	private RectTransform killsBoardMask;

	[SerializeField]
	private BetterButton killReportLeftButton;

	[SerializeField]
	private BetterButton killReportRightButton;

	[SerializeField]
	[Range(1f, 3f)]
	private float stampPunchStrength = 1.25f;

	[SerializeField]
	[Range(1f, 3f)]
	private float sharedXPPunchStrength = 1.5f;

	[SerializeField]
	[Range(0f, 5f)]
	private float stampPunchTweenDuration = 0.4f;

	[SerializeField]
	[Range(0f, 5f)]
	private float stampFadeTweenDuration = 0.4f;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("Can't be higher than waitBetweenEachKillsReportAppear")]
	private float sharedXPPunchTweenDuration = 0.2f;

	[SerializeField]
	private TextMeshProUGUI sharedXPText;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("From the end of kill reports appearing")]
	private float waitBeforeShowingPlayableReports;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitBeforePlayableXPAnimation = 0.3f;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("Playable XP animation can continue when the next objects appear")]
	private float waitAfterXPAnimationBeginning = 1f;

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
	private ScrollRect playablesScrollRect;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitAfterBottomCanvasAppear = 0.5f;

	[SerializeField]
	private CanvasGroup battleReportCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI unitAliveValueText;

	[SerializeField]
	private TextMeshProUGUI hpLostValueText;

	[SerializeField]
	private TextMeshProUGUI deadUnitValueText;

	[SerializeField]
	private Image battleReportRankStamp;

	[SerializeField]
	private CanvasGroup page1ContinueCanvasGroup;

	[SerializeField]
	private ScrollRect killsScrollRect;

	[SerializeField]
	private CanvasGroup page1SkipCanvasGroup;

	[SerializeField]
	private Image page2Cache;

	[SerializeField]
	private CanvasGroup page2SkipCanvasGroup;

	[SerializeField]
	private CanvasGroup nightRewardCanvasGroup;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitAfterNightRwardCanvasAppear = 1f;

	[SerializeField]
	private PanicPanel nightReportPanicPanel;

	[SerializeField]
	private PanicRewardIndicator panicRewardIndicator;

	[SerializeField]
	private Image panicRankStamp;

	[SerializeField]
	private CanvasGroup nightRewardContainerCanvasGroup;

	[SerializeField]
	private HUDJoystickSimpleTarget rewardJoystickTarget;

	[SerializeField]
	private LayoutNavigationInitializer layoutRewardNavigationInitializer;

	[SerializeField]
	private SoulsRewardPanel soulsRewardPanel;

	[SerializeField]
	private CanvasGroup nightRatingCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI nightCountText;

	[SerializeField]
	private TextMeshProUGUI commandersSentence;

	[SerializeField]
	private Image nightRankStamp;

	[SerializeField]
	private AudioSource audioSourceTemplate;

	[SerializeField]
	private int audioSourcesCount = 5;

	[SerializeField]
	private AudioClip fireAudioClip;

	[SerializeField]
	private AudioClip[] killReportAudioClips;

	[SerializeField]
	private AudioClip gainXPAudioClip;

	[SerializeField]
	private AudioClip stampAudioClip;

	[SerializeField]
	private AudioClip finalStampAudioClip;

	[SerializeField]
	private AudioClip characterAppearAudioClip;

	[SerializeField]
	private AudioClip rewardsAppearAudioClip;

	private Canvas canvas;

	private CanvasGroup canvasGroup;

	private RectTransform rectTransform;

	private Tween moveTween;

	private Tween sharedXPTextTween;

	private float posYInit;

	private int tabIndex;

	private bool firstTimeOpenedThisNight = true;

	private bool isOpened;

	private readonly List<KillReportDisplay> killReports = new List<KillReportDisplay>();

	private readonly List<PlayableReportDisplay> playableReports = new List<PlayableReportDisplay>();

	private bool wantToSkipAnimations;

	private AudioSource[] audioSources;

	private int nextAudioSourceIndex;

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public void Close()
	{
		if (isOpened)
		{
			CLoggerManager.Log("NightReportPanel closed", this, LogType.Log, CLogLevel.DETAILED);
			CameraView.AttenuateWorldForPopupFocus(null);
			if (moveTween != null)
			{
				moveTween.Kill();
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f);
			}
			isOpened = false;
			moveTween = rectTransform.DOAnchorPosY(posYInit, 0.25f).SetEase(Ease.InBack).SetFullId("NightReportPanelClose", this)
				.OnComplete(delegate
				{
					canvas.enabled = false;
					GameView.TopScreenPanel.TurnPanel.Refresh();
					SoundManager.PlayAudioClip(GameManager.AudioSource, GameManager.ProductionPhaseAudioClip);
				});
			firstTimeOpenedThisNight = true;
			canvasGroup.blocksRaycasts = false;
			soulsRewardPanel.ClearTrophies();
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		}
	}

	public void Update()
	{
		if (!isOpened)
		{
			return;
		}
		if (InputManager.GetButtonDown(7))
		{
			if (page1Cache.enabled && page1SkipCanvasGroup.alpha != 0f)
			{
				SkipPageOneAnimations();
			}
			else if (page2Cache.enabled && page2SkipCanvasGroup.alpha != 0f)
			{
				SkipPageTwoAnimations();
			}
			else
			{
				OnContinueClick();
			}
		}
		else if (InputManager.GetButtonDown(80) && page2Cache.enabled && page2SkipCanvasGroup.alpha == 0f)
		{
			OnBackToPage1Click();
		}
	}

	public void Open()
	{
		if (!isOpened)
		{
			CLoggerManager.Log("NightReportPanel opened", this, LogType.Log, CLogLevel.DETAILED);
			CameraView.AttenuateWorldForPopupFocus(this);
			GameView.TopScreenPanel.Display(show: false);
			if (complexFontLocalizedParent != null)
			{
				complexFontLocalizedParent.TargetKey = "Common";
				complexFontLocalizedParent.RefreshChildren();
			}
			if (moveTween != null)
			{
				moveTween.Kill();
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posYInit);
			}
			tabIndex = 0;
			page1SkipCanvasGroup.alpha = 1f;
			page1SkipCanvasGroup.blocksRaycasts = true;
			OpenPage1();
			canvas.enabled = true;
			canvasGroup.blocksRaycasts = true;
			soulsRewardPanel.RefreshPosition();
			isOpened = true;
		}
	}

	public void OnBackToPage1Click()
	{
		tabIndex = 0;
		OpenPage1();
	}

	public void OnContinueClick()
	{
		switch (tabIndex)
		{
		case 0:
			tabIndex = 1;
			OpenPage2();
			break;
		case 1:
			TPSingleton<PlayableUnitManager>.Instance.NightReport.NightReportController.CloseNightReportPanel();
			break;
		}
	}

	public void OnLeftKillsReportButtonClick()
	{
		TPSingleton<NightReportPanel>.Instance.killReportParent.localPosition = new Vector3(TPSingleton<NightReportPanel>.Instance.killReportParent.localPosition.x + 114f, TPSingleton<NightReportPanel>.Instance.killReportParent.localPosition.y, TPSingleton<NightReportPanel>.Instance.killReportParent.localPosition.z);
		TPSingleton<NightReportPanel>.Instance.ToggleKillsReportButtons();
	}

	public void OnRightKillsReportButtonClick()
	{
		TPSingleton<NightReportPanel>.Instance.killReportParent.localPosition = new Vector3(TPSingleton<NightReportPanel>.Instance.killReportParent.localPosition.x - 114f, TPSingleton<NightReportPanel>.Instance.killReportParent.localPosition.y, TPSingleton<NightReportPanel>.Instance.killReportParent.localPosition.z);
		TPSingleton<NightReportPanel>.Instance.ToggleKillsReportButtons();
	}

	public void OnLeftPlayableReportButtonClick()
	{
		TPSingleton<NightReportPanel>.Instance.playableReportParent.localPosition = new Vector3(TPSingleton<NightReportPanel>.Instance.playableReportParent.localPosition.x + 219f, TPSingleton<NightReportPanel>.Instance.playableReportParent.localPosition.y, TPSingleton<NightReportPanel>.Instance.playableReportParent.localPosition.z);
	}

	public void OnRightPlayableReportButtonClick()
	{
		TPSingleton<NightReportPanel>.Instance.playableReportParent.localPosition = new Vector3(TPSingleton<NightReportPanel>.Instance.playableReportParent.localPosition.x - 219f, TPSingleton<NightReportPanel>.Instance.playableReportParent.localPosition.y, TPSingleton<NightReportPanel>.Instance.playableReportParent.localPosition.z);
	}

	public void PlayAudioClip(AudioClip audioClip, float delay = 0f, bool doNotInterrupt = false)
	{
		SoundManager.PlayAudioClip(GetNextAudioSource(), audioClip, delay, doNotInterrupt);
	}

	public void RefreshPlayables()
	{
		foreach (PlayableReportDisplay playableReport in playableReports)
		{
			playableReport.RefreshView();
		}
	}

	public AudioSource GetNextAudioSource()
	{
		return audioSources[nextAudioSourceIndex++ % audioSources.Length];
	}

	public void SkipPageOneAnimations()
	{
		Time.timeScale = 50f;
		wantToSkipAnimations = true;
		page1SkipCanvasGroup.alpha = 0f;
		page1SkipCanvasGroup.blocksRaycasts = false;
	}

	public void SkipPageTwoAnimations()
	{
		Time.timeScale = 50f;
		wantToSkipAnimations = true;
		page2SkipCanvasGroup.alpha = 0f;
		page2SkipCanvasGroup.blocksRaycasts = false;
	}

	private void AddKillReport(int killReportIndex, KillReportData killReportData, ref int totalXP)
	{
		TPSingleton<NightReportPanel>.Instance.killReports[killReportIndex].gameObject.SetActive(value: true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(TPSingleton<NightReportPanel>.Instance.killReportParent);
		if (TPSingleton<NightReportPanel>.Instance.killReportParent.sizeDelta.x > TPSingleton<NightReportPanel>.Instance.killsBoardMask.sizeDelta.x)
		{
			TPSingleton<NightReportPanel>.Instance.killReportParent.localPosition = new Vector3(TPSingleton<NightReportPanel>.Instance.killReportParent.localPosition.x - 1000f, TPSingleton<NightReportPanel>.Instance.killReportParent.localPosition.y, TPSingleton<NightReportPanel>.Instance.killReportParent.localPosition.z);
		}
		int tweenSharedXP = totalXP;
		int num = (int)killReportData.TotalExperienceToShare;
		TPSingleton<NightReportPanel>.Instance.killReports[killReportIndex].Refresh(killReportData.SpecificAssetsId, killReportData.KillAmount, num);
		totalXP += num;
		sharedXPTextTween?.Kill();
		sharedXPTextTween = DOTween.To(() => tweenSharedXP, delegate(int x)
		{
			tweenSharedXP = x;
			TPSingleton<NightReportPanel>.Instance.sharedXPText.text = $"{tweenSharedXP}";
		}, totalXP, waitBetweenEachKillsReportAppear).SetFullId("SharedXPTransfer", this);
		if (!PlayableUnitManager.DebugForceSkipNightReport)
		{
			TPSingleton<NightReportPanel>.Instance.sharedXPText.transform.DOPunchScale(Vector3.one * TPSingleton<NightReportPanel>.Instance.sharedXPPunchStrength, TPSingleton<NightReportPanel>.Instance.sharedXPPunchTweenDuration, 1, 0.1f).SetFullId("SharedXPPunchScale", this);
		}
	}

	private void CollectGoldReward()
	{
		if (PanicManager.Panic.PanicReward.Gold > 0)
		{
			TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + PanicManager.Panic.PanicReward.Gold);
		}
	}

	private void CollectItemReward()
	{
		if (PanicManager.Panic.PanicReward.HasAtLeastOneItem)
		{
			ProductionItems productionItem = new ProductionItemController().ProductionItem;
			productionItem.IsNightProduction = true;
			TheLastStand.Model.Item.Item[] items = PanicManager.Panic.PanicReward.Items;
			foreach (TheLastStand.Model.Item.Item item in items)
			{
				productionItem.Items.Add(item);
			}
			TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportController.AddProductionObject(productionItem);
		}
	}

	private void CollectMaterialsReward()
	{
		if (PanicManager.Panic.PanicReward.Materials > 0)
		{
			TPSingleton<ResourceManager>.Instance.Materials += PanicManager.Panic.PanicReward.Materials;
		}
	}

	private void OpenPage1()
	{
		nightRewardCanvasGroup.alpha = 0f;
		nightRewardCanvasGroup.blocksRaycasts = false;
		nightRatingCanvasGroup.alpha = 0f;
		nightRatingCanvasGroup.blocksRaycasts = false;
		page2SkipCanvasGroup.alpha = 0f;
		page2SkipCanvasGroup.blocksRaycasts = false;
		page2Cache.enabled = false;
		soulsRewardPanel.Hide();
		TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		playablesScrollRect.enabled = true;
		killsScrollRect.enabled = true;
		page1Cache.enabled = true;
		if (firstTimeOpenedThisNight)
		{
			if (complexFontLocalizedParent != null)
			{
				complexFontLocalizedParent.TargetKey = "PageOne";
				complexFontLocalizedParent.RefreshChildren();
			}
			TPSingleton<PlayableUnitManager>.Instance.NightReport.NightReportController.GetTonightRanks();
			playableReportParent.gameObject.SetActive(value: true);
			titleAnimator.SetTrigger("Hide");
			xpPanelCanvasGroup.alpha = 0f;
			xpPanelCanvasGroup.blocksRaycasts = false;
			sharedXPText.text = string.Empty;
			playableReportCanvasGroup.alpha = 0f;
			battleReportCanvasGroup.alpha = 0f;
			battleReportCanvasGroup.blocksRaycasts = false;
			unitAliveValueText.text = string.Empty;
			hpLostValueText.text = string.Empty;
			deadUnitValueText.text = string.Empty;
			battleReportRankStamp.color = new Color(battleReportRankStamp.color.r, battleReportRankStamp.color.g, battleReportRankStamp.color.b, 0f);
			battleReportRankStamp.sprite = smallRankStamp.GetSpriteAt(TPSingleton<PlayableUnitManager>.Instance.NightReport.BattleRank);
			page1ContinueCanvasGroup.alpha = 0f;
			page1ContinueCanvasGroup.blocksRaycasts = false;
			killReportLeftButton.gameObject.SetActive(value: false);
			killReportRightButton.gameObject.SetActive(value: false);
			playableReportLeftButton.SetActive(value: false);
			playableReportRightButton.SetActive(value: false);
			nightReportPanicPanel.Refresh(PanicManager.Panic);
			killReportParent.localPosition = new Vector3(0f, killReportParent.localPosition.y, killReportParent.localPosition.z);
			playableReportParent.localPosition = new Vector3(0f, playableReportParent.localPosition.y, playableReportParent.localPosition.z);
			for (int i = 0; i < killReports.Count; i++)
			{
				killReports[i].gameObject.SetActive(value: false);
			}
			moveTween = rectTransform.DOAnchorPosY(panelDeltaPosY, 0.25f).SetEase(Ease.OutBack).SetFullId("NightReportPanelOpen", this)
				.OnComplete(delegate
				{
					titleAnimator.SetTrigger("FadeIn");
					StartCoroutine(ShowKills());
				});
			PlayAudioClip(fireAudioClip);
		}
		else
		{
			playableReportParent.gameObject.SetActive(value: true);
			xpPanelCanvasGroup.alpha = 1f;
			xpPanelCanvasGroup.blocksRaycasts = true;
			battleReportCanvasGroup.alpha = 1f;
			battleReportCanvasGroup.blocksRaycasts = true;
		}
	}

	private void OpenPage2()
	{
		playableReportParent.gameObject.SetActive(value: false);
		xpPanelCanvasGroup.alpha = 0f;
		xpPanelCanvasGroup.blocksRaycasts = false;
		battleReportCanvasGroup.alpha = 0f;
		battleReportCanvasGroup.blocksRaycasts = false;
		page1Cache.enabled = false;
		playablesScrollRect.enabled = false;
		killsScrollRect.enabled = false;
		soulsRewardPanel.Display(firstTimeOpenedThisNight);
		page2Cache.enabled = true;
		if (firstTimeOpenedThisNight)
		{
			if (complexFontLocalizedParent != null)
			{
				complexFontLocalizedParent.TargetKey = "PageTwo";
				complexFontLocalizedParent.RefreshChildren();
			}
			nightRewardCanvasGroup.alpha = 0f;
			nightRewardCanvasGroup.blocksRaycasts = false;
			nightRewardContainerCanvasGroup.alpha = 0f;
			nightRewardContainerCanvasGroup.blocksRaycasts = false;
			panicRankStamp.color = new Color(panicRankStamp.color.r, panicRankStamp.color.g, panicRankStamp.color.b, 0f);
			panicRankStamp.sprite = smallRankStamp.GetSpriteAt(TPSingleton<PlayableUnitManager>.Instance.NightReport.PanicRank);
			page2SkipCanvasGroup.alpha = 1f;
			page2SkipCanvasGroup.blocksRaycasts = true;
			nightRatingCanvasGroup.alpha = 0f;
			nightRatingCanvasGroup.blocksRaycasts = false;
			nightRankStamp.color = new Color(nightRankStamp.color.r, nightRankStamp.color.g, nightRankStamp.color.b, 0f);
			nightRankStamp.sprite = bigRankStamp.GetSpriteAt(TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightRank);
			nightRankStamp.sprite = bigRankStamp.GetSpriteAt(TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightRank);
			nightCountText.text = Localizer.Format("NightReportPanel_NightCount", TPSingleton<GameManager>.Instance.Game.DayNumber);
			commandersSentence.text = Localizer.Get("NightReportPanel_RankSentence_" + NightReportController.Constants.IndexToRankLabels[TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightRank]);
			panicRewardIndicator.Init(PanicManager.Panic);
			StartCoroutine(ShowNightRewardPanel());
		}
		else
		{
			nightRewardCanvasGroup.alpha = 1f;
			nightRewardCanvasGroup.blocksRaycasts = true;
			nightRatingCanvasGroup.alpha = 1f;
			nightRatingCanvasGroup.blocksRaycasts = true;
			page2SkipCanvasGroup.alpha = 0f;
			page2SkipCanvasGroup.blocksRaycasts = false;
		}
		firstTimeOpenedThisNight = false;
	}

	private IEnumerator ShowBattleReportPanel()
	{
		battleReportCanvasGroup.DOFade(1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : 1f).SetFullId("BattleReportFadeIn", this);
		battleReportCanvasGroup.blocksRaycasts = true;
		unitAliveValueText.text = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count.ToString();
		hpLostValueText.text = Mathf.FloorToInt(TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightHpLost).ToString();
		deadUnitValueText.text = (TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.ContainsKey(TPSingleton<GameManager>.Instance.DayNumber) ? TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits[TPSingleton<GameManager>.Instance.DayNumber].Count.ToString() : "0");
		battleReportRankStamp.DOFade(1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : stampFadeTweenDuration).SetFullId("BattleReportRankStampFadeIn", this);
		battleReportRankStamp.rectTransform.DOPunchScale(Vector3.one * stampPunchStrength, PlayableUnitManager.DebugForceSkipNightReport ? 0f : stampPunchTweenDuration, 1, 0.1f).SetFullId("BattleReportRankStampFadeIn", this);
		PlayAudioClip(stampAudioClip);
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitAfterBottomCanvasAppear);
		page1ContinueCanvasGroup.DOFade(1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : 1f).SetFullId("BattleReportContinueFadeIn", this);
		page1ContinueCanvasGroup.blocksRaycasts = true;
		page1SkipCanvasGroup.alpha = 0f;
		page1SkipCanvasGroup.blocksRaycasts = false;
		TPSingleton<NightReportPanel>.Instance.killReportLeftButton.gameObject.SetActive(TPSingleton<NightReportPanel>.Instance.killReportParent.sizeDelta.x > TPSingleton<NightReportPanel>.Instance.killsBoardMask.sizeDelta.x);
		TPSingleton<NightReportPanel>.Instance.killReportRightButton.gameObject.SetActive(TPSingleton<NightReportPanel>.Instance.killReportParent.sizeDelta.x > TPSingleton<NightReportPanel>.Instance.killsBoardMask.sizeDelta.x);
		TPSingleton<NightReportPanel>.Instance.playableReportLeftButton.SetActive(TPSingleton<NightReportPanel>.Instance.playableReportParent.sizeDelta.x > TPSingleton<NightReportPanel>.Instance.playableBoardMask.sizeDelta.x);
		TPSingleton<NightReportPanel>.Instance.playableReportRightButton.SetActive(TPSingleton<NightReportPanel>.Instance.playableReportParent.sizeDelta.x > TPSingleton<NightReportPanel>.Instance.playableBoardMask.sizeDelta.x);
		killReportRightButton.Interactable = false;
		if (wantToSkipAnimations)
		{
			Time.timeScale = 1f;
			wantToSkipAnimations = false;
		}
		if (PlayableUnitManager.DebugForceSkipNightReport)
		{
			OnContinueClick();
		}
	}

	private IEnumerator ShowKills()
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitAfterTitleAppear);
		xpPanelCanvasGroup.DOFade(1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : 0.5f).SetFullId("XPPanelFadeIn", this);
		xpPanelCanvasGroup.blocksRaycasts = true;
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingAllKillsReports);
		int index = 0;
		int totalXP = 0;
		TPSingleton<NightReportPanel>.Instance.sharedXPText.text = $"{totalXP}";
		foreach (KillReportData item2 in TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight)
		{
			if (!item2.HideInNightReport)
			{
				if (TPSingleton<NightReportPanel>.Instance.killReports.Count <= index)
				{
					KillReportDisplay item = Object.Instantiate(TPSingleton<NightReportPanel>.Instance.killReportPrefab, TPSingleton<NightReportPanel>.Instance.killReportParent);
					TPSingleton<NightReportPanel>.Instance.killReports.Add(item);
				}
				PlayAudioClip(killReportAudioClips[UnityEngine.Random.Range(0, killReportAudioClips.Length)]);
				AddKillReport(index, item2, ref totalXP);
				index++;
				yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBetweenEachKillsReportAppear);
			}
		}
		StartCoroutine(ShowPlayableReports());
	}

	private IEnumerator ShowPlayableReports()
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingPlayableReports);
		int playableUnitCount = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count;
		int num = (TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.ContainsKey(TPSingleton<GameManager>.Instance.DayNumber) ? TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits[TPSingleton<GameManager>.Instance.DayNumber].Count : 0);
		int i;
		for (i = 0; i < playableUnitCount; i++)
		{
			while (playableReports.Count <= i)
			{
				PlayableReportDisplay item = Object.Instantiate(TPSingleton<NightReportPanel>.Instance.playableReportPrefab, TPSingleton<NightReportPanel>.Instance.playableReportParent);
				playableReports.Add(item);
			}
			playableReports[i].gameObject.SetActive(value: true);
			playableReports[i].Refresh(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i]);
		}
		int j;
		for (j = 0; j < num; j++)
		{
			while (playableReports.Count <= i + j)
			{
				PlayableReportDisplay item2 = Object.Instantiate(TPSingleton<NightReportPanel>.Instance.playableReportPrefab, TPSingleton<NightReportPanel>.Instance.playableReportParent);
				playableReports.Add(item2);
			}
			playableReports[i + j].gameObject.SetActive(value: true);
			playableReports[i + j].Refresh(TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits[TPSingleton<GameManager>.Instance.DayNumber][j], isDead: true);
		}
		for (int k = i + j; k < playableReports.Count; k++)
		{
			playableReports[k].gameObject.SetActive(value: false);
		}
		PlayAudioClip(characterAppearAudioClip);
		playableReportCanvasGroup.DOFade(1f, 1f).SetEase(Ease.OutCubic).SetFullId("PlayableReportFadeIn", this);
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforePlayableXPAnimation);
		TPSingleton<PlayableUnitManager>.Instance.DistributeDailyExperience();
		RecruitmentController.GenerateNewRoster();
		PlayAudioClip(gainXPAudioClip);
		for (int l = 0; l < playableUnitCount; l++)
		{
			playableReports[l].UnitLevelDisplay.Refresh(instant: false);
		}
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitAfterXPAnimationBeginning);
		StartCoroutine(ShowBattleReportPanel());
	}

	private IEnumerator ShowNightRewardPanel()
	{
		nightRewardCanvasGroup.DOFade(1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : 1f).SetFullId("NightRewardFadeIn", this);
		nightRewardCanvasGroup.blocksRaycasts = true;
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitAfterNightRwardCanvasAppear);
		panicRewardIndicator.Refresh(PanicManager.Panic, PlayableUnitManager.DebugForceSkipNightReport);
		yield return new WaitWhile(() => panicRewardIndicator.IsMoving);
		panicRankStamp.DOFade(1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : stampFadeTweenDuration).SetFullId("PanicRankStampFadeIn", this);
		panicRankStamp.rectTransform.DOPunchScale(Vector3.one * stampPunchStrength, PlayableUnitManager.DebugForceSkipNightReport ? 0f : stampPunchTweenDuration, 1, 0.1f).SetFullId("PanicRankStampFadeIn", this);
		PlayAudioClip(stampAudioClip);
		yield return new WaitForSeconds(stampFadeTweenDuration);
		CollectGoldReward();
		CollectMaterialsReward();
		CollectItemReward();
		StartCoroutine((ApplicationManager.Application.RunsCompleted != 0) ? ShowSoulsRewardPanel() : ShowNightRatingPanel());
	}

	private IEnumerator ShowNightRatingPanel()
	{
		yield return null;
		nightRatingCanvasGroup.DOFade(1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : 1f).SetFullId("NightRatingFadeIn", this);
		nightRatingCanvasGroup.blocksRaycasts = true;
		nightRankStamp.DOFade(1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : stampFadeTweenDuration).SetFullId("NightRankStampFadeIn", this);
		nightRankStamp.rectTransform.DOPunchScale(Vector3.one * stampPunchStrength, PlayableUnitManager.DebugForceSkipNightReport ? 0f : stampPunchTweenDuration, 1, 0.1f).SetFullId("NightRankStampFadeIn", this);
		PlayAudioClip(finalStampAudioClip);
		page2SkipCanvasGroup.alpha = 0f;
		page2SkipCanvasGroup.blocksRaycasts = false;
		if (wantToSkipAnimations)
		{
			Time.timeScale = 1f;
			wantToSkipAnimations = false;
		}
		if (PlayableUnitManager.DebugForceSkipNightReport)
		{
			OnContinueClick();
		}
	}

	private IEnumerator ShowSoulsRewardPanel()
	{
		yield return soulsRewardPanel.ShowTrophiesPanel(TPSingleton<GameManager>.Instance.Game.IsDefeat);
		yield return ShowNightRatingPanel();
	}

	private void ToggleKillsReportButtons()
	{
		killReportLeftButton.Interactable = TPSingleton<NightReportPanel>.Instance.killReportParent.anchoredPosition.x != 114f;
		killReportRightButton.Interactable = TPSingleton<NightReportPanel>.Instance.killReportParent.anchoredPosition.x != 0f - TPSingleton<NightReportPanel>.Instance.killReportParent.sizeDelta.x + 678f - 114f;
	}

	protected override void Awake()
	{
		base.Awake();
		canvas = TPSingleton<NightReportPanel>.Instance.GetComponent<Canvas>();
		canvas.enabled = false;
		canvasGroup = TPSingleton<NightReportPanel>.Instance.GetComponent<CanvasGroup>();
		canvasGroup.blocksRaycasts = false;
		isOpened = false;
		rectTransform = GetComponent<RectTransform>();
		posYInit = rectTransform.anchoredPosition.y;
		audioSources = new AudioSource[audioSourcesCount];
		audioSources[0] = audioSourceTemplate;
		for (int i = 1; i < audioSources.Length; i++)
		{
			audioSources[i] = Object.Instantiate(audioSourceTemplate, audioSourceTemplate.transform.parent);
		}
	}

	[ContextMenu("Open")]
	public void DebugOpen()
	{
		if (!UnityEngine.Application.isPlaying)
		{
			Debug.LogError("Unable to use this context menu when the application is not running");
		}
		else if (!TPSingleton<NightReportPanel>.Instance.isOpened)
		{
			Open();
		}
	}

	[ContextMenu("Close")]
	public void DebugClose()
	{
		if (!UnityEngine.Application.isPlaying)
		{
			Debug.LogError("Unable to use this context menu when the application is not running");
		}
		else if (TPSingleton<NightReportPanel>.Instance.isOpened)
		{
			Close();
		}
	}
}
