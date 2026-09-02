using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Controller.Settings;
using TheLastStand.View.Settings.KeyRemapping;
using UnityEngine;

namespace TheLastStand.Manager;

public class KeyRemappingManager : Manager<KeyRemappingManager>
{
	public struct RemappingInfo
	{
		public KeyRemappingBindingLineView BindingView;

		public int ActionId;

		public ControllerMap ControllerMap;

		public ActionElementMap ActionElementMap;

		public AxisRange AxisRange;
	}

	[SerializeField]
	private KeyCode[] unmappableKeyboardKeys;

	[SerializeField]
	private bool allowModifierKeys;

	[SerializeField]
	private bool displayJoystickMaps;

	private KeyRemappingView keyRemappingView;

	private bool conflictResolved;

	private bool conflictConfirmed;

	private InputMapper inputMapper;

	private RemappingInfo? pendingRemappingInfo;

	public bool DisplayJoystickMaps => displayJoystickMaps;

	public bool RemappingInProgress { get; private set; }

	public bool ResetAllInProgress { get; private set; }

	public static void Initialize()
	{
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView = KeyRemappingViewAccessor.KeyRemappingView;
		if (TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.Initialized)
		{
			TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.RefreshTexts();
			TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.RebuildLayout();
			return;
		}
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ConflictConfirmButton.onClick.RemoveAllListeners();
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ConflictCancelButton.onClick.RemoveAllListeners();
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ResetAllButton.onClick.RemoveAllListeners();
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ResetAllConfirmButton.onClick.RemoveAllListeners();
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ResetAllCancelButton.onClick.RemoveAllListeners();
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ConflictConfirmButton.onClick.AddListener(TPSingleton<KeyRemappingManager>.Instance.OnConfirmConflictButtonClicked);
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ConflictCancelButton.onClick.AddListener(TPSingleton<KeyRemappingManager>.Instance.OnCancelConflictButtonClicked);
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ResetAllButton.onClick.AddListener(TPSingleton<KeyRemappingManager>.Instance.OnResetAllButtonClicked);
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ResetAllConfirmButton.onClick.AddListener(TPSingleton<KeyRemappingManager>.Instance.OnConfirmResetAllButtonClicked);
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ResetAllCancelButton.onClick.AddListener(TPSingleton<KeyRemappingManager>.Instance.OnCancelResetAllButtonClicked);
		List<InputCategory> list = ReInput.mapping.ActionCategories.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].userAssignable)
			{
				continue;
			}
			KeyRemappingCategoryView keyRemappingCategoryView = TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.InstantiateInputCategory(list[i]);
			List<InputAction> list2 = ReInput.mapping.ActionsInCategory(list[i].id, sort: true).ToList();
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j].userAssignable)
				{
					KeyRemappingBindingLineView keyRemappingBindingLineView = keyRemappingCategoryView.InstantiateInputBindingLine();
					switch (list2[j].type)
					{
					case InputActionType.Button:
						keyRemappingBindingLineView.Initialize(list2[j], list[i].id);
						keyRemappingBindingLineView.ActionRemapButtonPressed += TPSingleton<KeyRemappingManager>.Instance.OnActionRemapButtonClicked;
						break;
					case InputActionType.Axis:
						keyRemappingBindingLineView.Initialize(list2[j], list[i].id, AxisRange.Positive, list2[j].positiveDescriptiveName);
						keyRemappingBindingLineView.ActionRemapButtonPressed += TPSingleton<KeyRemappingManager>.Instance.OnActionRemapButtonClicked;
						keyRemappingBindingLineView = keyRemappingCategoryView.InstantiateInputBindingLine();
						keyRemappingBindingLineView.Initialize(list2[j], list[i].id, AxisRange.Negative, list2[j].negativeDescriptiveName);
						keyRemappingBindingLineView.ActionRemapButtonPressed += TPSingleton<KeyRemappingManager>.Instance.OnActionRemapButtonClicked;
						break;
					}
				}
			}
		}
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.RefreshTexts();
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.RebuildLayout();
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.Initialized = true;
	}

	private IEnumerator CancelRemapping()
	{
		while (true)
		{
			InputMapper inputMapper = this.inputMapper;
			if (inputMapper != null && inputMapper.status == InputMapper.Status.Listening)
			{
				if (InputManager.GetButtonDown(23))
				{
					this.inputMapper.Stop();
					break;
				}
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator ConfirmResetAllInput()
	{
		while (ResetAllInProgress)
		{
			if (InputManager.GetButtonDown(23))
			{
				OnCancelResetAllButtonClicked();
				break;
			}
			if (InputManager.GetButtonDown(66))
			{
				OnConfirmResetAllButtonClicked();
				break;
			}
			yield return null;
		}
	}

	public ControllerMap GetControllerMap(ControllerType type)
	{
		return TPSingleton<InputManager>.Instance.Player.controllers.maps.GetFirstMapInCategory(type, 0, 0);
	}

	private IEnumerator HandleInputMapperConflict(InputMapper.ConflictFoundEventData conflictFoundEventData)
	{
		conflictResolved = false;
		string conflictedActionName = string.Empty;
		switch (conflictFoundEventData.conflicts[0].action.type)
		{
		case InputActionType.Button:
			conflictedActionName = conflictFoundEventData.conflicts[0].action.name;
			break;
		case InputActionType.Axis:
			conflictedActionName = ((conflictFoundEventData.conflicts[0].elementMap.axisContribution == Pole.Positive) ? conflictFoundEventData.conflicts[0].action.positiveDescriptiveName : conflictFoundEventData.conflicts[0].action.negativeDescriptiveName);
			break;
		}
		keyRemappingView.DisplayConflictSolver(show: true, conflictedActionName, conflictFoundEventData.conflicts[0].keyCode);
		if (conflictFoundEventData.isProtected)
		{
			conflictFoundEventData.responseCallback(InputMapper.ConflictResponse.Cancel);
		}
		else
		{
			conflictConfirmed = false;
			yield return new WaitUntil(() => conflictResolved || InputManager.GetButtonDown(23));
			conflictFoundEventData.responseCallback(conflictConfirmed ? InputMapper.ConflictResponse.Replace : InputMapper.ConflictResponse.Cancel);
		}
		keyRemappingView.DisplayConflictSolver(show: false);
		keyRemappingView.DisplayResetAllOption(show: true);
	}

	public void ResetAll()
	{
		TPSingleton<KeyRemappingManager>.Instance.Log("Resetting all mapping settings.");
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadDefaultMaps(ControllerType.Mouse);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
		SettingsController.SetKeyboardLayout(SettingsManager.GetKeyboardLayoutFromSystemLanguage());
		InputManager.RefreshRewiredKeyboardLayout();
		int keyboardLayoutId = InputManager.GetKeyboardLayoutId();
		TPSingleton<KeyRemappingManager>.Instance.Log($"Loading Rewired categories Default/World using Keyboard Layout Id {keyboardLayoutId} (= {(SettingsManager.E_KeyboardLayout)keyboardLayoutId})");
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadMap(ControllerType.Keyboard, 0, 0, InputManager.GetKeyboardLayoutId());
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadMap(ControllerType.Keyboard, 0, 3, InputManager.GetKeyboardLayoutId());
	}

	private void Refresh()
	{
		keyRemappingView.RefreshBindingViews();
	}

	private void StartRemapper(RemappingInfo info)
	{
		pendingRemappingInfo = info;
		inputMapper = new InputMapper();
		inputMapper.StartedEvent += OnInputMapperStartedEvent;
		inputMapper.StoppedEvent += OnInputMapperStoppedEvent;
		inputMapper.ConflictFoundEvent += OnInputMapperConflictFoundEvent;
		inputMapper.options.allowKeyboardKeysWithModifiers = allowModifierKeys;
		inputMapper.options.allowAxes = false;
		inputMapper.options.allowButtons = true;
		inputMapper.options.timeout = 36000f;
		inputMapper.options.isElementAllowedCallback = OnIsElementAllowed;
		InputMapper.Context mappingContext = new InputMapper.Context
		{
			actionId = info.ActionId,
			actionElementMapToReplace = info.ActionElementMap,
			controllerMap = info.ControllerMap,
			actionRange = info.AxisRange
		};
		inputMapper.Start(mappingContext);
		StartCoroutine(CancelRemapping());
	}

	private void OnInputMapperStartedEvent(InputMapper.StartedEventData startedEventData)
	{
		RemappingInProgress = true;
		keyRemappingView.DisplayResetAllOption(show: false);
	}

	private void OnInputMapperStoppedEvent(InputMapper.StoppedEventData stoppedEventData)
	{
		RemappingInProgress = false;
		pendingRemappingInfo?.BindingView.HideRemappingFeedback();
		pendingRemappingInfo = null;
		keyRemappingView.DisplayResetAllOption(show: true);
		keyRemappingView.AllowPanelNavigation(state: true);
		Refresh();
	}

	private void OnInputMapperConflictFoundEvent(InputMapper.ConflictFoundEventData conflictFoundEventData)
	{
		StartCoroutine(HandleInputMapperConflict(conflictFoundEventData));
	}

	private bool OnIsElementAllowed(ControllerPollingInfo info)
	{
		if (info.controllerType == ControllerType.Keyboard)
		{
			return unmappableKeyboardKeys.Count((KeyCode o) => o == info.keyboardKey) == 0;
		}
		return true;
	}

	private void OnActionRemapButtonClicked(RemappingInfo infos)
	{
		if (!RemappingInProgress)
		{
			StartRemapper(infos);
			keyRemappingView.AllowPanelNavigation(state: false);
		}
	}

	private void OnCancelResetAllButtonClicked()
	{
		keyRemappingView.DisplayResetAllPanel(show: false);
		ResetAllInProgress = false;
	}

	private void OnConfirmResetAllButtonClicked()
	{
		ResetAll();
		Refresh();
		keyRemappingView.DisplayResetAllPanel(show: false);
		ResetAllInProgress = false;
	}

	private void OnCancelConflictButtonClicked()
	{
		conflictConfirmed = false;
		conflictResolved = true;
	}

	private void OnConfirmConflictButtonClicked()
	{
		conflictConfirmed = true;
		conflictResolved = true;
	}

	public void OnResetActionButtonClicked(int actionId, AxisRange axisRange)
	{
		List<ActionElementMap> list = new List<ActionElementMap>();
		for (int num = ReInput.mapping.MapCategories.Count - 1; num >= 0; num--)
		{
			List<ActionElementMap> list2 = new List<ActionElementMap>();
			ReInput.mapping.GetKeyboardMapInstanceSavedOrDefault(TPSingleton<InputManager>.Instance.Player.id, num, InputManager.GetKeyboardLayoutId()).GetElementMapsWithAction(actionId, list2);
			list.AddRange(list2);
			ReInput.mapping.GetKeyboardMapInstanceSavedOrDefault(TPSingleton<InputManager>.Instance.Player.id, num, 0).GetElementMapsWithAction(actionId, list2);
			list.AddRange(list2);
		}
		list.RemoveAll((ActionElementMap o) => (axisRange == AxisRange.Positive && o.axisContribution == Pole.Negative) || (axisRange == AxisRange.Negative && o.axisContribution == Pole.Positive));
		list.Select((ActionElementMap o) => o.keyCode);
		GetControllerMap(ControllerType.Keyboard).DeleteElementMapsWithAction(actionId);
		GetControllerMap(ControllerType.Mouse).DeleteElementMapsWithAction(actionId);
	}

	private void OnResetAllButtonClicked()
	{
		ResetAllInProgress = true;
		keyRemappingView.DisplayResetAllPanel(show: true);
		StartCoroutine(ConfirmResetAllInput());
	}

	[DevConsoleCommand("KeyRemappingResetAll")]
	public static void DebugResetAll()
	{
		TPSingleton<KeyRemappingManager>.Instance.ResetAll();
	}

	[DevConsoleCommand("KeyRemappingSave")]
	public static void DebugSave()
	{
		InputManager.SaveMap();
	}

	[DevConsoleCommand("KeyRemappingInit")]
	public static void DebugInit()
	{
		Initialize();
	}

	[DevConsoleCommand("KeyRemappingShowKeyCodes")]
	public static void DebugShowKeyCodes(bool show = true)
	{
		Object.FindObjectsOfType<KeyRemappingBindingView>().ToList().ForEach(delegate(KeyRemappingBindingView o)
		{
			o.DebugShowRawKeyCode(show);
		});
	}
}
