using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;

namespace TheLastStand.Definition.Tutorial;

public class DuringNightTutorialConditionDefinition : TutorialConditionDefinition
{
	public static class Constants
	{
		public const string Name = "DuringNight";
	}

	public bool BossNightOnly { get; private set; }

	public DuringNightTutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("BossNightOnly");
		if (xAttribute != null)
		{
			if (!bool.TryParse(xAttribute.Value, out var result))
			{
				CLoggerManager.Log("Could not parse DuringNightTutorialConditionDefinition BossNightOnly value " + xAttribute.Value + " to a valid bool!");
			}
			else
			{
				BossNightOnly = result;
			}
		}
	}
}
