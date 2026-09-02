using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Tutorial;

public abstract class TutorialConditionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public bool Invert { get; private set; }

	public TutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Invert");
		if (xAttribute != null)
		{
			if (!bool.TryParse(xAttribute.Value, out var result))
			{
				CLoggerManager.Log("Could not parse TutorialConditionDefinition Invert attribute value " + xAttribute.Value + " to a valid bool!");
			}
			else
			{
				Invert = result;
			}
		}
		else
		{
			Invert = false;
		}
	}
}
