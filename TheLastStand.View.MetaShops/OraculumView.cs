using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using Rewired;
using TPLib;
using TPLib.Yield;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.SDK;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.Meta;
using TheLastStand.View.Building.UI;
using TheLastStand.View.Cursor;
using TheLastStand.View.Generic;
using TheLastStand.View.Item;
using TheLastStand.View.ToDoList;
using TheLastStand.View.WorldMap.Glyphs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public class OraculumView : OraculumHub<OraculumView>
{
	private static class Constants
	{
		public static readonly Vector2 DarkShopPivot = new Vector2(0f, 0.5f);

		public static readonly Vector2 LightShopPivot = new Vector2(1f, 0.5f);

		public static readonly Vector2 HubPivot = new Vector2(0.5f, 0.5f);
	}

	[SerializeField]
	private MetaShopNotifications darkShopNotifications;

	[SerializeField]
	private MetaShopNotifications lightShopNotifications;

	[SerializeField]
	private Transform sealsScaler;

	[SerializeField]
	private UIParticle[] sealsParticles;

	[SerializeField]
	private GenericTooltipDisplayer leaveHubButtonBlocker;

	[SerializeField]
	private ItemTooltip itemTooltip;

	[SerializeField]
	private GlyphTooltip glyphTooltip;

	[SerializeField]
	private BuildingConstructionTooltip buildingTooltip;

	[SerializeField]
	private BuildingActionTooltip buildingActionTooltip;

	[SerializeField]
	private BuildingUpgradeTooltip buildingUpgradeTooltip;

	[SerializeField]
	[Tooltip("Goddess and shop panel are reset when leaving the meta shops (for game scene only).")]
	private bool resetGoddessAppearance = true;

	[SerializeField]
	[Tooltip("Goddess fade in animation appearance delay after camera tween is complete.")]
	private float goddessAppearanceDelay = 1f;

	[SerializeField]
	[Tooltip("Shops appearance delay after camera tween is complete.")]
	private float shopAppearanceDelay = 1f;

	[SerializeField]
	[Tooltip("Time for the shops to fade in.")]
	private float shopAppearanceDuration = 0.3f;

	[SerializeField]
	[Tooltip("Shops offset when fading in.")]
	private float shopAppearanceOffset = 200f;

	[SerializeField]
	[Tooltip("Duration of the hub->shop transition.")]
	private float toShopDuration = 1f;

	[SerializeField]
	[Tooltip("Curve applied on hub->shop transition.")]
	private AnimationCurve toShopEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	[Tooltip("Duration of the shop->hub transition.")]
	private float toHubDuration = 1f;

	[SerializeField]
	[Tooltip("Curve applied on shop->hub transition.")]
	private AnimationCurve toHubEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private OraculumCursorView cursorView;

	private bool goddessAppearanceAnimationRunning;

	private Sequence shopFadeSequence;

	private bool isFirstFrameInShop = true;

	private MetaUpgradeLineTooltipPackage metaUpgradeLineTooltipPackage;

	private Coroutine goddessAppearanceCoroutine;

	private bool inGoddessAppearance;

	public MetaShopView CurrentShop { get; private set; }

	public bool TransitionRunning { get; private set; }

	public bool DialogueRunning { get; private set; }

	public MetaUpgradeDefinition.E_MetaUpgradeCategory CurrentCategory { get; set; } = MetaUpgradeDefinition.E_MetaUpgradeCategory.All;

	public bool IsInAnyShop => CurrentShop != null;

	public bool IsInDarkShop => CurrentShop == TPSingleton<DarkShopManager>.Instance.MetaShopView;

	public bool IsInLightShop => CurrentShop == TPSingleton<LightShopManager>.Instance.MetaShopView;

	public OraculumCursorView CursorView => cursorView;

	public MetaUpgradeLineView SelectedUpgrade { get; private set; }

	public static bool CanTransitionFromShopToHub()
	{
		if (TPSingleton<OraculumView>.Instance.Displayed && TPSingleton<OraculumView>.Instance.IsInAnyShop && !TPSingleton<OraculumView>.Instance.TransitionRunning && !TPSingleton<OraculumView>.Instance.DialogueRunning)
		{
			return !TPSingleton<OraculumView>.Instance.goddessAppearanceAnimationRunning;
		}
		return false;
	}

	public static void TransitionFromShopToHub()
	{
		TPSingleton<OraculumView>.Instance.StartCoroutine(TPSingleton<OraculumView>.Instance.TransitionToHub());
	}

	public void DisplayToShop(bool darkShop)
	{
		OraculumHub<OraculumView>.Display(show: true, delegate
		{
			StartCoroutine(TransitionToShop(darkShop));
		});
	}

	public void SetSelectedUpgrade(MetaUpgradeLineView upgrade)
	{
		if (SelectedUpgrade != null && SelectedUpgrade.MetaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop)
		{
			((DarkMetaUpgradeLineView)SelectedUpgrade).OnPointerUp(null);
		}
		SelectedUpgrade = upgrade;
	}

	public IEnumerator TransitionToHub()
	{
		if (!TransitionRunning)
		{
			shopFadeSequence?.Complete();
			RefreshLeaveButton();
			SoundManager.PlayAudioClip(MetaShopsManager.AudioSourceTransition, MetaShopsManager.BackToMainScreenTransitionAudioClip);
			if (CurrentShop == TPSingleton<DarkShopManager>.Instance.MetaShopView)
			{
				TPSingleton<LightShopManager>.Instance.RefreshShop();
			}
			else
			{
				TPSingleton<DarkShopManager>.Instance.RefreshShop();
			}
			if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
				EventSystem.current.SetSelectedGameObject(null);
				SetSelectedUpgrade(null);
			}
			TPSingleton<LightningSDKManager>.Instance.HandleMetaShopTransition(LightningSDKManager.SDKEvent.HUB_SHOP);
			yield return Transition(Constants.HubPivot, toHubEase, toHubDuration);
			TPSingleton<DarkShopManager>.Instance.Canvas.enabled = false;
			TPSingleton<LightShopManager>.Instance.Canvas.enabled = false;
			cursorView.Enable(isOn: true);
			CurrentShop = null;
		}
	}

	public IEnumerator TransitionToShop(bool isDarkShop)
	{
		if (TransitionRunning || !base.Displayed)
		{
			yield break;
		}
		cursorView.Enable(isOn: false);
		RefreshForScreenResolution();
		TransitionRunning = true;
		if (!TPSingleton<MetaShopsManager>.Instance.CurrentFilter.HasFlag(MetaUpgradeDefinition.E_MetaUpgradeFilter.NotAcquiredYet) && MetaUpgradesManager.AnyAvailableMandatoryUpgrade)
		{
			TPSingleton<MetaShopsManager>.Instance.CurrentFilter |= MetaUpgradeDefinition.E_MetaUpgradeFilter.NotAcquiredYet;
			if (TPSingleton<MetaShopsManager>.Instance.CurrentFilter.HasFlag(MetaUpgradeDefinition.E_MetaUpgradeFilter.New))
			{
				TPSingleton<MetaShopsManager>.Instance.CurrentFilter &= ~MetaUpgradeDefinition.E_MetaUpgradeFilter.New;
			}
		}
		if (isDarkShop)
		{
			TPSingleton<DarkShopManager>.Instance.Canvas.enabled = true;
			TPSingleton<DarkShopManager>.Instance.SelectCategory(CurrentCategory);
			TPSingleton<DarkShopManager>.Instance.RefreshFilterView(TPSingleton<MetaShopsManager>.Instance.CurrentFilter);
			TPSingleton<DarkShopManager>.Instance.RefreshShopJoystickNavigation();
		}
		else
		{
			TPSingleton<LightShopManager>.Instance.Canvas.enabled = true;
			TPSingleton<LightShopManager>.Instance.SelectCategory(CurrentCategory);
			TPSingleton<LightShopManager>.Instance.RefreshFilterView(TPSingleton<MetaShopsManager>.Instance.CurrentFilter);
			TPSingleton<LightShopManager>.Instance.RefreshShopJoystickNavigation();
		}
		CurrentShop = (isDarkShop ? TPSingleton<DarkShopManager>.Instance.MetaShopView : TPSingleton<LightShopManager>.Instance.MetaShopView);
		CurrentShop.GoddessView.Refresh();
		CurrentShop.EnableCanvas(base.HideGoddesses || CurrentShop.GoddessView.IdleContainer.activeSelf || !CurrentShop.NarrationView.HasNarrationToPlay);
		bool worldMapState = ApplicationManager.Application.State.GetName() == "WorldMap";
		AudioClip audioClip;
		if (base.HideGoddesses)
		{
			audioClip = (isDarkShop ? MetaShopsManager.DarkShopTransitionNoGoddessAudioClip : MetaShopsManager.LightShopTransitionNoGoddessAudioClip);
		}
		else if (CurrentShop.NarrationView.HasNarrationToPlay && !worldMapState)
		{
			audioClip = (isDarkShop ? MetaShopsManager.DarkShopTransitionAudioClip : MetaShopsManager.LightShopTransitionAudioClip);
		}
		else
		{
			ActivateGoddessAfterAFrame(CurrentShop);
			ChangeGoddessEvolutionAfterAFrame();
			CurrentShop.NarrationView.DisplayShopGreeting(CurrentShop.NarrationView.MetaNarration.MetaNarrationController.GetRandomShopGreetingId());
			audioClip = MetaShopsManager.BackToMainScreenTransitionAudioClip;
		}
		SoundManager.PlayAudioClip(MetaShopsManager.AudioSourceTransition, audioClip);
		TPSingleton<LightningSDKManager>.Instance.HandleMetaShopTransition(isDarkShop ? LightningSDKManager.SDKEvent.DARK_SHOP : LightningSDKManager.SDKEvent.LIGHT_SHOP, toShopDuration + 0.2f);
		yield return Transition(isDarkShop ? Constants.DarkShopPivot : Constants.LightShopPivot, toShopEase, toShopDuration);
		if (TPSingleton<OraculumView>.Instance.HideGoddesses)
		{
			FadeInShop(CurrentShop);
		}
		else if (CurrentShop.NarrationView.HasNarrationToPlay && !worldMapState)
		{
			goddessAppearanceCoroutine = StartCoroutine(DelayGoddessAppearance(CurrentShop));
		}
		ToggleNotifications(show: false, isDarkShop);
		CurrentShop.ResetScrollbar(forceRefresh: true);
	}

	protected override void Awake()
	{
		base.Awake();
		metaUpgradeLineTooltipPackage = new MetaUpgradeLineTooltipPackage(itemTooltip, glyphTooltip, buildingTooltip, buildingActionTooltip, buildingUpgradeTooltip);
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
	}

	protected override void EnableRaycasters(bool state)
	{
		base.EnableRaycasters(state);
		TPSingleton<DarkShopManager>.Instance.MetaShopView.CanvasGroup.interactable = state;
		TPSingleton<LightShopManager>.Instance.MetaShopView.CanvasGroup.interactable = state;
		TPSingleton<DarkShopManager>.Instance.MetaShopView.CanvasGroup.blocksRaycasts = state;
		TPSingleton<LightShopManager>.Instance.MetaShopView.CanvasGroup.blocksRaycasts = state;
	}

	protected override IEnumerator InitCoroutine()
	{
		yield return RefreshForScreenResolutionDelayed();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (TPSingleton<DarkShopManager>.Exist())
		{
			TPSingleton<DarkShopManager>.Instance.MetaShopView.RemoveBackButtonListener(OnBackToHubButtonClicked);
			TPSingleton<LightShopManager>.Instance.MetaShopView.RemoveBackButtonListener(OnBackToHubButtonClicked);
		}
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}

	protected override void OnFadeToBlackComplete()
	{
		base.OnFadeToBlackComplete();
		SetActiveShops(base.Displayed);
		if (base.Displayed)
		{
			CurrentCategory = MetaUpgradeDefinition.E_MetaUpgradeCategory.All;
			StartCoroutine(RefreshForScreenResolutionDelayed());
			AdjustSealsScale();
			RefreshLeaveButton();
			TPSingleton<DarkShopManager>.Instance.RefreshShop();
			TPSingleton<DarkShopManager>.Instance.SelectFirstTab();
			TPSingleton<LightShopManager>.Instance.RefreshShop();
			TPSingleton<LightShopManager>.Instance.SelectFirstTab();
			if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
			{
				TPSingleton<OraculumView>.Instance.cursorView.Enable(isOn: true);
				TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			}
		}
		else if (ApplicationManager.Application.State.GetName() == "Game")
		{
			TPSingleton<ToDoListView>.Instance.RefreshMetaShopsNotification();
		}
	}

	protected override void OnFadeToBlackStarts()
	{
		base.OnFadeToBlackStarts();
		if (TPSingleton<OraculumView>.Instance.Displayed)
		{
			RefreshNotifications();
		}
	}

	protected override void OnHubExit()
	{
		base.OnHubExit();
		if (resetGoddessAppearance)
		{
			TPSingleton<DarkShopManager>.Instance.MetaShopView.ResetView();
			TPSingleton<LightShopManager>.Instance.MetaShopView.ResetView();
		}
	}

	protected override void RefreshShopsScene()
	{
		base.RefreshShopsScene();
		TPSingleton<DarkShopManager>.Instance.RefreshShop();
		TPSingleton<LightShopManager>.Instance.RefreshShop();
		RefreshNotifications();
		AdjustSealsScale();
	}

	protected override void Start()
	{
		leaveButton.onClick.AddListener(TPSingleton<MetaShopsManager>.Instance.LeaveShops);
		TPSingleton<DarkShopManager>.Instance.MetaShopView.NarrationView.MetaNarration = MetaNarrationsManager.DarkNarration;
		TPSingleton<LightShopManager>.Instance.MetaShopView.NarrationView.MetaNarration = MetaNarrationsManager.LightNarration;
		TPSingleton<LightShopManager>.Instance.MetaShopView.AddBackButtonListener(OnBackToHubButtonClicked);
		TPSingleton<DarkShopManager>.Instance.MetaShopView.AddBackButtonListener(OnBackToHubButtonClicked);
		if (ApplicationManager.Application.State.GetName() != "Credits")
		{
			TPSingleton<DarkShopManager>.Instance.InitTooltips(metaUpgradeLineTooltipPackage);
			TPSingleton<LightShopManager>.Instance.InitTooltips(metaUpgradeLineTooltipPackage);
			if (ApplicationManager.Application.State.GetName() == "MetaShops")
			{
				RefreshLeaveButton();
			}
			TPSingleton<DarkShopManager>.Instance.Init();
			TPSingleton<LightShopManager>.Instance.Init();
		}
		if (ApplicationManager.Application.State.GetName() == "MetaShops" && TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			cursorView.Enable(isOn: true);
		}
		base.Start();
		if (ApplicationManager.Application.State.GetName() != "MetaShops" && ApplicationManager.Application.State.GetName() != "Credits")
		{
			SetActiveShops(toggle: false);
		}
	}

	private void AdjustSealsScale()
	{
		float num = ((Screen.height >= 2160) ? 2f : ((Screen.height <= 768) ? (2f / 3f) : 1f));
		sealsScaler.localScale = Vector3.one * num;
		for (int num2 = sealsParticles.Length - 1; num2 >= 0; num2--)
		{
			sealsParticles[num2].scale = num;
		}
	}

	private IEnumerator DelayGoddessAppearance(MetaShopView shop)
	{
		bool flag = ApplicationManager.Application.State.GetName() == "WorldMap";
		List<MetaReplica> replicas;
		bool playNarration = shop.NarrationView.MetaNarration.MetaNarrationController.TryGetValidMandatoryReplica(1, out replicas) && !flag;
		if (shop.GoddessView.IdleContainer.activeSelf && !playNarration)
		{
			ActivateGoddessAfterAFrame(shop);
			yield break;
		}
		if (!playNarration && !flag)
		{
			playNarration = !MetaNarrationsManager.NarrationDoneThisDay && shop.NarrationView.MetaNarration.MetaNarrationController.TryGetValidReplicas(3, out replicas);
		}
		if (playNarration)
		{
			shop.ResetView();
			shop.GoddessView.SetPositionX(0f);
			goddessAppearanceAnimationRunning = true;
			yield return SharedYields.WaitForSeconds(goddessAppearanceDelay);
			shop.GoddessView.FadeInContainer.SetActive(value: true);
			yield return shop.GoddessView.PlayVisualAnimationAtIndexCoroutine((MetaNarrationsManager.VisualEvolutionForced > -1) ? MetaNarrationsManager.VisualEvolutionForced : shop.NarrationView.MetaNarration.MetaNarrationController.GetHighestAvailableVisualEvolutionIndex());
			yield return SharedYields.WaitForSeconds(shopAppearanceDelay);
			goddessAppearanceAnimationRunning = false;
			if (playNarration)
			{
				DialogueRunning = true;
				MetaNarrationsManager.NarrationDoneThisDay = true;
				shop.NarrationView.RefreshLocalizedFonts();
				yield return StartCoroutine(shop.NarrationView.GreetingSequenceCoroutine(shop.NarrationView.MetaNarration.MetaNarrationController.GetNextDialogueGreetingId()));
				shop.GoddessView.OffsetAfterGreeting();
				yield return StartCoroutine(shop.NarrationView.NarrationSequenceCoroutine(replicas));
				shop.NarrationView.RefreshDisplayedName();
				while (shop.NarrationView.MetaNarration.MetaNarrationController.TryGetValidMandatoryReplica(1, out replicas))
				{
					int highestAvailableVisualEvolutionIndex = shop.NarrationView.MetaNarration.MetaNarrationController.GetHighestAvailableVisualEvolutionIndex();
					if (shop.GoddessView.CurrentEvolutionIndex < highestAvailableVisualEvolutionIndex)
					{
						shop.NarrationView.Hide();
						shop.GoddessView.FadeInContainer.SetActive(value: false);
						shop.GoddessView.FadeInContainer.SetActive(value: true);
						yield return shop.GoddessView.PlayVisualAnimationAtIndexCoroutine(highestAvailableVisualEvolutionIndex);
					}
					yield return StartCoroutine(shop.NarrationView.NarrationSequenceCoroutine(replicas));
					shop.NarrationView.RefreshDisplayedName();
				}
				DialogueRunning = false;
			}
			shop.ExitText.SetActive(value: false);
			shop.NarrationView.DisplayShopGreeting(shop.NarrationView.MetaNarration.MetaNarrationController.GetRandomShopGreetingId());
			InitShopJoystick();
			FadeInShop(shop);
		}
		else
		{
			ActivateGoddessAfterAFrame(shop);
		}
	}

	private void ActivateGoddessAfterAFrame(MetaShopView shop)
	{
		StartCoroutine(ActivateGoddessAfterAFrameCoroutine(shop));
	}

	private IEnumerator ActivateGoddessAfterAFrameCoroutine(MetaShopView shop)
	{
		yield return null;
		shop.GoddessView.OffsetAfterGreeting(instantly: true);
		shop.GoddessView.IdleContainer.SetActive(value: true);
		shop.Canvas.enabled = true;
		shop.CanvasGroup.interactable = true;
		shop.CanvasGroup.alpha = 1f;
		shop.ShowBackButton(show: true);
		InitShopJoystick();
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			if (shop == TPSingleton<DarkShopManager>.Instance.MetaShopView)
			{
				TPSingleton<DarkShopManager>.Instance.SelectFirstActiveChild();
			}
			else
			{
				TPSingleton<LightShopManager>.Instance.SelectFirstActiveChild();
			}
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}
	}

	private void ChangeGoddessEvolutionAfterAFrame()
	{
		StartCoroutine(ChangeGoddessEvolutionAfterAFrameCoroutine());
	}

	private IEnumerator ChangeGoddessEvolutionAfterAFrameCoroutine()
	{
		yield return null;
		CurrentShop.GoddessView.ChangeEvolution((MetaNarrationsManager.VisualEvolutionForced > -1) ? MetaNarrationsManager.VisualEvolutionForced : CurrentShop.NarrationView.MetaNarration.MetaNarrationController.GetHighestAvailableVisualEvolutionIndex());
	}

	private void FadeInShop(MetaShopView shopView)
	{
		shopFadeSequence = DOTween.Sequence();
		bool isDarkShop = shopView == TPSingleton<DarkShopManager>.Instance.MetaShopView;
		shopView.Canvas.enabled = true;
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		}
		shopFadeSequence.Append(shopView.CanvasGroup.DOFade(1f, shopAppearanceDuration).SetEase(Ease.OutSine).OnComplete(delegate
		{
			shopView.CanvasGroup.interactable = true;
			shopView.ShowBackButton(show: true);
			if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
				if (isDarkShop)
				{
					TPSingleton<DarkShopManager>.Instance.SelectFirstActiveChild();
				}
				else
				{
					TPSingleton<LightShopManager>.Instance.SelectFirstActiveChild();
				}
			}
		}));
		Vector3 position = shopView.CanvasGroup.transform.position;
		float x = position.x;
		position.x += (isDarkShop ? (0f - shopAppearanceOffset) : shopAppearanceOffset);
		shopView.CanvasGroup.transform.position = position;
		shopFadeSequence.Join(shopView.CanvasGroup.transform.DOMoveX(x, shopAppearanceDuration).SetEase(Ease.OutSine));
	}

	private void InitShopJoystick()
	{
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			SetSelectedUpgrade(null);
			isFirstFrameInShop = true;
		}
	}

	private void OnBackToHubButtonClicked()
	{
		StartCoroutine(TransitionToHub());
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		if (!(ApplicationManager.Application.State.GetName() == "Credits"))
		{
			switch (controllerType)
			{
			case ControllerType.Joystick:
				cursorView.Enable(!IsInAnyShop);
				break;
			default:
				cursorView.Enable(isOn: false);
				break;
			}
		}
	}

	private void RefreshForScreenResolution()
	{
		for (int i = 0; i < dynamicWidthRectTransforms.Length; i++)
		{
			dynamicWidthRectTransforms[i].sizeDelta = new Vector2(Screen.width, Screen.height);
		}
		for (int j = 0; j < dynamicHeightRectTransforms.Length; j++)
		{
			RectTransform obj = dynamicHeightRectTransforms[j];
			obj.sizeDelta = new Vector2(obj.sizeDelta.x, Screen.height);
		}
		float num = 0f;
		for (int k = 0; k < globalRectContent.Length; k++)
		{
			num += globalRectContent[k].sizeDelta.x;
		}
		globalRect.sizeDelta = new Vector2(num, Screen.height);
		TPSingleton<OraculumView>.Instance.joystickCanvasScaler.UpdateScale(TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale);
	}

	private IEnumerator RefreshForScreenResolutionDelayed()
	{
		int i = 0;
		while (i < 2)
		{
			yield return SharedYields.WaitForEndOfFrame;
			int num = i + 1;
			i = num;
		}
		RefreshForScreenResolution();
		LayoutRebuilder.ForceRebuildLayoutImmediate(globalRect);
	}

	private void RefreshLeaveButton()
	{
		bool anyValidMandatoryNarration = MetaNarrationsManager.AnyValidMandatoryNarration;
		bool anyAvailableMandatoryUpgrade = MetaUpgradesManager.AnyAvailableMandatoryUpgrade;
		bool flag = anyValidMandatoryNarration || anyAvailableMandatoryUpgrade;
		leaveButton.Interactable = !flag;
		if (flag)
		{
			leaveHubButtonBlocker.LocaKey = (anyValidMandatoryNarration ? "MetaShops_CantLeaveTooltip_Narration" : "MetaShops_CantLeaveTooltip_MandatoryUpgrades");
		}
		leaveHubButtonBlocker.gameObject.SetActive(flag);
	}

	private void RefreshNotifications()
	{
		ToggleNotifications(TPSingleton<DarkShopManager>.Instance.IsAnyUpgradeAffordable(), isDarkShop: true);
		ToggleNotifications(TPSingleton<LightShopManager>.Instance.IsAnyUpgradeAffordable(), isDarkShop: false);
	}

	private void SetActiveShops(bool toggle)
	{
		TPSingleton<DarkShopManager>.Instance.SetActive(toggle);
		TPSingleton<LightShopManager>.Instance.SetActive(toggle);
	}

	private void ToggleNotifications(bool show, bool isDarkShop)
	{
		(isDarkShop ? darkShopNotifications : lightShopNotifications).Toggle(show);
	}

	private IEnumerator Transition(Vector2 pivot, AnimationCurve animationCurve, float duration)
	{
		TransitionRunning = true;
		globalRect.SetAnchors(pivot);
		globalRect.SetPivot(pivot);
		Tween t = globalRect.DOAnchorPosX(0f, duration).SetEase(animationCurve).OnUpdate(CurrentShop.SnapShopPosition)
			.OnComplete(delegate
			{
				TransitionRunning = false;
			});
		yield return t.WaitForCompletion();
	}

	private void Update()
	{
		if (SelectedUpgrade != null)
		{
			if (!isFirstFrameInShop)
			{
				if (TheLastStand.Manager.InputManager.GetButtonDown(79))
				{
					if (SelectedUpgrade.MetaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop)
					{
						((DarkMetaUpgradeLineView)SelectedUpgrade).OnPointerDown(null);
					}
					else
					{
						((LightMetaUpgradeLineView)SelectedUpgrade).OnPointerUp(null);
					}
				}
				else if (TheLastStand.Manager.InputManager.GetButtonUp(79) && SelectedUpgrade.MetaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop)
				{
					((DarkMetaUpgradeLineView)SelectedUpgrade).OnPointerUp(null);
				}
			}
			else
			{
				isFirstFrameInShop = false;
			}
		}
		if (IsInAnyShop && CurrentShop.ExitText.activeSelf && TheLastStand.Manager.InputManager.GetButtonDown(23))
		{
			CurrentShop.ExitText.SetActive(value: false);
			CurrentShop.NarrationView.SkipNextNarration();
			if (!CurrentShop.NarrationView.HasNarrationToPlay)
			{
				StopCoroutine(goddessAppearanceCoroutine);
				CurrentShop.GoddessView.OffsetAfterGreeting(instantly: true);
				CurrentShop.CanvasGroup.alpha = 1f;
				CurrentShop.GoddessView.IdleContainer.SetActive(value: true);
				CurrentShop.GoddessView.FadeInContainer.SetActive(value: false);
				DialogueRunning = false;
				CurrentShop.GoddessView.ChangeEvolution((MetaNarrationsManager.VisualEvolutionForced > -1) ? MetaNarrationsManager.VisualEvolutionForced : CurrentShop.NarrationView.MetaNarration.MetaNarrationController.GetHighestAvailableVisualEvolutionIndex());
				CurrentShop.NarrationView.DisplayShopGreeting(CurrentShop.NarrationView.MetaNarration.MetaNarrationController.GetRandomShopGreetingId());
				InitShopJoystick();
				FadeInShop(CurrentShop);
			}
		}
	}
}
