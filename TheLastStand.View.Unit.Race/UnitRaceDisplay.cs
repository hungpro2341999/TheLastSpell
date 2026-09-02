using TMPro;
using TheLastStand.Definition.Unit.Race;
using TheLastStand.Model.Unit;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Race;

public class UnitRaceDisplay : MonoBehaviour
{
	[SerializeField]
	private Image raceIcon;

	[SerializeField]
	private TextMeshProUGUI raceName;

	[SerializeField]
	private TextMeshProUGUI raceDescription;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	private bool isIconInHoveredState;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public PlayableUnit PlayableUnit { get; private set; }

	public RaceDefinition RaceDefinition { get; private set; }

	public void SetContent(RaceDefinition raceDefinition, PlayableUnit playableUnit)
	{
		RaceDefinition = raceDefinition;
		PlayableUnit = playableUnit;
	}

	public void SetIconInHoveredState(bool isHovered)
	{
		isIconInHoveredState = isHovered;
	}

	public void Refresh()
	{
		if (RaceDefinition != null)
		{
			if (raceIcon != null)
			{
				raceIcon.sprite = (isIconInHoveredState ? RaceDefinition.RaceHoveredSprite : RaceDefinition.RaceSprite);
			}
			if (raceName != null)
			{
				raceName.text = RaceDefinition.Name;
			}
			if (raceDescription != null)
			{
				raceDescription.text = RaceDefinition.GetDescription(PlayableUnit);
			}
		}
	}
}
