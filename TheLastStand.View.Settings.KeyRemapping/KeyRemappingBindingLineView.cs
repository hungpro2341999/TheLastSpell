using System;
using System.Collections.Generic;
using Rewired;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings.KeyRemapping;

public class KeyRemappingBindingLineView : MonoBehaviour
{
	private static class Constants
	{
		public const string GameObjectNamePrefix = "Action Panel - ";
	}

	[SerializeField]
	private TextMeshProUGUI actionNameText;

	[SerializeField]
	private LocalizedFont actionNameLocalizedFont;

	[SerializeField]
	private KeyRemappingBindingView[] bindingButtons;

	private readonly List<ActionElementMap> actionElementMaps = new List<ActionElementMap>();

	private readonly List<ControllerMap> controllerMaps = new List<ControllerMap>();

	private string overrideName;

	public AxisRange AxisRange { get; private set; }

	public int CategoryId { get; private set; }

	public InputAction InputAction { get; private set; }

	public event Action<KeyRemappingManager.RemappingInfo> ActionRemapButtonPressed;

	public void HideRemappingFeedback()
	{
		for (int num = bindingButtons.Length - 1; num >= 0; num--)
		{
			bindingButtons[num].DisplayRemappingFeedback(show: false);
			bindingButtons[num].Highlight(state: false);
		}
	}

	public void Initialize(InputAction action, int categoryId, AxisRange axisRange = AxisRange.Full, string overrideName = null)
	{
		base.transform.name = "Action Panel - " + action.name;
		InputAction = action;
		CategoryId = categoryId;
		AxisRange = axisRange;
		this.overrideName = overrideName;
		Refresh();
	}

	public void Refresh()
	{
		actionElementMaps.Clear();
		controllerMaps.Clear();
		for (int i = 0; i < bindingButtons.Length; i++)
		{
			bindingButtons[i].Button.onClick.RemoveAllListeners();
		}
		InitializeBindingsWithAllMaps();
		HideRemappingFeedback();
		RefreshTexts();
	}

	public void RefreshTexts()
	{
		actionNameText.SetText(Localizer.Get("KeyRemapping_ActionName_" + (overrideName ?? InputAction.name)));
		if (actionNameLocalizedFont != null)
		{
			actionNameLocalizedFont.RefreshFont();
		}
		for (int i = 0; i < bindingButtons.Length; i++)
		{
			bindingButtons[i].RefreshLocalizedKeyCode();
		}
	}

	private ActionElementMap GetActionElementMap(int index)
	{
		if (index < actionElementMaps.Count)
		{
			return actionElementMaps[index];
		}
		return null;
	}

	private ControllerMap GetControllerMap(int index)
	{
		if (controllerMaps.Count != 0)
		{
			return controllerMaps[Mathf.Clamp(index, 0, controllerMaps.Count - 1)];
		}
		return TPSingleton<KeyRemappingManager>.Instance.GetControllerMap(ControllerType.Keyboard);
	}

	private void InitializeBindingsWithAllMaps()
	{
		List<ActionElementMap> list = new List<ActionElementMap>();
		foreach (ControllerMap allMap in TPSingleton<TheLastStand.Manager.InputManager>.Instance.Player.controllers.maps.GetAllMaps())
		{
			if (allMap.controllerType == ControllerType.Joystick && !TPSingleton<KeyRemappingManager>.Instance.DisplayJoystickMaps)
			{
				continue;
			}
			List<ActionElementMap> list2 = new List<ActionElementMap>();
			allMap.GetButtonMapsWithAction(InputAction.id, list2);
			foreach (ActionElementMap item in list2)
			{
				if (InputAction.type == InputActionType.Button || (AxisRange == AxisRange.Positive && item.axisContribution == Pole.Positive) || (AxisRange == AxisRange.Negative && item.axisContribution == Pole.Negative))
				{
					list.Add(item);
					actionElementMaps.Add(item);
					controllerMaps.Add(item.controllerMap);
				}
			}
		}
		for (int i = 0; i < bindingButtons.Length; i++)
		{
			if (i < list.Count)
			{
				bindingButtons[i].SetKeyCode(list[i].keyCode);
				bindingButtons[i].RefreshText(list[i].elementIdentifierName);
			}
			else
			{
				bindingButtons[i].SetKeyCode(KeyCode.None);
				bindingButtons[i].RefreshText(string.Empty);
			}
			int elementMapId = i;
			KeyRemappingManager.RemappingInfo remappingInfos = new KeyRemappingManager.RemappingInfo
			{
				ActionId = InputAction.id,
				ActionElementMap = GetActionElementMap(elementMapId),
				ControllerMap = GetControllerMap(elementMapId),
				AxisRange = AxisRange,
				BindingView = this
			};
			bindingButtons[i].Button.onClick.AddListener(delegate
			{
				if (!TPSingleton<KeyRemappingManager>.Instance.RemappingInProgress)
				{
					this.ActionRemapButtonPressed?.Invoke(remappingInfos);
					bindingButtons[elementMapId].DisplayRemappingFeedback(show: true);
				}
			});
		}
	}

	[ContextMenu("Log Default Key")]
	private void LogDefaultKey()
	{
		TPSingleton<KeyRemappingManager>.Instance.OnResetActionButtonClicked(InputAction.id, AxisRange);
	}
}
