using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheLastStand.View.HUD;

[CreateAssetMenu(fileName = "New Gamepad Buttons Sets Table", menuName = "TLS/Joystick Config/Gamepad Buttons Sets Table", order = 100)]
public class GamepadButtonsSetsTable : SerializedScriptableObject
{
	[SerializeField]
	private Dictionary<E_GamepadButtonType, GamepadButtonsSet> table;

	public GamepadButtonsSet GetSetForButtonType(E_GamepadButtonType gamepadButtonType)
	{
		if (!table.TryGetValue(gamepadButtonType, out var value))
		{
			return null;
		}
		return value;
	}
}
