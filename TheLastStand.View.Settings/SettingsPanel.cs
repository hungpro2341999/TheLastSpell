using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Rewired;
using Sirenix.OdinInspector;
using TPLib;
using TPLib.UI;
using TheLastStand.Controller;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.Menus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class SettingsPanel : SerializedMonoBehaviour, IOverlayUser
{
	public enum E_State
	{
		Closed,
		Opened
	}

	public static class Constants
	{
		public static class LocalizationKeys
		{
			public const string ConsentWillNotBeSavedTitle = "Consent_WillNotbeSaved";

			public const string ConsentBackToMainMenuTitle = "Consent_BackToMainMenu";

			public const string ConsentAbandon = "Consent_AbandonGame";

			public const string ConsentSkipTutorial = "Consent_SkipTutorial";
		}
	}

	[SerializeField]
	[Range(0.1f, 1f)]
	private float fadeDuration = 0.2f;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private LanguagePanel languagePanel;

	[SerializeField]
	private ScreenSettingsPanel screenSettingsPanel;

	[SerializeField]
	private VolumesPanel volumesPanel;

	[SerializeField]
	private ScreenShakesOptionPanel screenShakesOptionPanel;

	[SerializeField]
	private SettingsBottomPanel settingsBottomPanel;

	[SerializeField]
	private TurnEndWarningsOptionPanel turnEndWarningsOptionPanel;

	[SerializeField]
	private RunInBackgroundPanel runInBackgroundPanel;

	[SerializeField]
	private RestrictedCursorPanel restrictedCursorPanel;

	[SerializeField]
	private EraseSavePanel eraseSavePanel;

	[SerializeField]
	private UISizePanel uiSizePanel;

	[SerializeField]
	private SmartCastPanel smartCastPanel;

	[SerializeField]
	private SpeedModePanel speedModePanel;

	[SerializeField]
	private SpeedScalePanel speedScalePanel;

	[SerializeField]
	private AlwaysDisplayMaxStatValuePanel alwaysDisplayMaxStatValuePanel;

	[SerializeField]
	private AlwaysDisplayUnitPortraitAttribute alwaysDisplayUnitPortraitAttribute;

	[SerializeField]
	private HideCompendium hideCompendium;

	[SerializeField]
	private ShowSkillsHotkeysPanel showSkillsHotkeysPanel;

	[SerializeField]
	private EdgePanPanel edgePanPanel;

	[SerializeField]
	private FocusCamOnSelectionsPanel focusCamOnSelectionsPanel;

	[SerializeField]
	private ResolutionDropdownPanel resolutionPanel;

	[FormerlySerializedAs("edgePanOverUIPanel")]
	[SerializeField]
	private EdgePanOverUIPanel edgePanOverUIPanel;

	[SerializeField]
	private OtherSettingsPanel otherSettingsPanelPanel;

	[SerializeField]
	private GameObject[] hiddenGameObjectsWithController;

	[SerializeField]
	private Button mainMenuButton;

	[SerializeField]
	private Button abandonButton;

	[SerializeField]
	private Dictionary<Toggle, TabbedPageView> tabPagePairs = new Dictionary<Toggle, TabbedPageView>();

	[SerializeField]
	private Dictionary<Toggle, TabbedPageView> tabPagePairsAdditionalTabs = new Dictionary<Toggle, TabbedPageView>();

	[SerializeField]
	private Toggle keyRemappingToggle;

	[SerializeField]
	private TabbedPageView keyRemappingTab;

	[SerializeField]
	private Toggle toggleGame;

	[SerializeField]
	private ToggleGroup toggleGroup;

	[SerializeField]
	private Transform tabPosOn;

	[SerializeField]
	private Transform tabPosOff;

	[SerializeField]
	[Min(0f)]
	private float snapshotTransitionDurationIn;

	[SerializeField]
	[Min(0f)]
	private float snapshotTransitionDurationOut;

	[SerializeField]
	private HUDJoystickDynamicTarget joystickTarget;

	[SerializeField]
	private HUDJoystickDynamicTarget dynamicTabTarget;

	private Dictionary<Toggle, TabbedPageView> allTabPagePairs = new Dictionary<Toggle, TabbedPageView>();

	private Tween fadeTween;

	private Game.E_State lastState;

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public ResolutionDropdownPanel ResolutionDropdownPanel => resolutionPanel;

	public E_State State { get; private set; }

	public HUDJoystickTarget JoystickTarget => joystickTarget;

	public void Close(bool backToGame = true)
	{
		if (State == E_State.Closed)
		{
			return;
		}
		SaveSettings();
		bool flag = TPSingleton<MainMenuView>.Exist();
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick && flag)
		{
			TPSingleton<MainMenuView>.Instance.JoystickSelectOptionToggle();
		}
		if (backToGame)
		{
			CameraView.AttenuateWorldForPopupFocus(null);
			TPSingleton<SoundManager>.Instance.TransitionToNormalSnapshot(snapshotTransitionDurationOut);
			if (TPSingleton<GameManager>.Exist())
			{
				GameController.SetState(lastState);
			}
			fadeTween?.Kill();
			fadeTween = DOTween.To(() => canvasGroup.alpha, delegate(float a)
			{
				canvasGroup.alpha = a;
			}, 0f, fadeDuration).OnComplete(delegate
			{
				canvas.enabled = false;
			});
			fadeTween.Play();
			if (TheLastStand.Manager.InputManager.IsLastControllerJoystick && !flag)
			{
				StartCoroutine(ExitHUDNavigationModeEndOfFrame());
			}
			State = E_State.Closed;
		}
	}

	private IEnumerator ExitHUDNavigationModeEndOfFrame()
	{
		yield return null;
		TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
	}

	public void OnAbandonButtonClick()
	{
		if (State == E_State.Opened)
		{
			GenericConsent.Open(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap ? "Consent_SkipTutorial" : "Consent_AbandonGame", Abandon, OnAbandonPopupClosed, smallVersion: true);
			if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
			{
				DiscardJoystickSelection();
			}
		}
	}

	public void OnResumeButtonClick()
	{
		Resume();
	}

	public void OnGameQuitButtonClick()
	{
		if (State != E_State.Opened)
		{
			return;
		}
		if (TPSingleton<GameManager>.Instance.IsSaveAllowed())
		{
			GenericConsent.Open("Consent_BackToMainMenu", delegate
			{
				Quit();
			}, OnMainMenuPopupClosed, smallVersion: true);
		}
		else
		{
			GenericConsent.Open("Consent_WillNotbeSaved", delegate
			{
				Quit();
			}, OnMainMenuPopupClosed);
		}
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			DiscardJoystickSelection();
		}
	}

	public void Open()
	{
		if (State == E_State.Opened)
		{
			return;
		}
		canvas.enabled = true;
		if (TPSingleton<GameManager>.Exist())
		{
			lastState = TPSingleton<GameManager>.Instance.Game.State;
			GameController.SetState(Game.E_State.Settings);
			TPSingleton<SoundManager>.Instance.TransitionToSettingsSnapshot(snapshotTransitionDurationIn);
		}
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		fadeTween?.Kill();
		fadeTween = DOTween.To(() => canvasGroup.alpha, delegate(float a)
		{
			canvasGroup.alpha = a;
		}, 1f, fadeDuration).OnComplete(delegate
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		});
		fadeTween.Play();
		CameraView.AttenuateWorldForPopupFocus(this);
		State = E_State.Opened;
		toggleGame.isOn = true;
		toggleGroup.NotifyToggleOn(toggleGame);
		Refresh();
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			if (!TPSingleton<MainMenuView>.Exist())
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			}
			keyRemappingToggle.interactable = false;
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
		}
		else
		{
			keyRemappingToggle.interactable = true;
		}
	}

	public void Refresh()
	{
		screenSettingsPanel.Refresh();
		volumesPanel.Refresh();
		screenShakesOptionPanel.Refresh();
		settingsBottomPanel.Refresh();
		languagePanel.Refresh();
		turnEndWarningsOptionPanel.Refresh();
		runInBackgroundPanel.Refresh();
		restrictedCursorPanel.Refresh();
		uiSizePanel.Refresh();
		smartCastPanel.Refresh();
		speedModePanel.Refresh();
		speedScalePanel.Refresh();
		alwaysDisplayMaxStatValuePanel.Refresh();
		alwaysDisplayUnitPortraitAttribute.Refresh();
		hideCompendium.Refresh();
		edgePanPanel.Refresh();
		edgePanOverUIPanel.Refresh();
		showSkillsHotkeysPanel.Refresh();
		focusCamOnSelectionsPanel.Refresh();
		otherSettingsPanelPanel.Refresh();
		for (int i = 0; i < hiddenGameObjectsWithController.Length; i++)
		{
			hiddenGameObjectsWithController[i].SetActive(!TheLastStand.Manager.InputManager.IsLastControllerJoystick);
		}
		RefreshOpenedPage();
	}

	public void RefreshOpenedPage()
	{
		foreach (KeyValuePair<Toggle, TabbedPageView> allTabPagePair in allTabPagePairs)
		{
			if (allTabPagePair.Value != null && allTabPagePair.Key.isOn)
			{
				allTabPagePair.Value.IsDirty = true;
			}
		}
	}

	public void RefreshFrameRateCap()
	{
		screenSettingsPanel.FrameRateCapPanel.Refresh();
	}

	public void OnAbandonPopupClosed()
	{
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
			EventSystem.current.SetSelectedGameObject(abandonButton.gameObject);
		}
	}

	public void OnMainMenuPopupClosed()
	{
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
			EventSystem.current.SetSelectedGameObject(mainMenuButton.gameObject);
		}
	}

	public void OnEraseSavePopupClosed()
	{
		StartCoroutine(OnEraseSavePopupClosedCoroutine());
	}

	private IEnumerator OnEraseSavePopupClosedCoroutine()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
		TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(dynamicTabTarget.GetSelectionInfo());
		yield return null;
		EventSystem.current.SetSelectedGameObject(eraseSavePanel.Selectable.gameObject);
	}

	private void Abandon()
	{
		TileObjectSelectionManager.DeselectAll();
		SettingsManager.CloseSettings(backToGame: false);
		TPSingleton<SoundManager>.Instance.TransitionToDefaultSnapshot(GameManager.AmbientSoundsFadeOutDuration);
		GameController.TriggerGameOver(Game.E_GameOverCause.Abandon);
	}

	private void DiscardJoystickSelection()
	{
		EventSystem.current.SetSelectedGameObject(null);
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
	}

	private void InitAllTabPagePairs()
	{
		allTabPagePairs = tabPagePairs.Concat(tabPagePairsAdditionalTabs).ToDictionary((KeyValuePair<Toggle, TabbedPageView> kvp) => kvp.Key, (KeyValuePair<Toggle, TabbedPageView> v) => v.Value);
	}

	private void OnDestroy()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}

	private void OnLastActiveControllerChanged(Rewired.ControllerType controllerType)
	{
		if (State == E_State.Opened)
		{
			keyRemappingToggle.interactable = controllerType != Rewired.ControllerType.Joystick;
			for (int i = 0; i < hiddenGameObjectsWithController.Length; i++)
			{
				hiddenGameObjectsWithController[i].SetActive(controllerType != Rewired.ControllerType.Joystick);
			}
		}
	}

	private void Quit(bool killSave = false)
	{
		SettingsManager.CloseSettings(backToGame: false);
		if (TPSingleton<GameManager>.Exist())
		{
			TileObjectSelectionManager.ResetStaticProperties();
			GameController.GoBackToMainMenu(killSave);
		}
	}

	private void Resume()
	{
		if (State == E_State.Opened)
		{
			SettingsManager.CloseSettings();
		}
	}

	private void SaveSettings()
	{
		SettingsManager.Save();
	}

	private void Start()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		canvas.enabled = false;
		canvasGroup.alpha = 0f;
		InitAllTabPagePairs();
		if (allTabPagePairs.Count == 0)
		{
			TPSingleton<SettingsManager>.Instance.LogError("AllTabPagePairs doesn't have an entry.", this);
		}
		else
		{
			foreach (KeyValuePair<Toggle, TabbedPageView> tabbedPage in allTabPagePairs)
			{
				tabbedPage.Key.onValueChanged.AddListener(delegate(bool value)
				{
					TabbedPageToggle_ValueChanged(tabbedPage.Key, value);
				});
			}
		}
		eraseSavePanel.Refresh();
	}

	private void TabbedPageToggle_ValueChanged(Toggle sender, bool value)
	{
		sender.transform.localPosition = new Vector3(sender.transform.localPosition.x, value ? tabPosOn.transform.localPosition.y : tabPosOff.transform.localPosition.y, sender.transform.localPosition.z);
		if (value)
		{
			allTabPagePairs[sender].Open();
		}
		else
		{
			allTabPagePairs[sender].Close();
		}
	}

	private void SelectNeighbourTab(bool next)
	{
		int count = allTabPagePairs.Count;
		for (int i = 0; i < count; i++)
		{
			KeyValuePair<Toggle, TabbedPageView> keyValuePair = allTabPagePairs.ElementAt(i);
			if (keyValuePair.Key.isOn)
			{
				keyValuePair.Key.isOn = false;
				int index = (i + (next ? 1 : (-1))).Mod(count);
				if (TheLastStand.Manager.InputManager.IsLastControllerJoystick && allTabPagePairs.ElementAt(index).Value == keyRemappingTab)
				{
					index = (i + (next ? 2 : (-2))).Mod(count);
				}
				allTabPagePairs.ElementAt(index).Key.isOn = true;
				break;
			}
		}
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(dynamicTabTarget.GetSelectionInfo());
		}
	}

	private void Update()
	{
		if (State != E_State.Opened)
		{
			return;
		}
		if (TheLastStand.Manager.InputManager.GetButtonDown(80))
		{
			if (!GenericConsent.IsWaitingForInput())
			{
				Resume();
			}
		}
		else if (TheLastStand.Manager.InputManager.GetButtonDown(109))
		{
			SelectNeighbourTab(next: true);
		}
		else if (TheLastStand.Manager.InputManager.GetButtonDown(110))
		{
			SelectNeighbourTab(next: false);
		}
	}
}
