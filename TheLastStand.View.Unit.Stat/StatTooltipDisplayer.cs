using TPLib.Log;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.View.Generic;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Unit.Stat;

public class StatTooltipDisplayer : TooltipDisplayer, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Tooltip("If null, will try to automatically find a UnitStatDisplay on its gameObject")]
	[SerializeField]
	private UnitStatDisplay sourceUnitStatDisplay;

	[Tooltip("If true, sourceUnitStatDisplay will change its visuals when hovered")]
	[SerializeField]
	private bool displayHoverState = true;

	[Tooltip("Use the currently selected unit or overrides it with a custom one")]
	[SerializeField]
	private bool overrideUnit;

	[SerializeField]
	private FollowElement.FollowDatas followDatas = new FollowElement.FollowDatas();

	[SerializeField]
	private bool displayOnRight;

	[SerializeField]
	private Transform topLeftAnchor;

	[SerializeField]
	private Transform topRightAnchor;

	public StatTooltip StatTooltip => targetTooltip as StatTooltip;

	public void DisplayTooltip(bool display)
	{
		if (sourceUnitStatDisplay.StatDefinition == null)
		{
			CLoggerManager.Log("UnitStatDisplay can't be null! Aborting...", this, LogType.Error, CLogLevel.MAJOR);
			base.enabled = false;
			return;
		}
		if (displayHoverState && !InputManager.IsLastControllerJoystick)
		{
			sourceUnitStatDisplay.DisplayHover(display);
		}
		if (display)
		{
			if (overrideUnit)
			{
				StatTooltip.TooltipUnitStatDisplay.TargetUnit = sourceUnitStatDisplay.TargetUnit;
			}
			StatTooltip.SetContent(sourceUnitStatDisplay);
			targetTooltip.FollowElement.ChangeFollowDatas(followDatas);
			targetTooltip.FollowElement.ChangeTarget(displayOnRight ? topRightAnchor : topLeftAnchor);
			StatTooltip.UpdateAnchor(displayOnRight);
			targetTooltip.Display();
		}
		else
		{
			targetTooltip.Hide();
			if (overrideUnit)
			{
				StatTooltip.TooltipUnitStatDisplay.TargetUnit = null;
			}
		}
	}

	public override void DisplayTooltip()
	{
		DisplayTooltip(display: true);
	}

	public override void HideTooltip()
	{
		DisplayTooltip(display: false);
	}

	private void Awake()
	{
		if (targetTooltip == null)
		{
			targetTooltip = PlayableUnitManager.StatTooltip;
		}
		if (sourceUnitStatDisplay == null)
		{
			sourceUnitStatDisplay = GetComponent<UnitStatDisplay>();
		}
	}
}
