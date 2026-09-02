using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Rewired;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.Apocalypse;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Apocalypse;
using TheLastStand.Serialization;
using TheLastStand.View.Generic;
using TheLastStand.View.SaveSlots;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Menus;

public class MainMenuView : TPSingleton<MainMenuView>
{
	public static class Constants
	{
		public const string ContinueGameButtonKey = "MainMenu_ContinueGameButton";

		public const string ContinueGameInfoValidKey = "MainMenu_ContinueGameInfo_Valid";

		public const string ContinueGameInfoValidNoApocalypseKey = "MainMenu_ContinueGameInfo_ValidNoApocalypse";

		public const string ContinueGameInfoOutdatedKey = "MainMenu_ContinueGameInfo_Outdated";

		public const string ContinueGameInfoMissingModKey = "MainMenu_ContinueGameInfo_MissingMod";

		public const string ContinueGameInfoMissingDLCKey = "MainMenu_ContinueGameInfo_MissingDLC";

		public const string ContinueGameInfoCorruptedKey = "MainMenu_ContinueGameInfo_Corrupted";

		public const string ContinueCampaignButtonKey = "MainMenu_ContinueCampaignButton";

		public const string NewCampaignButtonKey = "MainMenu_NewCampaignButton";

		public const string PopupOutdatedGameSaveTitleKey = "MainMenu_Popup_OutdatedGameSave_Title";

		public const string PopupOutdatedGameSaveTextKey = "MainMenu_Popup_OutdatedGameSave_Text";

		public const string PopupMissingDLCGameSaveTitleKey = "MainMenu_Popup_MissingDLCGameSave_Title";

		public const string PopupMissingDLCGameSaveTextKey = "MainMenu_Popup_MissingDLCGameSave_Text";

		public const string PopupMissingModGameSaveTitleKey = "MainMenu_Popup_MissingModGameSave_Title";

		public const string PopupMissingModGameSaveTextKey = "MainMenu_Popup_MissingModGameSave_Text";

		public const string PopupWillCorruptGameSaveTitleKey = "MainMenu_Popup_WillCorruptGameSave_Title";

		public const string PopupWillCorruptGameSaveTextKey = "MainMenu_Popup_WillCorruptGameSave_Text";

		public const string PopupWillCorruptMissingDLCGameSaveTitleKey = "MainMenu_Popup_WillCorruptMissingDLCGameSave_Title";

		public const string PopupWillCorruptMissingDLCGameSaveTextKey = "MainMenu_Popup_WillCorruptMissingDLCGameSave_Text";

		public const string KeyRemappingChangesDisclaimerTitle = "KeyRemappingChanges_Disclaimer_Title";

		public const string SettingsChangesDisclaimerTitle = "SettingsChanges_Disclaimer_Title";
	}

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private GraphicRaycaster graphicRaycaster;

	[SerializeField]
	private TextMeshProUGUI buildVersionText;

	[SerializeField]
	private TextMeshProUGUI continueText;

	[SerializeField]
	private TextMeshProUGUI continueInfoText;

	[SerializeField]
	private GameObject openingSequenceButton;

	[SerializeField]
	private Selectable optionToggle;

	[SerializeField]
	private Selectable saveSlotsToggle;

	[SerializeField]
	private Selectable[] mainMenuToggles;

	[SerializeField]
	private GameObject[] disabledWhenUsingController;

	public static string GetContinueGameText(string city)
	{
		return string.Format(Localizer.Get("MainMenu_ContinueGameButton"), city);
	}

	public static string GetContinueCampaignText()
	{
		return Localizer.Get("MainMenu_ContinueCampaignButton");
	}

	public static string GetNewCampaignText()
	{
		return Localizer.Get("MainMenu_NewCampaignButton");
	}

	public static string GetLoadFailedSubtitle((SaveManager.E_BrokenSaveReason? Reason, Exception Exception)? failedLoadsInfo)
	{
		return failedLoadsInfo?.Reason switch
		{
			SaveManager.E_BrokenSaveReason.WRONG_VERSION => string.Format(Localizer.Get("MainMenu_ContinueGameInfo_Outdated")), 
			SaveManager.E_BrokenSaveReason.MISSING_MOD => string.Format(Localizer.Get("MainMenu_ContinueGameInfo_MissingMod")), 
			SaveManager.E_BrokenSaveReason.MISSING_DLC => string.Format(Localizer.Get("MainMenu_ContinueGameInfo_MissingDLC")), 
			_ => string.Format(Localizer.Get("MainMenu_ContinueGameInfo_Corrupted")), 
		};
	}

