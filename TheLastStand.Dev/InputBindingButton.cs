using System;
using System.Collections.Generic;
using Rewired;
using TMPro;
using TPLib;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Dev;

[DisallowMultipleComponent]
public class InputBindingButton : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI actionName;

	[SerializeField]
	private BetterButton[] buttons;

	[SerializeField]
	private BetterButton resetActionButton;

	[SerializeField]
	private GameObject remappingFeedback;

	private List<ActionElementMap> actionElementMaps = new List<ActionElementMap>();

	private List<ControllerMap> maps = new List<ControllerMap>();

	public AxisRange AxisRange { get; private set; }

	public int CategoryId { get; private set; }

	public int ElementMapId { get; private set; }

	public InputAction InputAction { get; private set; }

	public event Action<KeyRemappingManager.RemappingInfo> ActionRemapButtonPressed;

	public event Action<int> ActionResetButtonPressed;

	public void DisplayRemappingFeedback(bool show)
	{
		remappingFeedback.SetActive(show);
	}

	public ActionElementMap GetActionElementMap(int index)
	{
		if (index >= actionElementMaps.Count)
		{
			return null;
		}
		return actionElementMaps[index];
	}

	public ControllerMap GetControllerMap(int index)
	{
		if (maps.Count == 0)
		{
			return TPSingleton<KeyRemappingManager>.Instance.GetControllerMap(ControllerType.Keyboard);
		}
		return maps[Mathf.Clamp(index, 0, maps.Count - 1)];
	}

	public void Init(InputAction action, int categoryId, AxisRange axisRange = AxisRange.Full, string overrideDescriptiveName = null)
	{
		actionElementMaps.Clear();
		maps.Clear();
		InputAction = action;
		CategoryId = categoryId;
		AxisRange = axisRange;
		actionName.SetText("KeyRemapping_ActionName_" + (overrideDescriptiveName ?? InputAction.name));
		InitWithAllMaps();
		resetActionButton.onClick.AddListener(delegate
		{
			this.ActionResetButtonPressed?.Invoke(InputAction.id);
		});
	}

	private void InitWithAllMaps()
	{
		List<ActionElementMap> list = new List<ActionElementMap>();
		foreach (ControllerMap allMap in TPSingleton<TheLastStand.Manager.InputManager>.Instance.Player.controllers.maps.GetAllMaps())
		{
			List<ActionElementMap> list2 = new List<ActionElementMap>();
			allMap.GetButtonMapsWithAction(InputAction.id, list2);
			foreach (ActionElementMap item in list2)
			{
				if (InputAction.type == InputActionType.Button || (AxisRange == AxisRange.Positive && item.axisContribution == Pole.Positive) || (AxisRange == AxisRange.Negative && item.axisContribution == Pole.Negative))
				{
					list.Add(item);
					actionElementMaps.Add(item);
					maps.Add(item.controllerMap);
				}
			}
		}
		for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i].ChangeText(string.Empty);
			if (i < list.Count)
			{
				buttons[i].ChangeText(list[i].elementIdentifierName);
			}
			int index = i;
			KeyRemappingManager.RemappingInfo remappingInfos = new KeyRemappingManager.RemappingInfo
			{
				ActionId = InputAction.id,
				ActionElementMap = GetActionElementMap(index),
				ControllerMap = GetControllerMap(index),
				AxisRange = AxisRange,
				BindingView = null
			};
			buttons[i].onClick.AddListener(delegate
			{
				this.ActionRemapButtonPressed?.Invoke(remappingInfos);
			});
		}
	}
}
