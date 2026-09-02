using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public abstract class AFadeCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public Color Color { get; private set; } = Color.white;

	public float Duration { get; private set; }

	public bool WaitDuration { get; private set; } = true;

	protected AFadeCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Color");
		if (ColorUtility.TryParseHtmlString(xAttribute.Value, out var color))
		{
			Color = color;
		}
		else
		{
			CLoggerManager.Log("Could not parse " + GetType().Name + " value " + xAttribute.Value + " as a valid color.");
		}
		XAttribute xAttribute2 = xElement.Attribute("Duration");
		if (float.TryParse(xAttribute2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Duration = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse " + GetType().Name + " value " + xAttribute2.Value + " as a valid float value.");
		}
		XAttribute xAttribute3 = xElement.Attribute("WaitDuration");
		if (xAttribute3 != null)
		{
			if (bool.TryParse(xAttribute3.Value, out var result2))
			{
				WaitDuration = result2;
				return;
			}
			CLoggerManager.Log("Could not parse " + GetType().Name + " waitDuration value " + xAttribute3.Value + " as a valid bool value.");
			WaitDuration = true;
		}
	}
}
