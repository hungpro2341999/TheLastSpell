using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;

namespace TheLastStand.Definition.Tutorial;

public class DuringDayTurnTutorialConditionDefinition : TutorialConditionDefinition
{
	public static class Constants
	{
		public const string Name = "DuringDayTurn";
	}

	public Game.E_DayTurn DayTurn { get; private set; }

	public DuringDayTurnTutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("DayTurn");
		if (!Enum.TryParse<Game.E_DayTurn>(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("Could not parse DuringDayTurnTutorialConditionDefinition DayTurn value " + xAttribute.Value + " to a valid DayTurn!");
		}
		else
		{
			DayTurn = result;
		}
	}
}