	public static void OpenSettingsChangesWarning()
	{
		if (TPSingleton<SettingsManager>.Instance.SettingsChangesWarningsToDisplay.Count > 0)
		{
			GenericPopUp.Open(TPSingleton<SettingsManager>.Instance.SettingsChangesWarningsToDisplay.Select((SettingsManager.SettingsChangesNoteLocalizationKeys o) => o.TitleKey).ToList(), TPSingleton<SettingsManager>.Instance.SettingsChangesWarningsToDisplay.Select((SettingsManager.SettingsChangesNoteLocalizationKeys o) => o.TextKey).ToList(), TPSingleton<SettingsManager>.Instance.SettingsChangesWarningsToDisplay.Select((SettingsManager.SettingsChangesNoteLocalizationKeys o) => o.FormatArgs).ToList());
			TPSingleton<SettingsManager>.Instance.SettingsChangesWarningsToDisplay.Clear();
		}
	}

	public void OnContinueClick()
	{
		TryContinueGame(graphicRaycaster);
	}

	public void TryContinueGame(GraphicRaycaster raycasterToDisable)
	{
		if (EventSystem.current.currentSelectedGameObject != null)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject(null);
		}
		raycasterToDisable.enabled = false;
		if (ApplicationManager.Application.ApplicationQuitInOraculum)
		{
			ApplicationManager.Application.ApplicationController.SetState("MetaShops");
		}
		else if (SaveManager.DoesCurrentGameSaveExist())
		{
			SaverLoader.SerializedContainerLoadingInfo<SerializedGameState> currentPreloadedGameSave = TPSingleton<SaveManager>.Instance.CurrentPreloadedGameSave;
			if (currentPreloadedGameSave == null || currentPreloadedGameSave.FailedLoadsInfo[0].Reason != SaveManager.E_BrokenSaveReason.WRONG_VERSION)
			{
				SaverLoader.SerializedContainerLoadingInfo<SerializedGameState> currentPreloadedGameSave2 = TPSingleton<SaveManager>.Instance.CurrentPreloadedGameSave;
				if (currentPreloadedGameSave2 == null || !currentPreloadedGameSave2.FailedLoadsInfo[1].Reason.HasValue)
				{
					ApplicationManager.Application.ApplicationController.SetState("LoadGame");
					return;
				}
			}
			SaverLoader.SerializedContainerLoadingInfo<SerializedGameState> currentPreloadedGameSave3 = TPSingleton<SaveManager>.Instance.CurrentPreloadedGameSave;
			if (currentPreloadedGameSave3 != null && currentPreloadedGameSave3.FailedLoadsInfo[0].Reason == SaveManager.E_BrokenSaveReason.MISSING_DLC)
			{
				string[] parameters = new string[1] { string.Join(Localizer.Get("Generic_EnumerableSeparator"), (TPSingleton<SaveManager>.Instance.CurrentPreloadedGameSave.FailedLoadsInfo[0].Exception as SaverLoader.MissingDLCException).GetLocalizedMissingDLCs()) };
				GenericConsent.Open(new Localizer.ParameterizedLocalizationLine("MainMenu_Popup_WillCorruptMissingDLCGameSave_Title"), new Localizer.ParameterizedLocalizationLine("MainMenu_Popup_WillCorruptMissingDLCGameSave_Text", parameters), delegate
				{
					SaveManager.CorruptGameSave(SaveManager.CurrentProfileIndex);
					ApplicationManager.Application.ApplicationController.SetState("LoadWorldMap");
				}, delegate
				{
					raycasterToDisable.enabled = true;
					if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
					{
						EventSystem.current.SetSelectedGameObject((TPSingleton<SaveSlotsPanel>.Instance.PopupState == SaveSlotsPanel.E_State.Closed) ? mainMenuToggles[0].gameObject : TPSingleton<SaveSlotsPanel>.Instance.LastSelectedGameObject);
					}
				}, new Localizer.ParameterizedLocalizationLine("GenericConsent_Delete"), new Localizer.ParameterizedLocalizationLine("GenericConsent_Close"));
				return;
			}
			GenericBlockingPopup.OpenAsSimple("MainMenu_Popup_WillCorruptGameSave_Title", "MainMenu_Popup_WillCorruptGameSave_Text", delegate
			{
				SaveManager.CorruptGameSave(SaveManager.CurrentProfileIndex);
				ApplicationManager.Application.ApplicationController.SetState("LoadWorldMap");
			}, smallVersion: false, delegate
			{
				raycasterToDisable.enabled = true;
				if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
				{
					EventSystem.current.SetSelectedGameObject((TPSingleton<SaveSlotsPanel>.Instance.PopupState == SaveSlotsPanel.E_State.Closed) ? mainMenuToggles[0].gameObject : TPSingleton<SaveSlotsPanel>.Instance.LastSelectedGameObject);
				}
			});
		}
		else if (ApplicationManager.Application.RunsCompleted == 0)
		{
			ApplicationManager.Application.HasSeenIntroduction = true;
			AnimatedCutsceneManager.PlayPreGameAnimatedCutscene("NewGame");
		}
		else
		{
			ApplicationManager.Application.ApplicationController.SetState("LoadWorldMap");
		}
	}

	public void OnSaveSlotsClick()
	{
		ApplicationManager.Application.ApplicationController.SetState("SaveSlots");
	}

	public void OnCreditsClick()
	{
		if (EventSystem.current.currentSelectedGameObject != null)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject(null);
		}
		graphicRaycaster.enabled = false;
		ApplicationManager.Application.ApplicationController.SetState("Credits");
	}

	public void OnOpeningSequenceClick()
	{
		if (EventSystem.current.currentSelectedGameObject != null)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject(null);
		}
		graphicRaycaster.enabled = false;
		ApplicationManager.Application.HasSeenIntroduction = true;
		AnimatedCutsceneManager.PlayPreGameAnimatedCutscene("GameLobby");
	}

	public void OnOptionsClick()
	{
		if (SettingsManager.CanOpenSettings())
		{
			ApplicationManager.Application.ApplicationController.SetState("Settings");
			if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
				TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<SettingsManager>.Instance.SettingsPanel.JoystickTarget.GetSelectionInfo());
			}
		}
	}

	public void OnQuitClick()
	{
		if (EventSystem.current.currentSelectedGameObject != null)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject(null);
		}
		graphicRaycaster.enabled = false;
		canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InOutSine).OnComplete(delegate
		{
			ApplicationManager.Application.ApplicationController.SetState("ExitApp");
		});
	}

	public void Refresh()
	{
		RefreshContinueText();
	}

	public void JoystickSelectOptionToggle()
	{
		EventSystem.current.SetSelectedGameObject(optionToggle.gameObject);
	}

	public void JoystickSelectSaveSlotsToggle()
	{
		EventSystem.current.SetSelectedGameObject(saveSlotsToggle.gameObject);
	}

	private void InitJoystickNavigation()
	{
		List<Selectable> list = new List<Selectable>();
		for (int i = 0; i < mainMenuToggles.Length; i++)
		{
			if (mainMenuToggles[i].gameObject.activeInHierarchy && mainMenuToggles[i].IsInteractable())
			{
				list.Add(mainMenuToggles[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].SetMode(Navigation.Mode.Explicit);
			if (j > 0)
			{
				list[j].SetSelectOnUp(list[j - 1]);
			}
			if (j < list.Count - 1)
			{
				list[j].SetSelectOnDown(list[j + 1]);
			}
		}
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}

	private void OnLastActiveControllerChanged(Rewired.ControllerType controllerType)
	{
		if (!(ApplicationManager.Application.State.GetName() != "GameLobby") && !GenericPopUp.IsOpen && !GenericConsent.IsOpen && !GenericBlockingPopup.IsOpen)
		{
			if (controllerType == Rewired.ControllerType.Joystick && EventSystem.current.currentSelectedGameObject == null)
			{
				EventSystem.current.SetSelectedGameObject(mainMenuToggles[0].gameObject);
			}
			for (int i = 0; i < disabledWhenUsingController.Length; i++)
			{
				disabledWhenUsingController[i].SetActive(controllerType != Rewired.ControllerType.Joystick);
			}
		}
	}

	private void OnLocalize()
	{
		RefreshContinueText();
	}

	private void RefreshContinueText()
	{
		continueInfoText.gameObject.SetActive(SaveManager.DoesCurrentGameSaveExist());
		if (SaveManager.DoesCurrentGameSaveExist())
		{
			string text = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Name;
			string text2 = (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.NumberOfRuns + 1).ToString();
			string city = text + ((TPSingleton<WorldMapCityManager>.Instance.SelectedCity.NumberOfRuns > 0) ? (" #" + text2) : string.Empty);
			continueText.text = GetContinueGameText(city);
			if (TPSingleton<SaveManager>.Instance.CurrentPreloadedGameSave?.LoadedContainer != null)
			{
				SerializedGameState loadedContainer = TPSingleton<SaveManager>.Instance.CurrentPreloadedGameSave.LoadedContainer;
				TimeSpan timeSpan = TimeSpan.FromSeconds(loadedContainer.TotalTimeSpent);
				string text3 = Localizer.Get((loadedContainer.Game.Cycle == Game.E_Cycle.Night) ? "Cycle_Night" : "Cycle_Day");
				int num = loadedContainer.Apocalypse.ApocalypseIndex;
				if (loadedContainer.SaveVersion <= 23)
				{
					ApocalypseRetroCompatibilityLevelEquivalence apocalypseLevelEquivalence = ApocalypseRetroCompatibilityController.GetApocalypseLevelEquivalence(loadedContainer.Apocalypse.ApocalypseIndex);
					if (apocalypseLevelEquivalence != null)
					{
						num = apocalypseLevelEquivalence.CorrespondingLevel;
					}
				}
				continueInfoText.text = (ApocalypseManager.IsApocalypseUnlocked ? string.Format(Localizer.Get("MainMenu_ContinueGameInfo_Valid"), text3, num, loadedContainer.Game.DayNumber, timeSpan.ToString("hh\\:mm\\:ss")) : string.Format(Localizer.Get("MainMenu_ContinueGameInfo_ValidNoApocalypse"), text3, loadedContainer.Game.DayNumber, timeSpan.ToString("hh\\:mm\\:ss")));
			}
			else
			{
				TextMeshProUGUI textMeshProUGUI = continueInfoText;
				SaverLoader.SerializedContainerLoadingInfo<SerializedGameState> currentPreloadedGameSave = TPSingleton<SaveManager>.Instance.CurrentPreloadedGameSave;
				textMeshProUGUI.text = GetLoadFailedSubtitle((currentPreloadedGameSave != null) ? new(SaveManager.E_BrokenSaveReason?, Exception)?(currentPreloadedGameSave.FailedLoadsInfo[0]) : (((SaveManager.E_BrokenSaveReason?, Exception)?)null));
			}
		}
		else if (ApplicationManager.Application.RunsCompleted == 0)
		{
			continueText.text = GetNewCampaignText();
		}
		else
		{
			continueText.text = GetContinueCampaignText();
		}
	}

	private IEnumerator SelectFirstOptionEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		EventSystem.current.SetSelectedGameObject(mainMenuToggles[0].gameObject);
	}

	private IEnumerator CheckSelectFirstOptionAfterStartWarning()
	{
		yield return SharedYields.WaitForEndOfFrame;
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick && !GenericPopUp.IsOpen && !GenericConsent.IsOpen && !GenericBlockingPopup.IsOpen)
		{
			EventSystem.current.SetSelectedGameObject(mainMenuToggles[0].gameObject);
		}
	}

	private void Start()
	{
		TPSingleton<SoundManager>.Instance.FadeMusic(TPSingleton<SoundManager>.Instance.MenuMusic);
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		buildVersionText.text = ApplicationManager.VersionString;
		OpenSettingsChangesWarning();
		openingSequenceButton.SetActive(ApplicationManager.Application.HasSeenIntroduction);
		OnLocalize();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		InitJoystickNavigation();
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			StartCoroutine(SelectFirstOptionEndOfFrame());
		}
	}

	private void Update()
	{
		Action onCancel = delegate
		{
			StartCoroutine(CheckSelectFirstOptionAfterStartWarning());
		};
		if (SaveManager.LoadFailedInfos.Count > 0 && UIManager.DisplayGameSaveErrorPopUp(SaveManager.LoadFailedInfos, onCancel))
		{
			foreach (LoadFailedInfos loadFailedInfo in SaveManager.LoadFailedInfos)
			{
				if (!loadFailedInfo.BackupHasBeenLoaded)
				{
					TPSingleton<SaveManager>.Instance.LogError("===============================[ LOGBAR ]===============================", CLogLevel.NORMAL, forcePrintInUnity: true, printStackTrace: false);
					TPSingleton<SaveManager>.Instance.LogError("Hello again - from now on, you can stop ignoring NullRefs.", CLogLevel.NORMAL, forcePrintInUnity: true, printStackTrace: false);
					break;
				}
			}
			SaveManager.LoadFailedInfos.Clear();
		}
		if (!SaveManager.NewlyPreloadedGameSave)
		{
			return;
		}
		List<Localizer.ParameterizedLocalizationLine> list = new List<Localizer.ParameterizedLocalizationLine>();
		List<Localizer.ParameterizedLocalizationLine> list2 = new List<Localizer.ParameterizedLocalizationLine>();
		bool flag = false;
		HashSet<string> hashSet = new HashSet<string>();
		HashSet<string> hashSet2 = new HashSet<string>();
		for (int num = 0; num < TPSingleton<SaveManager>.Instance.PreloadedGameSaves.Length; num++)
		{
			SaverLoader.SerializedContainerLoadingInfo<SerializedGameState> serializedContainerLoadingInfo = TPSingleton<SaveManager>.Instance.PreloadedGameSaves[num];
			switch (serializedContainerLoadingInfo?.FailedLoadsInfo[0].Reason)
			{
			case SaveManager.E_BrokenSaveReason.WRONG_VERSION:
				flag = true;
				break;
			case SaveManager.E_BrokenSaveReason.MISSING_MOD:
				hashSet.UnionWith((serializedContainerLoadingInfo.FailedLoadsInfo[0].Exception as SaverLoader.MissingModException).MissingModIds);
				break;
			case SaveManager.E_BrokenSaveReason.MISSING_DLC:
				hashSet2.UnionWith((serializedContainerLoadingInfo.FailedLoadsInfo[0].Exception as SaverLoader.MissingDLCException).GetLocalizedMissingDLCs());
				break;
			}
		}
		if (flag)
		{
			list.Add(new Localizer.ParameterizedLocalizationLine("MainMenu_Popup_OutdatedGameSave_Title"));
			list2.Add(new Localizer.ParameterizedLocalizationLine("MainMenu_Popup_OutdatedGameSave_Text"));
		}
		if (hashSet.Count > 0)
		{
			string[] parameters = new string[1] { string.Join(Localizer.Get("Generic_EnumerableSeparator"), hashSet) };
			list.Add(new Localizer.ParameterizedLocalizationLine("MainMenu_Popup_MissingModGameSave_Title"));
			list2.Add(new Localizer.ParameterizedLocalizationLine("MainMenu_Popup_MissingModGameSave_Text", parameters));
		}
		if (hashSet2.Count > 0)
		{
			string[] parameters2 = new string[1] { string.Join(Localizer.Get("Generic_EnumerableSeparator"), hashSet2) };
			list.Add(new Localizer.ParameterizedLocalizationLine("MainMenu_Popup_MissingDLCGameSave_Title"));
			list2.Add(new Localizer.ParameterizedLocalizationLine("MainMenu_Popup_MissingDLCGameSave_Text", parameters2));
		}
		if (list.Count > 0 && list2.Count > 0)
		{
			GenericPopUp.Open(list, list2, null, null, onCancel);
		}
		SaveManager.NewlyPreloadedGameSave = false;
	}
}
