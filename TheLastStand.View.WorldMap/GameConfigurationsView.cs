using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Steamworks;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.DLC;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.DLC;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Animation;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.Apocalypse;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.MetaShops;
using TheLastStand.View.WorldMap.Apocalypse;
using TheLastStand.View.WorldMap.Glyphs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap;

public class GameConfigurationsView : TPSingleton<GameConfigurationsView>
{
	public delegate void DelApocalypseSelectionChanged(bool selected);

	public DelApocalypseSelectionChanged OnApocalypseSelectionHasChanged;

	[SerializeField]
	private GlyphSelectionPreview glyphSelectionPreview;

	[SerializeField]
	private ApocalypseSelectionPreview apocalypseSelectionPreview;

	[SerializeField]
	private BetterButton backButton;

	[SerializeField]
	private RectTransform backButtonRect;

	[SerializeField]
	private GamepadInputDisplay backButtonGamepadInputDisplay;

	[SerializeField]
	private FloatTweenAnimation backButtonAnimation;

	[SerializeField]
	private TextMeshProUGUI cityDescription;

	[SerializeField]
	private Image cityImage;

	[SerializeField]
	private TextMeshProUGUI cityName;

	[SerializeField]
	private RectTransform panelRectTransform;

	[SerializeField]
	private FloatTweenAnimation configurationPanelAnimation;

	[SerializeField]
	private BetterButton closeButton;

	[SerializeField]
	private RectTransform pannelCloseButton;

	[SerializeField]
	private FloatTweenAnimation closeButtonAnimation;

	[SerializeField]
	private BetterButton nextCityButton;

	[SerializeField]
	private BetterButton previousCityButton;

	[SerializeField]
	private TextMeshProUGUI startingSetupText;

	[SerializeField]
	private RectTransform scrollViewport;

	[SerializeField]
	private Scrollbar scrollBar;

	[SerializeField]
	[Range(0f, 1f)]
	private float scrollButtonsSensitivity = 0.1f;

	[SerializeField]
	private BetterToggle storyCityToggle;

	[SerializeField]
	private BetterToggle dlcCityToggle;

	[SerializeField]
	private RectTransform tabPosOn;

	[SerializeField]
	private RectTransform tabPosOff;

	[SerializeField]
	private float yOffset = 10f;

	[SerializeField]
	private RectTransform panelTransform;

	[SerializeField]
	private RectTransform boxTransform;

	[SerializeField]
	private RectTransform topTransform;

	[SerializeField]
	private RectTransform contentTransform;

	[SerializeField]
	private RectTransform bottomDecorationTransform;

	[SerializeField]
	private RectTransform boxMinSizeTransform;

	[SerializeField]
	private RectTransform boxWithoutDecorationsTransform;

	private bool isFolded = true;

	private bool previousCityUnlocked;

	private WorldMapCity currentCity;

	public List<ApocalypseView> ApocalypseLines { get; } = new List<ApocalypseView>();

	public ApocalypseSelectionPreview ApocalypseSelectionPreview => apocalypseSelectionPreview;

	public RectTransform BoxWithoutDecorationsTransform => boxWithoutDecorationsTransform;

	public GlyphSelectionPreview GlyphSelectionPreview => glyphSelectionPreview;

	public static void OpenSelectedCityLinkedDLCStorePage()
	{
		WorldMapCity selectedCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		if (selectedCity != null && selectedCity.CityDefinition.HasLinkedDLC)
		{
			DLCDefinition dLCFromId = TPSingleton<DLCManager>.Instance.GetDLCFromId(selectedCity.CityDefinition.LinkedDLCId);
			if (dLCFromId != null)
			{
				SteamFriends.ActivateGameOverlayToWebPage(dLCFromId.GetStoreURL());
			}
		}
	}

	public static void StartNewGameIfEnoughGlyph()
	{
		TPSingleton<GameConfigurationsView>.Instance.Fold();
		WorldMapCity selectedCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		if (selectedCity.CurrentGlyphPoints < selectedCity.CityDefinition.MaxGlyphPoints)
		{
			GenericConsent.Open(new Localizer.ParameterizedLocalizationLine("WorldMap_Consent_NotEnoughGlyphs"), WorldMapCityManager.StartNewGame, TPSingleton<GameConfigurationsView>.Instance.Unfold);
		}
		else
		{
			WorldMapCityManager.StartNewGame();
		}
	}

