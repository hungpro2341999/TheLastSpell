using TheLastStand.Manager.Unit;
using TheLastStand.View.Generic;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Unit.Race;

public class RaceTooltipDisplayer : TooltipDisplayer
{
	[SerializeField]
	private UnitRaceDisplay unitRaceDisplay;

	[SerializeField]
	private bool changeRaceIconOnHover;

	[SerializeField]
	private FollowElement.FollowDatas followData = new FollowElement.FollowDatas();

	public void DisplayTooltip(bool display)
	{
		if ((display && unitRaceDisplay.RaceDefinition == null) || !(targetTooltip is RaceTooltip raceTooltip))
		{
			return;
		}
		if (display)
		{
			raceTooltip.SetContent(unitRaceDisplay);
			if (followData.FollowTarget != null)
			{
				raceTooltip.FollowElement.ChangeFollowDatas(followData);
			}
			raceTooltip.Display();
		}
		else
		{
			raceTooltip.Hide();
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

	public override void OnPointerEnter(PointerEventData eventData)
	{
		SetOnSprite();
		DisplayTooltip(display: true);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		SetOffSprite();
		DisplayTooltip(display: false);
	}

	public void SetOnSprite()
	{
		if (changeRaceIconOnHover)
		{
			unitRaceDisplay.SetIconInHoveredState(isHovered: true);
			unitRaceDisplay.Refresh();
		}
	}

	public void SetOffSprite()
	{
		unitRaceDisplay.SetIconInHoveredState(isHovered: false);
		unitRaceDisplay.Refresh();
	}

	private void Awake()
	{
		if (targetTooltip == null)
		{
			targetTooltip = PlayableUnitManager.RaceTooltip;
		}
	}
}
