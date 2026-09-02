using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;

namespace TheLastStand.Definition.Tutorial;

public class CurrentNightHourTutorialConditionDefinition : TutorialConditionDefinition
{
	public static class Constants
	{
		public const string Name = "CurrentNightHour";
	}

	public int NightHour { get; private set; }

	public CurrentNightHourTutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("NightHour");
		if (!int.TryParse(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("Could not parse CurrentNightHourTutorialConditionDefinition value " + xAttribute.Value + " to a valid int!");
		}
		else
		{
			NightHour = result;
		}
	}
}