	public void AdjustScrollView(RectTransform item)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(item, scrollViewport, scrollBar, 0.01f, 0.01f);
	}

	public void AdjustScrollViewToBottom()
	{
		scrollBar.value = 0f;
	}

	public void JoystickSelectPanel(bool forceSelect = false)
	{
		if (GlyphSelectionPreview.EditButton.gameObject.activeSelf && (!isFolded || forceSelect))
		{
			EventSystem.current.SetSelectedGameObject(GlyphSelectionPreview.EditButton.gameObject);
			return;
		}
		EventSystem.current.SetSelectedGameObject(null);
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
	}

	public void OnBackButtonClicked()
	{
		ApplicationManager.Application.ApplicationController.SetState("GameLobby");
	}

	public void OnBotButtonClick()
	{
		scrollBar.value = Mathf.Clamp01(scrollBar.value - (scrollBar.size + scrollButtonsSensitivity));
	}

	public void OnCloseButtonClicked()
	{
		if (!ACameraView.IsZooming)
		{
			WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.EXPLORATION);
		}
	}

	public void OnStateChange()
	{
		switch (TPSingleton<WorldMapStateManager>.Instance.CurrentState)
		{
		case WorldMapStateManager.WorldMapState.EXPLORATION:
			Fold();
			PauseAnimations();
			break;
		case WorldMapStateManager.WorldMapState.FOCUSED:
			ContinueAnimations();
			Refresh();
			Unfold();
			break;
		}
	}

	public void OnTopButtonClick()
	{
		scrollBar.value = Mathf.Clamp01(scrollBar.value + scrollBar.size + scrollButtonsSensitivity);
	}

	public void OpenApocalypseSelectionPanel()
	{
		OraculumHub<ApocalypseSelectionPanel>.Display(show: true);
	}

	public void OpenGlyphSelectionPanel()
	{
		OraculumHub<GlyphSelectionPanel>.Display(show: true);
	}

	public void Refresh()
	{
		apocalypseSelectionPreview.gameObject.SetActive(ApocalypseManager.IsApocalypseUnlocked);
		if (ApocalypseManager.IsApocalypseUnlocked)
		{
			apocalypseSelectionPreview.Refresh();
		}
		GlyphSelectionPreview.Refresh();
		currentCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		if (currentCity != null)
		{
			cityName.text = currentCity.CityDefinition.Name;
			cityDescription.text = currentCity.CityDefinition.Description;
			startingSetupText.text = Localizer.Get("WorldMap_StartingSetup_" + currentCity.CityDefinition.StartingSetup);
			cityImage.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/WorldMap/WorldMap_CityPortrait_" + currentCity.CityDefinition.Id);
			RefreshCityToggles();
			apocalypseSelectionPreview.gameObject.SetActive(ApocalypseManager.IsApocalypseUnlocked && currentCity.IsUnlocked);
			glyphSelectionPreview.RefreshLockedUI(currentCity);
			if (previousCityUnlocked != currentCity.IsUnlocked)
			{
				RefreshBoxSize();
			}
			previousCityUnlocked = currentCity.IsUnlocked;
		}
		RefreshJoystickNavigation();
	}

	public void RefreshBoxSize()
	{
		StartCoroutine(RefreshBoxSizeAfterAFrame());
	}

	public IEnumerator RefreshBoxSizeAfterAFrame()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
		yield return null;
		float b = topTransform.sizeDelta.y + contentTransform.sizeDelta.y + bottomDecorationTransform.sizeDelta.y + yOffset;
		boxTransform.sizeDelta = new Vector2(boxTransform.sizeDelta.x, Mathf.Max(boxMinSizeTransform.sizeDelta.y, Mathf.Min(panelTransform.rect.height, b)));
	}

	private void Fold()
	{
		if (!isFolded)
		{
			configurationPanelAnimation.StatusTransitionTween?.Kill();
			configurationPanelAnimation.StatusTransitionTween = panelRectTransform.DOAnchorPosX(configurationPanelAnimation.StatusOne, configurationPanelAnimation.TransitionDuration).SetEase(configurationPanelAnimation.TransitionEase).SetFullId("FoldTween", this);
			configurationPanelAnimation.InStatusOne = true;
			closeButtonAnimation.StatusTransitionTween?.Kill();
			closeButtonAnimation.StatusTransitionTween = pannelCloseButton.DOAnchorPosX(closeButtonAnimation.StatusOne, closeButtonAnimation.TransitionDuration).SetEase(closeButtonAnimation.TransitionEase).SetFullId("CloseButtonFoldTween", this);
			closeButtonAnimation.InStatusOne = true;
			backButtonGamepadInputDisplay.gameObject.SetActive(value: true);
			backButtonAnimation.StatusTransitionTween?.Kill();
			backButtonAnimation.StatusTransitionTween = backButtonRect.DOAnchorPosX(backButtonAnimation.StatusOne, backButtonAnimation.TransitionDuration).SetEase(backButtonAnimation.TransitionEase).SetFullId("BackButtonFoldTween", this);
			backButtonAnimation.InStatusOne = true;
			if (InputManager.IsLastControllerJoystick)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			isFolded = true;
		}
	}

	private void Unfold()
	{
		if (!isFolded)
		{
			return;
		}
		configurationPanelAnimation.StatusTransitionTween?.Kill();
		configurationPanelAnimation.StatusTransitionTween = panelRectTransform.DOAnchorPosX(configurationPanelAnimation.StatusTwo, configurationPanelAnimation.TransitionDuration).SetEase(configurationPanelAnimation.TransitionEase).SetFullId("UnfoldTween", this);
		RefreshBoxSize();
		configurationPanelAnimation.InStatusOne = false;
		closeButtonAnimation.StatusTransitionTween?.Kill();
		closeButtonAnimation.StatusTransitionTween = pannelCloseButton.DOAnchorPosX(closeButtonAnimation.StatusTwo, closeButtonAnimation.TransitionDuration).SetEase(closeButtonAnimation.TransitionEase).SetFullId("CloseButtonUnfoldTween", this);
		closeButtonAnimation.InStatusOne = false;
		backButtonGamepadInputDisplay.gameObject.SetActive(value: false);
		backButtonAnimation.StatusTransitionTween?.Kill();
		backButtonAnimation.StatusTransitionTween = backButtonRect.DOAnchorPosX(backButtonAnimation.StatusTwo, backButtonAnimation.TransitionDuration).SetEase(backButtonAnimation.TransitionEase).SetFullId("BackButtonUnfoldTween", this);
		backButtonAnimation.InStatusOne = false;
		if (InputManager.IsLastControllerJoystick)
		{
			Object.FindObjectOfType<JoystickHighlight>().ToggleAlwaysFollow(state: true);
			configurationPanelAnimation.StatusTransitionTween.OnComplete(delegate
			{
				Object.FindObjectOfType<JoystickHighlight>().ToggleAlwaysFollow(state: false);
			});
			JoystickSelectPanel(forceSelect: true);
		}
		isFolded = false;
	}

	private void ContinueAnimations()
	{
		apocalypseSelectionPreview.ContinueAnimations();
	}

	private void PauseAnimations()
	{
		apocalypseSelectionPreview.PauseAnimations();
	}

	private void RefreshJoystickNavigation()
	{
		Selectable selectable = GlyphSelectionPreview.FirstPreviewedGlyphDisplay?.GetComponent<Selectable>();
		bool flag = false;
		if (apocalypseSelectionPreview.gameObject.activeSelf)
		{
			flag = true;
			apocalypseSelectionPreview.EditButtonSelectable.SetSelectOnUp((selectable != null) ? selectable : GlyphSelectionPreview.EditButton);
		}
		GlyphSelectionPreview.EditButton.SetMode(Navigation.Mode.Explicit);
		GlyphSelectionPreview.EditButton.SetSelectOnDown((selectable != null) ? selectable : (flag ? apocalypseSelectionPreview.EditButtonSelectable : null));
		if (InputManager.IsLastControllerJoystick)
		{
			JoystickSelectPanel();
		}
	}

	private void RefreshCityToggles()
	{
		if (currentCity.CityDefinition.IsStoryMap)
		{
			storyCityToggle.gameObject.SetActive(value: true);
			dlcCityToggle.gameObject.SetActive(currentCity.CityDefinition.HasLinkedCity);
			if (!storyCityToggle.isOn)
			{
				storyCityToggle.SetIsOnWithoutNotify(value: true);
			}
		}
		else
		{
			storyCityToggle.gameObject.SetActive(currentCity.CityDefinition.HasLinkedCity);
			dlcCityToggle.gameObject.SetActive(value: true);
			if (!dlcCityToggle.isOn)
			{
				dlcCityToggle.SetIsOnWithoutNotify(value: true);
			}
		}
		RefreshCityTogglePosition(storyCityToggle);
		RefreshCityTogglePosition(dlcCityToggle);
	}

	private void RefreshCityTogglePosition(Toggle cityToggle)
	{
		if (!(cityToggle == null))
		{
			cityToggle.transform.localPosition = new Vector3(cityToggle.transform.localPosition.x, cityToggle.isOn ? tabPosOn.transform.localPosition.y : tabPosOff.transform.localPosition.y, cityToggle.transform.localPosition.z);
		}
	}

	private void CityTypeToggle_ValueChanged(bool value, bool isStoryMap, Toggle sender)
	{
		RefreshCityTogglePosition(sender);
		if (value)
		{
			if (isStoryMap)
			{
				TPSingleton<WorldMapCityManager>.Instance.SelectStoryMapCity();
			}
			else
			{
				TPSingleton<WorldMapCityManager>.Instance.SelectDLCMapCity();
			}
		}
	}

	private void Start()
	{
		previousCityButton.onClick.AddListener(TPSingleton<WorldMapCityManager>.Instance.SelectPreviousCity);
		nextCityButton.onClick.AddListener(TPSingleton<WorldMapCityManager>.Instance.SelectNextCity);
		storyCityToggle.onValueChanged.AddListener(delegate(bool value)
		{
			CityTypeToggle_ValueChanged(value, isStoryMap: true, storyCityToggle);
		});
		dlcCityToggle.onValueChanged.AddListener(delegate(bool value)
		{
			CityTypeToggle_ValueChanged(value, isStoryMap: false, dlcCityToggle);
		});
		closeButton.onClick.AddListener(OnCloseButtonClicked);
		backButton.onClick.AddListener(OnBackButtonClicked);
		PauseAnimations();
		RefreshJoystickNavigation();
		scrollBar.value = 1f;
	}
}
