using TPLib;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.HUD;

public class HUDJoystickDynamicTarget : HUDJoystickTarget
{
	[SerializeField]
	private HUDJoystickTarget[] targets;

	[SerializeField]
	private HUDJoystickTarget defaultTarget;

	public override bool IsSelectable()
	{
		if (base.IsSelectable())
		{
			if (!(GetFirstAvailableTarget() != null))
			{
				return defaultTarget != null;
			}
			return true;
		}
		return false;
	}

	public override SelectionInfo GetSelectionInfo(Vector2? direction = null)
	{
		if (!IsSelectable())
		{
			LogSelectionNotSelectable();
			return SelectionInfo.Empty;
		}
		HUDJoystickTarget hUDJoystickTarget = GetFirstAvailableTarget();
		if (hUDJoystickTarget == null)
		{
			if (direction.HasValue)
			{
				hUDJoystickTarget = GetNextPanelForDirection(direction.Value);
			}
			if (hUDJoystickTarget == null)
			{
				hUDJoystickTarget = defaultTarget;
			}
		}
		return hUDJoystickTarget.GetSelectionInfo(direction);
	}

	private void LogSelectionNotSelectable()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.Log("Joystick SelectionInfo of " + base.gameObject.name + " isn't selectable !", CLogLevel.DETAILED, forcePrintInUnity: true);
		if (!base.IsSelectable())
		{
			LogTargetIsNotSelectable();
		}
		else if (targets.Length != 0)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.Log("The list of target is empty !", CLogLevel.DETAILED, forcePrintInUnity: true);
		}
		else if (defaultTarget != null)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.Log("The default target is null !", CLogLevel.DETAILED, forcePrintInUnity: true);
		}
	}

	private HUDJoystickTarget GetFirstAvailableTarget()
	{
		for (int i = 0; i < targets.Length; i++)
		{
			HUDJoystickTarget hUDJoystickTarget = targets[i];
			if (hUDJoystickTarget.IsSelectable())
			{
				return hUDJoystickTarget;
			}
		}
		return null;
	}
}
