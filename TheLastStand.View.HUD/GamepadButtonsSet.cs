using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.View.HUD;

[CreateAssetMenu(fileName = "New Gamepad Buttons Set", menuName = "TLS/Joystick Config/Gamepad Buttons Set", order = 100)]
public class GamepadButtonsSet : SerializedScriptableObject
{
	[SerializeField]
	private Dictionary<ControllerType, Sprite> iconsByHardwareGuid;

	public Sprite GetIcon()
	{
		if (TPSingleton<InputManager>.Instance.DebugDisplayedControllerType.HasValue && iconsByHardwareGuid.TryGetValue(TPSingleton<InputManager>.Instance.DebugDisplayedControllerType.Value, out var value))
		{
			return value;
		}
		Guid joystickGuid = TPSingleton<InputManager>.Instance.JoystickGuid;
		ControllerType controllerType = InputManager.JoystickConfig.ControllerTypeGuidTable.GetControllerType(joystickGuid);
		if (!iconsByHardwareGuid.TryGetValue(controllerType, out var value2))
		{
			return null;
		}
		return value2;
	}
}
