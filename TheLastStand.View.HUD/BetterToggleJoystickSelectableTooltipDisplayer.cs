using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.Generic;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.HUD;

public class BetterToggleJoystickSelectableTooltipDisplayer : MonoBehaviour
{
	[SerializeField]
	private TooltipDisplayer tooltipDisplayer;

	[SerializeField]
	private bool overrideFollowData = true;

	[SerializeField]
	private FollowElement.FollowDatas followDatas;

	private void OnEnable()
	{
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	private void OnDisable()
	{
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if (!(this == null) && !(EventSystem.current.currentSelectedGameObject != base.gameObject) && !(tooltipDisplayer == null) && tooltipDisplayer != null)
		{
			if (showTooltips)
			{
				DisplayTooltip();
			}
			else
			{
				HideTooltip();
			}
		}
	}

	public void OnSelect()
	{
		if (InputManager.IsLastControllerJoystick && TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips && tooltipDisplayer != null)
		{
			DisplayTooltip();
		}
	}

	public void OnDeselect()
	{
		if (InputManager.IsLastControllerJoystick && tooltipDisplayer != null && tooltipDisplayer.Displayed)
		{
			HideTooltip();
		}
	}

	private void DisplayTooltip()
	{
		SetCustomFollowData();
		tooltipDisplayer.DisplayTooltip();
	}

	private void HideTooltip()
	{
		ResetFollowData();
		tooltipDisplayer.HideTooltip();
	}

	private void SetCustomFollowData()
	{
		if (followDatas != null && overrideFollowData)
		{
			FollowElement followElement = tooltipDisplayer.GetTooltip().FollowElement;
			followElement.FollowElementDatas.FollowTarget = followDatas.FollowTarget;
			followElement.FollowElementDatas.Offset = followDatas.Offset;
		}
	}

	private void ResetFollowData()
	{
		if (followDatas != null && overrideFollowData)
		{
			FollowElement followElement = tooltipDisplayer.GetTooltip().FollowElement;
			followElement.FollowElementDatas.FollowTarget = null;
			followElement.RestoreFollowDatasOffset();
		}
	}
}
