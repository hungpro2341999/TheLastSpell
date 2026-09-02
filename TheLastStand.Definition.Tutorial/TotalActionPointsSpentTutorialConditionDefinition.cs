using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;

namespace TheLastStand.Definition.Tutorial;

public class TotalActionPointsSpentTutorialConditionDefinition : TutorialConditionDefinition
{
	public static class Constants
	{
		public const string Name = "TotalActionPointsSpent";
	}

	public int Value { get; private set; }

	public TotalActionPointsSpentTutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		if (!int.TryParse(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("Could not parse TotalActionPointsSpentTutorialConditionDefinition value " + xAttribute.Value + " to a valid int!");
		}
		else
		{
			Value = result;
		}
	}
}
