using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Unit.Race;

public class RaceTooltip : TooltipBase
{
	[SerializeField]
	private UnitRaceDisplay unitRaceDisplay;

	[SerializeField]
	private RectTransform tooltipRectTransform;

	[SerializeField]
	private GameObject racePanel;

	public UnitRaceDisplay LinkedUnitRaceDisplay { get; private set; }

	public RectTransform TooltipPanel => tooltipPanel;

	public RectTransform TooltipRectTransform => tooltipRectTransform;

	public void SetContent(UnitRaceDisplay linkedUnitRaceDisplay)
	{
		LinkedUnitRaceDisplay = linkedUnitRaceDisplay;
		RefreshRaceDefinition();
	}

	public void UpdateAnchors(bool displayTowardsRight, bool displayTop = false)
	{
		Vector2 vector = ((!displayTowardsRight) ? Vector2.one : Vector2.up);
		if (displayTop)
		{
			vector.y = 0f;
		}
		TooltipRectTransform.anchorMin = vector;
		TooltipRectTransform.anchorMax = vector;
		TooltipRectTransform.pivot = vector;
	}

	protected override void Awake()
	{
		racePanel.SetActive(value: true);
		base.Awake();
	}

	protected override bool CanBeDisplayed()
	{
		return unitRaceDisplay.RaceDefinition != null;
	}

	protected override void RefreshContent()
	{
		RefreshRaceDefinition();
		unitRaceDisplay.Refresh();
	}

	private void RefreshRaceDefinition()
	{
		if (LinkedUnitRaceDisplay != null && LinkedUnitRaceDisplay.RaceDefinition?.Id != unitRaceDisplay.RaceDefinition?.Id)
		{
			unitRaceDisplay.SetContent(LinkedUnitRaceDisplay.RaceDefinition, LinkedUnitRaceDisplay.PlayableUnit);
		}
	}
}
