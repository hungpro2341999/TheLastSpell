using System;
using TPLib;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

[DisallowMultipleComponent]
public abstract class HUDJoystickTarget : MonoBehaviour
{
	[Serializable]
	public struct Navigation
	{
		public HUDJoystickTarget Up;

		public HUDJoystickTarget UpRight;

		public HUDJoystickTarget UpLeft;

		public HUDJoystickTarget Down;

		public HUDJoystickTarget DownRight;

		public HUDJoystickTarget DownLeft;

		public HUDJoystickTarget Left;

		public HUDJoystickTarget Right;
	}

	public struct SelectionInfo
	{
		public HUDJoystickTarget HUDTarget;

		public Selectable Selectable;

		public static SelectionInfo Empty => default(SelectionInfo);
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	public bool NavigationEnabled = true;

	[SerializeField]
	protected Navigation navigation;

	[SerializeField]
	private UnityEvent onSelect;

	[SerializeField]
	private UnityEvent onDeselect;

	public abstract SelectionInfo GetSelectionInfo(Vector2? direction = null);

	public virtual bool IsSelectable()
	{
		if (NavigationEnabled && base.gameObject.activeInHierarchy)
		{
			if (!(canvas == null))
			{
				return canvas.enabled;
			}
			return true;
		}
		return false;
	}

	public void RaiseSelectEvent()
	{
		onSelect?.Invoke();
	}

	public void RaiseDeselectEvent()
	{
		onDeselect?.Invoke();
	}

	public HUDJoystickTarget GetNextPanelForDirection(Vector2 direction)
	{
		int num = Mathf.FloorToInt((Vector2.SignedAngle(direction, Vector2.down) + 180f + 22.5f) / 45f) % 8;
		bool flag = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
		HUDJoystickTarget hUDJoystickTarget = num switch
		{
			0 => (navigation.Up != null) ? navigation.Up : ((direction.x > 0f) ? navigation.UpRight : navigation.UpLeft), 
			1 => (navigation.UpRight != null) ? navigation.UpRight : (flag ? navigation.Right : navigation.Up), 
			2 => (navigation.Right != null) ? navigation.Right : ((direction.y > 0f) ? navigation.UpRight : navigation.DownRight), 
			3 => (navigation.DownRight != null) ? navigation.DownRight : (flag ? navigation.Right : navigation.Down), 
			4 => (navigation.Down != null) ? navigation.Down : ((direction.x > 0f) ? navigation.DownRight : navigation.DownLeft), 
			5 => (navigation.DownLeft != null) ? navigation.DownLeft : (flag ? navigation.Left : navigation.Down), 
			6 => (navigation.Left != null) ? navigation.Left : ((direction.y > 0f) ? navigation.UpLeft : navigation.DownLeft), 
			7 => (navigation.UpLeft != null) ? navigation.UpLeft : (flag ? navigation.Left : navigation.Up), 
			_ => null, 
		};
		if (hUDJoystickTarget != null && !hUDJoystickTarget.IsSelectable())
		{
			return hUDJoystickTarget.GetNextPanelForDirection(direction);
		}
		return hUDJoystickTarget;
	}

	public void LogTargetIsNotSelectable()
	{
		if (!NavigationEnabled)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.Log("The navigation of the target is not enabled !", CLogLevel.DETAILED, forcePrintInUnity: true);
		}
		else if (!base.gameObject.activeInHierarchy)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.Log("The gameObject of the target is not active in the hierarchy !", CLogLevel.DETAILED, forcePrintInUnity: true);
		}
		else if (canvas == null || canvas.enabled)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.Log("The canvas of the target is null or not enabled !", CLogLevel.DETAILED, forcePrintInUnity: true);
		}
	}
}
