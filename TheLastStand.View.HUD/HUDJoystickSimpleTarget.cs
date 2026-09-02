using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class HUDJoystickSimpleTarget : HUDJoystickTarget
{
	[SerializeField]
	[Tooltip("All selectables belonging to the HUD target. First available will be selected on HUD target selected.")]
	private List<Selectable> selectables;

	public override bool IsSelectable()
	{
		if (base.IsSelectable())
		{
			return GetFirstAvailableSelectable() != null;
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
		Selectable firstAvailableSelectable = GetFirstAvailableSelectable();
		if (firstAvailableSelectable == null)
		{
			TPSingleton<UIManager>.Instance.LogWarning("No Selectable has been found for selection on " + base.transform.name + ".", base.gameObject);
			return SelectionInfo.Empty;
		}
		return new SelectionInfo
		{
			HUDTarget = this,
			Selectable = firstAvailableSelectable
		};
	}

	public void ClearSelectables()
	{
		foreach (Selectable selectable in selectables)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.UnregisterSelectable(selectable);
		}
		selectables.Clear();
	}

	public void ClearUnavailableNavigations()
	{
		if (navigation.Up != null && !navigation.Up.IsSelectable())
		{
			navigation.Up = null;
		}
		if (navigation.UpRight != null && !navigation.UpRight.IsSelectable())
		{
			navigation.UpRight = null;
		}
		if (navigation.UpLeft != null && !navigation.UpLeft.IsSelectable())
		{
			navigation.UpLeft = null;
		}
		if (navigation.Down != null && !navigation.Down.IsSelectable())
		{
			navigation.Down = null;
		}
		if (navigation.DownRight != null && !navigation.DownRight.IsSelectable())
		{
			navigation.DownRight = null;
		}
		if (navigation.DownLeft != null && !navigation.DownLeft.IsSelectable())
		{
			navigation.DownLeft = null;
		}
		if (navigation.Left != null && !navigation.Left.IsSelectable())
		{
			navigation.Left = null;
		}
		if (navigation.Right != null && !navigation.Right.IsSelectable())
		{
			navigation.Right = null;
		}
	}

	public void AddSelectable(Selectable selectable)
	{
		selectables.Add(selectable);
		TPSingleton<HUDJoystickNavigationManager>.Instance.RegisterSelectable(selectable, this);
	}

	public void AddSelectables(IEnumerable<Selectable> selectables)
	{
		foreach (Selectable selectable in selectables)
		{
			AddSelectable(selectable);
			TPSingleton<HUDJoystickNavigationManager>.Instance.RegisterSelectable(selectable, this);
		}
	}

	public void ClearMissingSelectables()
	{
		for (int num = selectables.Count - 1; num >= 0; num--)
		{
			if (selectables[num] == null)
			{
				selectables.RemoveAt(num);
			}
		}
	}

	private Selectable GetFirstAvailableSelectable()
	{
		for (int i = 0; i < selectables.Count; i++)
		{
			Selectable selectable = selectables[i];
			if (!(selectable == null) && selectable.IsInteractable() && selectable.gameObject.activeInHierarchy)
			{
				return selectable;
			}
		}
		return null;
	}

	private void LogSelectionNotSelectable()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.Log("Joystick SelectionInfo of " + base.gameObject.name + " isn't selectable !", CLogLevel.DETAILED, forcePrintInUnity: true);
		if (!base.IsSelectable())
		{
			LogTargetIsNotSelectable();
		}
		else if (selectables.Count == 0)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.Log("The list of selectable is empty !", CLogLevel.DETAILED, forcePrintInUnity: true);
		}
	}

	private void Awake()
	{
		for (int i = 0; i < selectables.Count; i++)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.RegisterSelectable(selectables[i], this);
		}
	}

	private void OnDestroy()
	{
		if (TPSingleton<HUDJoystickNavigationManager>.Exist())
		{
			for (int i = 0; i < selectables.Count; i++)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.UnregisterSelectable(selectables[i]);
			}
		}
	}
}
