using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Yield;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Database.Meta;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using TheLastStand.View.Building.UI;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD;
using TheLastStand.View.Item;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public abstract class MetaUpgradeLineView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
	public enum E_State
	{
		None,
		Locked,
		Unlocked,
		Activated
	}

	public static class Constants
	{
		public const string MetaUnlocksLockedTitle = "<color=#fff>???</color>";

		public const string LockedUpgradeTitle = "???";

		public const float LockedLineAlpha = 0.35f;

		public const float UnlockedLineAlpha = 1f;

		public const float FxWidth = 1200f;

		public const float FxHeight = 400f;

		public const float LineMinHeight = 104f;

		public const float DelayBeforeFxAnimation = 0.25f;

		public const float DelayAfterFxAnimation = 0.35f;

		public const float SliderHeight = 45f;

		public const float SpacingHeight = 15f;

		public const int DescriptionMaximumIconsPerLineNb = 5;
	}

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	protected RectTransform parentRectTransform;

	[SerializeField]
	protected LayoutElement layoutElement;

	[SerializeField]
	protected Image fillerDynamic;

	[SerializeField]
	private GameObject newLabel;

	[SerializeField]
	private Image noSliderBackground;

	[SerializeField]
	protected LayoutElement noSliderLayoutElement;

	[SerializeField]
	private Image stateImage;

	[SerializeField]
	private SimpleFontLocalizedParent fontLocalizedParent;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private GameObject selector;

	[SerializeField]
	protected TextMeshProUGUI selectorLabel;

	[SerializeField]
	private Image bottomSeparator;

	[SerializeField]
	private Image topSeparator;

	[SerializeField]
	private TextMeshProUGUI upgradeNameText;

	[SerializeField]
	private Image upgradeIconBorder;

	[SerializeField]
	private Image upgradeIcon;

	[SerializeField]
	protected Slider slider;

	[SerializeField]
	private Animator sliderBackgroundAnimator;

	[SerializeField]
	protected Image sliderFill;

	[SerializeField]
	private Animator sliderFillAreaAnimator;

	[SerializeField]
	private Animator sliderHandleAnimator;

	[SerializeField]
	protected RectTransform descriptionContainer;

	[SerializeField]
	protected TextMeshProUGUI unlocksDescription;

	[SerializeField]
	protected RectTransform unlocksDescriptionRectTransform;

	[SerializeField]
	protected TextMeshProUGUI unlocksTitle;

	[SerializeField]
	protected RectTransform unlocksTitleRectTransform;

	[SerializeField]
	protected RectTransform descriptionIconsContainer;

	[SerializeField]
	private ItemIcon ItemIconPrefab;

	[SerializeField]
	private GlyphIcon GlyphIconPrefab;

	[SerializeField]
	private BuildingIcon BuildingIconPrefab;

	[SerializeField]
	private BuildingActionIcon BuildingActionIconPrefab;

	[SerializeField]
	private BuildingUpgradeIcon BuildingUpgradeIconPrefab;

	[SerializeField]
	private Ease fadeLineTweenEasing = Ease.OutQuad;

	[SerializeField]
	private float fadeLineTweenDuration = 0.2f;

	[SerializeField]
	protected float fillingTweenDuration = 2f;

	[SerializeField]
	protected Ease fillingTweenEasing = Ease.InQuad;

	[SerializeField]
	protected float lineResizeTweenDuration = 0.35f;

	[SerializeField]
	protected Ease lineResizeTweenEasing = Ease.OutSine;

	[SerializeField]
	private Sprite activatedBoxMetaIconSprite;

	[SerializeField]
	private Sprite activateNoSliderSprite;

	[SerializeField]
	private Sprite checkSprite;

	[SerializeField]
	private Sprite lockBoxMetaIconSprite;

	[SerializeField]
	private Sprite lockNoSliderSprite;

	[SerializeField]
	private Sprite lockSeparatorSprite;

	[SerializeField]
	private Sprite lockSprite;

	[SerializeField]
	private Sprite unlockBoxMetaIconSprite;

	[SerializeField]
	private Sprite unlockSeparatorSprite;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	[SerializeField]
	private LayoutNavigationInitializer iconsLayoutNavigationInitializer;

	protected float previousSoulsSliderValue;

	private E_State currentState;

	private bool isANewMetaUpgrade;

	private Tween unlockTween;

	private Tween fadeLineTween;

	private MetaUpgradeLineTooltipPackage metaUpgradeLineTooltipPackage;

	private readonly List<ItemIcon> itemIcons = new List<ItemIcon>();

	private readonly List<GlyphIcon> glyphIcons = new List<GlyphIcon>();

	private readonly List<BuildingIcon> buildingIcons = new List<BuildingIcon>();

	private readonly List<BuildingActionIcon> buildingActionIcons = new List<BuildingActionIcon>();

	private readonly List<BuildingUpgradeIcon> buildingUpgradeIcons = new List<BuildingUpgradeIcon>();

	private bool? isActive;

	private bool dirty;

	private bool textsDirty;

	private bool redirectingSelectionToIcon;

	public bool IsDisplayed { get; private set; }

	protected abstract string SliderBackgroundIdleKey { get; }

	protected abstract string SliderFillAreaIdleKey { get; }

	protected abstract string SliderHandleIdleKey { get; }

	protected abstract string FxExplodeAnimationLabel { get; }

	public bool? IsActive => isActive;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public RectTransform LineRectTransform => base.transform as RectTransform;

	public MetaUpgrade MetaUpgrade { get; private set; }

	public Slider Slider => slider;

	protected bool IsANewMetaUpgrade
	{
		get
		{
			return isANewMetaUpgrade;
		}
		set
		{
			newLabel.SetActive(value);
			isANewMetaUpgrade = value;
		}
	}

	public E_State State
	{
		get
		{
			return currentState;
		}
		private set
		{
			StateHasChanged = currentState != value;
			currentState = value;
			RefreshData();
		}
	}

	public bool StateHasChanged { get; private set; }

	public void Init(MetaUpgrade metaUpgrade, E_State initialState, MetaUpgradeLineTooltipPackage newMetaUpgradeLineTooltipPackage)
	{
		MetaUpgrade = metaUpgrade;
		base.transform.name = base.transform.name.Replace("(Clone)", " " + metaUpgrade.MetaUpgradeDefinition.Id);
		metaUpgradeLineTooltipPackage = newMetaUpgradeLineTooltipPackage;
		ChangeState(initialState);
	}

	public void ChangeState(E_State state)
	{
		State = state;
	}

	public void Display(bool display, bool forceRefresh = false)
	{
		if (textsDirty)
		{
			RefreshTexts(forceRefresh: true);
			textsDirty = false;
		}
		if (forceRefresh || IsDisplayed != display)
		{
			IsDisplayed = display;
			layoutElement.preferredHeight = (display ? (-1f) : parentRectTransform.rect.height);
			parentRectTransform.gameObject.SetActive(display);
			RefreshSliderIfNeeded();
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		selector.SetActive(value: true);
		MarkUpgradeAsSeen();
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		selector.SetActive(value: false);
	}

	public virtual void OnSliderValueChanged(float value)
	{
		fillerDynamic.fillAmount = sliderFill.fillAmount;
		previousSoulsSliderValue = value;
	}

	public void OnSelect()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			OnPointerEnter(null);
			if (MetaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop)
			{
				TPSingleton<DarkShopManager>.Instance.MetaShopView.OnSlotViewJoystickSelect(rectTransform);
			}
			else
			{
				TPSingleton<LightShopManager>.Instance.MetaShopView.OnSlotViewJoystickSelect(rectTransform);
			}
			iconsLayoutNavigationInitializer.InitNavigation();
			TPSingleton<OraculumView>.Instance.SetSelectedUpgrade(this);
			if (TryGetFirstIconSelectable(out var selectable))
			{
				StartCoroutine(SelectIconEndOfFrame(selectable));
			}
		}
	}

	public void OnDeselect()
	{
		if (InputManager.IsLastControllerJoystick && !redirectingSelectionToIcon)
		{
			TPSingleton<OraculumView>.Instance.SetSelectedUpgrade(null);
			OnPointerExit(null);
		}
	}

	public void OnIconDeselect()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			StartCoroutine(OnIconDeselectEndOfFrame());
		}
	}

	public void RefreshData()
	{
		RefreshNewMetaUpgradeNotification();
		slider.onValueChanged.RemoveAllListeners();
		RefreshSliderIfNeeded();
		RefreshSliderValues();
		RefreshSelector();
		RefreshStateImage();
		RefreshSeparators();
		RefreshMetaIcon();
		fontLocalizedParent?.RefreshChildren();
		if (currentState != E_State.Locked && canvasGroup.alpha != 1f)
		{
			canvasGroup.alpha = 1f;
		}
		else if (currentState == E_State.Locked && canvasGroup.alpha != 0.35f)
		{
			canvasGroup.alpha = 0.35f;
		}
		RefreshTexts();
		RefreshDescriptionIcons();
		if (State == E_State.Unlocked)
		{
			slider.onValueChanged.AddListener(OnSliderValueChanged);
		}
	}

	public void SetActive(bool active)
	{
		if (active != IsActive)
		{
			isActive = active;
			base.gameObject.SetActive(active);
			RefreshSliderIfNeeded();
		}
	}

	public bool UpdateDisplay(bool forceRefresh = false)
	{
		bool flag = IsTopAboveBottomScreen() && IsBottomBelowTopScreen();
		Display(flag, forceRefresh);
		return flag;
	}

	public bool IsTopAboveBottomScreen()
	{
		return ACameraView.MainCam.ScreenToViewportPoint(rectTransform.position).y > 0f;
	}

	public bool IsBottomBelowTopScreen()
	{
		return ACameraView.MainCam.ScreenToViewportPoint(rectTransform.position - Vector3.up * rectTransform.rect.height).y < 1f;
	}

	protected virtual void ActivateMetaUpgradeLineAndModel()
	{
		MetaUpgradesManager.RefreshFulfilledUpgrades();
		TPSingleton<MetaUpgradesManager>.Instance.ActivateUpgrade(MetaUpgrade);
		TPSingleton<MetaConditionManager>.Instance.RefreshProgression();
		ChangeState(E_State.Activated);
		StartCoroutine(MoveMetaOnTopOfUnlocked());
		LayoutRebuilder.ForceRebuildLayoutImmediate(MetaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop ? TPSingleton<DarkShopManager>.Instance.MetaShopView.LayoutGroupContainer : TPSingleton<LightShopManager>.Instance.MetaShopView.LayoutGroupContainer);
	}

	protected abstract Animator GetUnlockFxAnimator();

	protected abstract RectTransform GetUnlockFxTransform();

	protected abstract bool IsFromLightShop();

	protected virtual IEnumerator MoveMetaOnTopOfUnlocked()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject(null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
		UnlockUpgrade();
		yield return SharedYields.WaitForSeconds(0.25f);
		PlayUnlockFX();
		PlayFadeTween();
		yield return SharedYields.WaitForSeconds(0.35f);
		yield return null;
		canvasGroup.alpha = 1f;
	}

	protected virtual void RefreshMetaIcon()
	{
		switch (State)
		{
		case E_State.Locked:
			if (upgradeIconBorder.sprite != lockBoxMetaIconSprite)
			{
				upgradeIconBorder.sprite = lockBoxMetaIconSprite;
			}
			break;
		case E_State.Unlocked:
			if (upgradeIconBorder.sprite != unlockBoxMetaIconSprite)
			{
				upgradeIconBorder.sprite = unlockBoxMetaIconSprite;
			}
			break;
		case E_State.Activated:
			if (upgradeIconBorder.sprite != activatedBoxMetaIconSprite)
			{
				upgradeIconBorder.sprite = activatedBoxMetaIconSprite;
			}
			break;
		}
		if (currentState != E_State.Locked && !upgradeIcon.enabled)
		{
			upgradeIcon.enabled = true;
		}
		else if (currentState == E_State.Locked && upgradeIcon.enabled)
		{
			upgradeIcon.enabled = false;
		}
		if (upgradeIcon.enabled && upgradeIcon.sprite == null)
		{
			upgradeIcon.sprite = ResourcePooler.LoadOnce<Sprite>(MetaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop ? ("View/Sprites/UI/Meta/DarkShop/Icon_DarkShop_" + MetaUpgrade.MetaUpgradeDefinition.IconName) : ("View/Sprites/UI/Meta/LightShop/Icon_LightShop_" + MetaUpgrade.MetaUpgradeDefinition.IconName));
		}
		else if (currentState == E_State.Locked)
		{
			upgradeIcon.sprite = null;
		}
	}

	protected virtual void RefreshSelector()
	{
		selectorLabel.gameObject.SetActive(currentState == E_State.Unlocked);
	}

	protected abstract void RefreshShopNewMetaUpgrades();

	protected abstract void RefreshSliderValues();

	protected virtual void RefreshTexts(bool forceRefresh = false)
	{
		if (forceRefresh || StateHasChanged)
		{
			upgradeNameText.text = ((State != E_State.Locked) ? MetaUpgrade.Name : "???");
			switch (State)
			{
			case E_State.Activated:
				unlocksTitle.text = Localizer.Get("Meta_Unlocked");
				unlocksDescription.text = MetaUpgrade.Description;
				break;
			case E_State.Unlocked:
				unlocksTitle.text = Localizer.Get("Meta_Unlock");
				unlocksDescription.text = MetaUpgrade.Description;
				break;
			case E_State.Locked:
				unlocksTitle.text = "<color=#fff>???</color>";
				unlocksDescription.text = string.Empty;
				break;
			}
			StateHasChanged = false;
		}
	}

	private void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void MarkUpgradeAsSeen()
	{
		TPSingleton<MetaShopsManager>.Instance.AddMetaUpgradeToAlreadySeen(MetaUpgrade);
		RefreshNewMetaUpgradeNotification();
		RefreshShopNewMetaUpgrades();
	}

	private void PlayUnlockFX()
	{
		RectTransform unlockFxTransform = GetUnlockFxTransform();
		Transform parent = unlockFxTransform.parent;
		unlockFxTransform.SetParent(LineRectTransform);
		unlockFxTransform.anchoredPosition = Vector2.zero;
		unlockFxTransform.SetParent(parent);
		float num = parentRectTransform.sizeDelta.y - 104f;
		unlockFxTransform.sizeDelta = new Vector2(1200f, 400f + num);
		GetUnlockFxAnimator().Play(FxExplodeAnimationLabel);
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		slider.onValueChanged.RemoveAllListeners();
	}

	private IEnumerator OnIconDeselectEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		for (int i = 0; i < descriptionIconsContainer.childCount; i++)
		{
			if (EventSystem.current.currentSelectedGameObject == descriptionIconsContainer.GetChild(i).gameObject)
			{
				yield break;
			}
		}
		OnPointerExit(null);
		if (TPSingleton<OraculumView>.Instance.SelectedUpgrade == this)
		{
			TPSingleton<OraculumView>.Instance.SetSelectedUpgrade(null);
		}
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshTexts(forceRefresh: true);
		}
		else
		{
			textsDirty = true;
		}
	}

	private void PlayFadeTween()
	{
		fadeLineTween?.Kill();
		fadeLineTween = canvasGroup.DOFade(0f, fadeLineTweenDuration).SetEase(fadeLineTweenEasing);
	}

	private IEnumerator SelectIconEndOfFrame(Selectable selectable)
	{
		redirectingSelectionToIcon = true;
		yield return SharedYields.WaitForEndOfFrame;
		EventSystem.current.SetSelectedGameObject(selectable.gameObject);
		redirectingSelectionToIcon = false;
	}

	private bool TryGetFirstIconSelectable(out Selectable selectable)
	{
		selectable = null;
		if (itemIcons.Count > 0)
		{
			selectable = itemIcons[0].Selectable;
		}
		else if (glyphIcons.Count > 0)
		{
			selectable = glyphIcons[0].Selectable;
		}
		else if (buildingIcons.Count > 0)
		{
			selectable = buildingIcons[0].Selectable;
		}
		else if (buildingActionIcons.Count > 0)
		{
			selectable = buildingActionIcons[0].Selectable;
		}
		else if (buildingUpgradeIcons.Count > 0)
		{
			selectable = buildingUpgradeIcons[0].Selectable;
		}
		return selectable != null;
	}

	private void RefreshDescriptionIcons()
	{
		if (State != E_State.Locked)
		{
			bool active = MetaUpgrade.MetaUpgradeDefinition.ItemsToShow.Count > 0 || MetaUpgrade.MetaUpgradeDefinition.GlyphsToShow.Count > 0 || MetaUpgrade.MetaUpgradeDefinition.BuildingsToShow.Count > 0 || MetaUpgrade.MetaUpgradeDefinition.BuildingActionsToShow.Count > 0 || MetaUpgrade.MetaUpgradeDefinition.BuildingUpgradesToShow.Count > 0;
			descriptionIconsContainer.gameObject.SetActive(active);
			RefreshItemIcons();
			RefreshGlyphIcons();
			RefreshBuildingIcons();
			RefreshBuildingActionIcons();
			RefreshBuildingUpgradeIcons();
		}
	}

	private void RefreshItemIcons()
	{
		RefreshIconsOfType(itemIcons, ItemIconPrefab, MetaUpgrade.MetaUpgradeDefinition.ItemsToShow.Count, InitItem);
		void InitItem(int i)
		{
			itemIcons[i].Init(ItemDatabase.ItemDefinitions[MetaUpgrade.MetaUpgradeDefinition.ItemsToShow[i]], this, metaUpgradeLineTooltipPackage.ItemTooltip, IsFromLightShop());
		}
	}

	private void RefreshGlyphIcons()
	{
		RefreshIconsOfType(glyphIcons, GlyphIconPrefab, MetaUpgrade.MetaUpgradeDefinition.GlyphsToShow.Count, InitGlyph);
		void InitGlyph(int i)
		{
			glyphIcons[i].Init(GlyphDatabase.GlyphDefinitions[MetaUpgrade.MetaUpgradeDefinition.GlyphsToShow[i]], this, metaUpgradeLineTooltipPackage.GlyphTooltip, IsFromLightShop());
		}
	}

	private void RefreshBuildingIcons()
	{
		RefreshIconsOfType(buildingIcons, BuildingIconPrefab, MetaUpgrade.MetaUpgradeDefinition.BuildingsToShow.Count, InitBuilding);
		void InitBuilding(int i)
		{
			buildingIcons[i].Init(BuildingDatabase.BuildingDefinitions[MetaUpgrade.MetaUpgradeDefinition.BuildingsToShow[i]], this, metaUpgradeLineTooltipPackage.BuildingTooltip, IsFromLightShop());
		}
	}

	private void RefreshBuildingActionIcons()
	{
		RefreshIconsOfType(buildingActionIcons, BuildingActionIconPrefab, MetaUpgrade.MetaUpgradeDefinition.BuildingActionsToShow.Count, InitBuildingAction);
		void InitBuildingAction(int i)
		{
			buildingActionIcons[i].Init(BuildingDatabase.BuildingActionDefinitions[MetaUpgrade.MetaUpgradeDefinition.BuildingActionsToShow[i]], this, metaUpgradeLineTooltipPackage.BuildingActionTooltip, IsFromLightShop());
		}
	}

	private void RefreshBuildingUpgradeIcons()
	{
		RefreshIconsOfType(buildingUpgradeIcons, BuildingUpgradeIconPrefab, MetaUpgrade.MetaUpgradeDefinition.BuildingUpgradesToShow.Count, InitBuildingUpgrade);
		void InitBuildingUpgrade(int i)
		{
			buildingUpgradeIcons[i].Init(BuildingDatabase.BuildingUpgradeDefinitions[MetaUpgrade.MetaUpgradeDefinition.BuildingUpgradesToShow[i]], this, metaUpgradeLineTooltipPackage.BuildingUpgradeTooltip, IsFromLightShop());
		}
	}

	private void RefreshIconsOfType<T>(List<T> icons, T iconPrefab, int count, Action<int> initAction) where T : OraculumUnlockIcon
	{
		for (int num = icons.Count - 1; num >= count; num--)
		{
			UnityEngine.Object.Destroy(buildingActionIcons[num].gameObject);
			icons.RemoveAt(num);
		}
		for (int i = icons.Count; i < count; i++)
		{
			icons.Add(UnityEngine.Object.Instantiate(iconPrefab, descriptionIconsContainer));
		}
		for (int j = 0; j < count; j++)
		{
			icons[j].PointerEventsListener.OnPointerDownEvent.AddListener(delegate
			{
				OnPointerDown(null);
			});
			icons[j].PointerEventsListener.OnPointerUpEvent.AddListener(delegate
			{
				OnPointerUp(null);
			});
			initAction(j);
		}
	}

	private void RefreshNewMetaUpgradeNotification()
	{
		IsANewMetaUpgrade = TPSingleton<MetaShopsManager>.Instance.IsANewUpgrade(MetaUpgrade) && State != E_State.Locked && State != E_State.Activated;
	}

	private void RefreshSeparators()
	{
		if (State == E_State.Locked && (topSeparator.sprite != lockSeparatorSprite || bottomSeparator.sprite != lockSeparatorSprite))
		{
			topSeparator.sprite = lockSeparatorSprite;
			bottomSeparator.sprite = lockSeparatorSprite;
		}
		else if (State != E_State.Locked && (topSeparator.sprite != unlockSeparatorSprite || bottomSeparator.sprite != unlockSeparatorSprite))
		{
			topSeparator.sprite = unlockSeparatorSprite;
			bottomSeparator.sprite = unlockSeparatorSprite;
		}
	}

	private void RefreshSlider()
	{
		noSliderBackground.gameObject.SetActive(State != E_State.Unlocked);
		noSliderLayoutElement.gameObject.SetActive(State != E_State.Unlocked);
		slider.gameObject.SetActive(State == E_State.Unlocked);
		Image image = noSliderBackground;
		image.sprite = State switch
		{
			E_State.Activated => activateNoSliderSprite, 
			E_State.Locked => lockNoSliderSprite, 
			_ => null, 
		};
		if (State == E_State.Unlocked)
		{
			float value = UnityEngine.Random.value;
			sliderBackgroundAnimator.Play(SliderBackgroundIdleKey, 0, value);
			sliderFillAreaAnimator.Play(SliderFillAreaIdleKey, 0, value);
			sliderHandleAnimator.Play(SliderHandleIdleKey, 0, value);
		}
	}

	private void RefreshSliderIfNeeded()
	{
		if (IsDisplayed && isActive == true && dirty)
		{
			RefreshSlider();
			dirty = false;
		}
		else
		{
			dirty = true;
		}
	}

	private void RefreshStateImage()
	{
		if (State == E_State.Unlocked && stateImage.gameObject.activeSelf)
		{
			stateImage.gameObject.SetActive(value: false);
			return;
		}
		if (currentState == E_State.Unlocked)
		{
			return;
		}
		if (!stateImage.gameObject.activeSelf)
		{
			stateImage.gameObject.SetActive(value: true);
		}
		Image image = stateImage;
		E_State e_State = currentState;
		Sprite sprite;
		if (e_State != E_State.Locked)
		{
			if (e_State != E_State.Activated || !(stateImage.sprite != checkSprite))
			{
				goto IL_00b4;
			}
			sprite = checkSprite;
		}
		else
		{
			if (!(stateImage.sprite != lockSprite))
			{
				goto IL_00b4;
			}
			sprite = lockSprite;
		}
		goto IL_00c0;
		IL_00c0:
		image.sprite = sprite;
		return;
		IL_00b4:
		sprite = stateImage.sprite;
		goto IL_00c0;
	}

	private void UnlockUpgrade()
	{
		unlockTween = noSliderLayoutElement.DOPreferredSize(new Vector2(noSliderLayoutElement.preferredWidth, 0f), lineResizeTweenDuration).SetEase(lineResizeTweenEasing);
	}

	public abstract void OnPointerUp(PointerEventData eventData);

	public abstract void OnPointerDown(PointerEventData eventData);
}
