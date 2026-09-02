using System;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using Rewired.Data;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Framework.Automaton;
using TheLastStand.Model;
using TheLastStand.Serialization;
using TheLastStand.View;
using TheLastStand.View.Cursor;
using TheLastStand.View.Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.Manager;

public class InputManager : Manager<InputManager>
{
	public delegate void LastActiveControllerChangedHandler(Rewired.ControllerType controllerType);

	[SerializeField]
	private WorldInputsView worldInputsView;

	[SerializeField]
	private UserDataStore keyRemappingSaverLoader;

	[SerializeField]
	private JoystickConfig joystickConfig;

	private int allowingCursorUICount;

	private int currentKeyboardLayoutId = -1;

	private bool hasBeenInitialized;

	private Player player;

	public static bool IsLastControllerJoystick => GetLastControllerType() == Rewired.ControllerType.Joystick;

	public static bool IsPointerOverAllowingCursorUI
	{
		get
		{
			return TPSingleton<InputManager>.Instance.allowingCursorUICount > 0;
		}
		set
		{
			TPSingleton<InputManager>.Instance.allowingCursorUICount += (value ? 1 : (-1));
		}
	}

	public static bool IsPointerOverWorld => TPSingleton<InputManager>.Instance.worldInputsView.IsPointerOverWorld;

	public static Vector3 MousePosition => Input.mousePosition;

	public static JoystickConfig JoystickConfig => TPSingleton<InputManager>.Instance.joystickConfig;

	public static Vector3 JoystickCursorPosition => TPSingleton<CursorView>.Instance.JoystickCursorPosition;

	public bool JoysticksEnabled { get; private set; } = true;

	public bool KeyboardEnabled { get; private set; } = true;

	public Player Player
	{
		get
		{
			if (player == null)
			{
				player = ReInput.players.GetPlayer("Player");
			}
			return player;
		}
	}

	public Guid JoystickGuid
	{
		get
		{
			if (ReInput.controllers.Joysticks.Count <= 0)
			{
				return Guid.Empty;
			}
			return ReInput.controllers.Joysticks[0].hardwareTypeGuid;
		}
	}

	public TheLastStand.Model.ControllerType? DebugDisplayedControllerType { get; private set; }

	public static event LastActiveControllerChangedHandler LastActiveControllerChanged;

	public static float GetAxis(int axisId)
	{
		if (TPSingleton<InputManager>.Instance.Player.isPlaying)
		{
			return TPSingleton<InputManager>.Instance.Player.GetAxis(axisId);
		}
		return 0f;
	}

	public static bool GetButton(int buttonId)
	{
		if (TPSingleton<InputManager>.Instance.Player.isPlaying)
		{
			return TPSingleton<InputManager>.Instance.Player.GetButton(buttonId);
		}
		return false;
	}

	public static bool GetButtonDown(int buttonId)
	{
		if (TPSingleton<InputManager>.Instance.Player.isPlaying)
		{
			return TPSingleton<InputManager>.Instance.Player.GetButtonDown(buttonId);
		}
		return false;
	}

	public static bool GetButtonUp(int buttonId)
	{
		if (TPSingleton<InputManager>.Instance.Player.isPlaying)
		{
			return TPSingleton<InputManager>.Instance.Player.GetButtonUp(buttonId);
		}
		return false;
	}

	public static string[] GetLocalizedHotkeysForAction(string actionKey)
	{
		InputAction inputAction = null;
		List<InputCategory> list = ReInput.mapping.ActionCategories.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			inputAction = ReInput.mapping.ActionsInCategory(list[i].id, sort: true).ToList().Find((InputAction o) => o.name == actionKey);
			if (inputAction != null)
			{
				break;
			}
		}
		if (inputAction == null)
		{
			TPSingleton<InputManager>.Instance.LogWarning("No Rewired Action has been found for Id (string) " + actionKey + ".");
			return null;
		}
		return GetLocalizedHotkeysForAction(inputAction);
	}

	public static string[] GetLocalizedHotkeysForAction(int actionId)
	{
		InputAction inputAction = null;
		List<InputCategory> list = ReInput.mapping.ActionCategories.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			inputAction = ReInput.mapping.ActionsInCategory(list[i].id, sort: true).ToList().Find((InputAction o) => o.id == actionId);
			if (inputAction != null)
			{
				break;
			}
		}
		if (inputAction == null)
		{
			TPSingleton<InputManager>.Instance.LogWarning($"No Rewired Action has been found for Id (int) {actionId}.");
			return null;
		}
		return GetLocalizedHotkeysForAction(inputAction);
	}

	public static string[] GetLocalizedHotkeysForAction(InputAction action)
	{
		List<ActionElementMap> list = new List<ActionElementMap>();
		foreach (ControllerMap allMap in TPSingleton<InputManager>.Instance.Player.controllers.maps.GetAllMaps())
		{
			if ((allMap.controllerType != Rewired.ControllerType.Joystick || IsLastControllerJoystick) && ((allMap.controllerType != Rewired.ControllerType.Keyboard && allMap.controllerType != Rewired.ControllerType.Mouse) || !IsLastControllerJoystick))
			{
				List<ActionElementMap> list2 = new List<ActionElementMap>();
				allMap.GetButtonMapsWithAction(action.id, list2);
				list.AddRange(list2);
			}
		}
		if (list.Count > 0)
		{
			string[] array = new string[list.Count];
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				array[i] = (Localizer.TryGet($"KeyCode_{list[i].keyCode}", out var value) ? value : list[i].elementIdentifierName);
			}
			return array;
		}
		return null;
	}

	public static Rewired.ControllerType GetLastControllerType()
	{
		return TPSingleton<InputManager>.Instance.Player.controllers.GetLastActiveController()?.type ?? Rewired.ControllerType.Keyboard;
	}

	public static bool GetSubmitButtonDown()
	{
		if (!Input.GetMouseButtonDown(0))
		{
			return GetButtonDown(79);
		}
		return true;
	}

	public static void LoadMap()
	{
		if (TPSingleton<InputManager>.Instance.keyRemappingSaverLoader == null)
		{
			TPSingleton<InputManager>.Instance.LogWarning("keyRemappingSaverLoader reference is missing, trying to find it with FindObjectOfType.", CLogLevel.MAJOR);
			TPSingleton<InputManager>.Instance.keyRemappingSaverLoader = UnityEngine.Object.FindObjectOfType<KeyRemappingSaverLoader>();
			if (TPSingleton<InputManager>.Instance.keyRemappingSaverLoader == null)
			{
				TPSingleton<InputManager>.Instance.LogWarning("keyRemappingSaverLoader reference is missing, cannot load input mapping.", CLogLevel.MAJOR);
				return;
			}
		}
		TPSingleton<InputManager>.Instance.keyRemappingSaverLoader.Load();
	}

	public static void OnDropdownClose()
	{
		if (TPSingleton<GameManager>.Exist())
		{
			OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
		}
		else if (TPSingleton<ApplicationManager>.Exist())
		{
			TPSingleton<InputManager>.Instance.OnApplicationStateChange(ApplicationManager.Application.State);
		}
	}

	public static void OnDropdownOpen()
	{
		TPSingleton<InputManager>.Instance.Player.controllers.maps.SetAllMapsEnabled(state: false);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(state: true, 15);
	}

	public static void OnGameStateChange(Game.E_State state)
	{
		TPSingleton<InputManager>.Instance.Player.controllers.maps.SetAllMapsEnabled(state: false);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(state: true, 0);
		switch (state)
		{
		case Game.E_State.Management:
		case Game.E_State.UnitPreparingSkill:
		case Game.E_State.UnitExecutingSkill:
		case Game.E_State.BuildingPreparingAction:
		case Game.E_State.BuildingExecutingAction:
		case Game.E_State.BuildingPreparingSkill:
		case Game.E_State.BuildingExecutingSkill:
		case Game.E_State.Construction:
		case Game.E_State.PlaceUnit:
		case Game.E_State.BuildingUpgrade:
		case Game.E_State.Wait:
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(state: true, 3);
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(state: true, 4);
			break;
		case Game.E_State.CharacterSheet:
		case Game.E_State.Recruitment:
		case Game.E_State.Shopping:
		case Game.E_State.NightReport:
		case Game.E_State.ProductionReport:
		case Game.E_State.Settings:
		case Game.E_State.GameOver:
		case Game.E_State.MetaShops:
		case Game.E_State.UnitCustomisation:
		case Game.E_State.ConsentPopup:
		case Game.E_State.BlockingPopup:
		case Game.E_State.ApocalypseMoreInfo:
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(state: true, 11);
			break;
		case Game.E_State.HowToPlay:
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(state: true, 17);
			break;
		case Game.E_State.LevelEdition:
			SetLevelEditorMapsEnabled(areEnabled: true);
			break;
		case Game.E_State.CutscenePlaying:
		case Game.E_State.SaveSlots:
			break;
		}
	}

	public static void OnGenericConsentViewToggled(bool state)
	{
		if (TPSingleton<GameManager>.Exist())
		{
			OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
		}
		else if (state)
		{
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetAllMapsEnabled(state: false);
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(state: true, 0);
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(state: true, 11);
		}
		else if (ApplicationManager.Application.State.GetName() == "WorldMap")
		{
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(state: true, 3);
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(state: true, 4);
		}
	}

	public static void OnItemTooltipDisplayedChange(bool isVisible)
	{
	}

	public static void OnTutorialPopupOpen()
	{
		TPSingleton<InputManager>.Instance.Player.controllers.maps.SetAllMapsEnabled(state: false);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(state: true, 17);
	}

	public static void OnTutorialPopupClosed()
	{
		if (TPSingleton<GameManager>.Exist())
		{
			OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
		}
		else if (TPSingleton<ApplicationManager>.Exist())
		{
			TPSingleton<InputManager>.Instance.OnApplicationStateChange(ApplicationManager.Application.State);
		}
	}

	public static void OnInputDeviceTypeChanged(SettingsManager.E_InputDeviceType inputDeviceType)
	{
		switch (inputDeviceType)
		{
		case SettingsManager.E_InputDeviceType.MouseKeyboard:
			TPSingleton<InputManager>.Instance.JoysticksEnabled = false;
			TPSingleton<InputManager>.Instance.KeyboardEnabled = true;
			UnityEngine.Cursor.visible = true;
			if (TPSingleton<HUDJoystickNavigationManager>.Exist() && !TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode())
			{
				EventSystem.current.SetSelectedGameObject(null);
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			}
			break;
		case SettingsManager.E_InputDeviceType.Controller:
		{
			int count = ReInput.controllers.Joysticks.Count;
			if (count == 0)
			{
				TPSingleton<InputManager>.Instance.LogWarning($"Setting InputDeviceType to {inputDeviceType} though no controller is registered! Registering setting but keeping Mouse/KB enabled.");
			}
			TPSingleton<InputManager>.Instance.JoysticksEnabled = true;
			TPSingleton<InputManager>.Instance.KeyboardEnabled = count == 0;
			UnityEngine.Cursor.visible = count == 0;
			break;
		}
		case SettingsManager.E_InputDeviceType.Auto:
			TPSingleton<InputManager>.Instance.JoysticksEnabled = true;
			TPSingleton<InputManager>.Instance.KeyboardEnabled = true;
			break;
		default:
			TPSingleton<InputManager>.Instance.LogError($"Unhandled InputDeviceType {inputDeviceType}!");
			return;
		}
		UpdateJoystickControllersEnabled();
		UpdateKeyboardControllerEnabled();
	}

	public static void SaveMap()
	{
		if (TPSingleton<InputManager>.Instance.keyRemappingSaverLoader == null)
		{
			TPSingleton<InputManager>.Instance.LogWarning("keyRemappingSaverLoader reference is missing, trying to find it with FindObjectOfType.", CLogLevel.MAJOR);
			TPSingleton<InputManager>.Instance.keyRemappingSaverLoader = UnityEngine.Object.FindObjectOfType<KeyRemappingSaverLoader>();
			if (TPSingleton<InputManager>.Instance.keyRemappingSaverLoader == null)
			{
				TPSingleton<InputManager>.Instance.LogWarning("keyRemappingSaverLoader reference is missing, cannot save input mapping.", CLogLevel.MAJOR);
				return;
			}
		}
		TPSingleton<InputManager>.Instance.keyRemappingSaverLoader.Save();
	}

	public static int GetKeyboardLayoutId()
	{
		switch (TPSingleton<SettingsManager>.Instance.Settings.KeyboardLayout)
		{
		case SettingsManager.E_KeyboardLayout.AZERTY:
			return 2;
		case SettingsManager.E_KeyboardLayout.QWERTY:
			return 3;
		case SettingsManager.E_KeyboardLayout.QWERTZ:
			return 4;
		default:
			TPSingleton<InputManager>.Instance.LogError($"Keyboard layout {TPSingleton<SettingsManager>.Instance.Settings.KeyboardLayout} not handled, please add it to the switch");
			return 3;
		}
	}

	public static void RefreshRewiredKeyboardLayout()
	{
		int keyboardLayoutId = GetKeyboardLayoutId();
		TPSingleton<InputManager>.Instance.Log($"Refreshing keyboard layout using Layout Id {keyboardLayoutId} (= {(SettingsManager.E_KeyboardLayout)keyboardLayoutId})");
		if (keyboardLayoutId == TPSingleton<InputManager>.Instance.currentKeyboardLayoutId)
		{
			TPSingleton<InputManager>.Instance.Log($"Layout Id {keyboardLayoutId} (= {(SettingsManager.E_KeyboardLayout)keyboardLayoutId}) used for refresh is the current one -> aborting.");
			return;
		}
		TPSingleton<InputManager>.Instance.Player.controllers.maps.RemoveMap(Rewired.ControllerType.Keyboard, 0, 0, TPSingleton<InputManager>.Instance.currentKeyboardLayoutId);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.RemoveMap(Rewired.ControllerType.Keyboard, 0, 3, TPSingleton<InputManager>.Instance.currentKeyboardLayoutId);
		TPSingleton<InputManager>.Instance.currentKeyboardLayoutId = keyboardLayoutId;
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadMap(Rewired.ControllerType.Keyboard, 0, 0, TPSingleton<InputManager>.Instance.currentKeyboardLayoutId);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadMap(Rewired.ControllerType.Keyboard, 0, 3, TPSingleton<InputManager>.Instance.currentKeyboardLayoutId);
		if (TPSingleton<GameManager>.Exist() && TPSingleton<GameManager>.Instance.Game != null)
		{
			OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
		}
	}

	public static void SetLevelEditorMapsEnabled(bool areEnabled)
	{
		if (!(TPSingleton<InputManager>.Instance == null) && TPSingleton<InputManager>.Instance.isActiveAndEnabled)
		{
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(areEnabled, 3);
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(areEnabled, 4);
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(areEnabled, 7);
		}
	}

	public void Init()
	{
		if (!hasBeenInitialized)
		{
			if (TPSingleton<ApplicationManager>.Exist())
			{
				ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent += OnApplicationStateChange;
			}
			Player.controllers.AddLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
			Player.isPlaying = true;
			if (TPSingleton<DebugManager>.Exist())
			{
				DebugManager.DevConsoleDisplayed += OnDevConsoleDisplayedChange;
			}
			if (TPSingleton<GameManager>.Exist())
			{
				worldInputsView.PointerOverWorldChangeEvent += OnPointerOverWorldChanged;
			}
			if (!UnityEngine.Application.isEditor && !UnityEngine.Application.isConsolePlatform && !JoystickConfig.BuildEnabled)
			{
				DebugToggleJoystickControllers();
			}
			Player.controllers.maps.LoadDefaultMaps(Rewired.ControllerType.Joystick);
			ReInput.ControllerConnectedEvent += OnControllerConnected;
			ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
			hasBeenInitialized = true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (TPSingleton<ApplicationManager>.Exist())
		{
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent -= OnApplicationStateChange;
		}
		if (TPSingleton<DebugManager>.Exist())
		{
			DebugManager.DevConsoleDisplayed -= OnDevConsoleDisplayedChange;
		}
		ReInput.ControllerConnectedEvent -= OnControllerConnected;
		ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
	}

	private void OnApplicationStateChange(State state)
	{
		LogWarning($"OnApplicationStateChange: {state}");
		Player.controllers.maps.SetAllMapsEnabled(state: false);
		Player.controllers.maps.SetMapsEnabled(state: true, 0);
		switch (state.GetName())
		{
		case "WorldMap":
			Player.controllers.maps.SetMapsEnabled(state: true, 3);
			Player.controllers.maps.SetMapsEnabled(state: true, 4);
			break;
		case "Game":
			if (TPSingleton<GameManager>.Exist())
			{
				OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
				if (TPSingleton<TutorialView>.Instance.DisplayCoroutineRunning)
				{
					OnTutorialPopupOpen();
				}
			}
			break;
		case "Settings":
			Player.controllers.maps.SetMapsEnabled(state: true, 11);
			break;
		}
	}

	private void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		SettingsManager.E_InputDeviceType inputDeviceType = TPSingleton<SettingsManager>.Instance.Settings.InputDeviceType;
		if (inputDeviceType == SettingsManager.E_InputDeviceType.Controller)
		{
			OnInputDeviceTypeChanged(inputDeviceType);
		}
	}

	private void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		OnInputDeviceTypeChanged(TPSingleton<SettingsManager>.Instance.Settings.InputDeviceType);
	}

	private static void UpdateJoystickControllersEnabled()
	{
		foreach (Joystick joystick in TPSingleton<InputManager>.Instance.player.controllers.Joysticks)
		{
			joystick.enabled = TPSingleton<InputManager>.Instance.JoysticksEnabled;
		}
	}

	private static void UpdateKeyboardControllerEnabled()
	{
		TPSingleton<InputManager>.Instance.player.controllers.Keyboard.enabled = TPSingleton<InputManager>.Instance.KeyboardEnabled;
	}

	private void OnDevConsoleDisplayedChange(bool isVisible)
	{
		Player.isPlaying = !isVisible;
	}

	private void OnPointerOverWorldChanged(bool isPointerOverWorld)
	{
		Player.controllers.maps.SetMapsEnabled(isPointerOverWorld, 4);
	}

	private void OnLastActiveControllerChanged(Player player, Rewired.Controller controller)
	{
		switch (controller.type)
		{
		case Rewired.ControllerType.Joystick:
			UnityEngine.Cursor.visible = false;
			break;
		default:
			UnityEngine.Cursor.visible = true;
			break;
		}
		InputManager.LastActiveControllerChanged?.Invoke(controller.type);
	}

	[DevConsoleCommand("ToggleJoystickControllers")]
	private static void DebugToggleJoystickControllers()
	{
		TPSingleton<InputManager>.Instance.JoysticksEnabled = !TPSingleton<InputManager>.Instance.JoysticksEnabled;
		UpdateJoystickControllersEnabled();
	}

	public static void DebugOnMetaConditionDebugViewToggled(bool state)
	{
		if (state)
		{
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetAllMapsEnabled(state: false);
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(state: true, 11);
		}
		else
		{
			OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
		}
	}

	[DevConsoleCommand("ForceDisplayedControllerTypeReset")]
	public static void ForceDisplayedControllerTypeReset()
	{
		TPSingleton<InputManager>.Instance.DebugDisplayedControllerType = null;
	}

	[DevConsoleCommand("ForceDisplayedControllerType")]
	public static void ForceDisplayedControllerType(TheLastStand.Model.ControllerType controllerType)
	{
		TPSingleton<InputManager>.Instance.DebugDisplayedControllerType = controllerType;
	}
}
