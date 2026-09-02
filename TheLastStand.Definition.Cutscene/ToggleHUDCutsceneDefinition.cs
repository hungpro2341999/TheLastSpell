using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class ToggleHUDCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public class Constants
	{
		public const string Id = "ToggleHUD";
	}

	public bool Display { get; private set; }

	public bool OnlyIfVictoryTriggered { get; private set; }

	public ToggleHUDCutsceneDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		if (bool.TryParse(obj.Attribute("Display")?.Value, out var result))
		{
			Display = result;
		}
		else
		{
			CLoggerManager.Log("Missing or Incorrect Display Attribute in ToggleHUD Element.", LogType.Error, CLogLevel.MAJOR);
		}
		XAttribute xAttribute = obj.Attribute("OnlyIfVictoryTriggered");
		if (!string.IsNullOrEmpty(xAttribute?.Value))
		{
			if (bool.TryParse(xAttribute.Value, out var result2))
			{
				OnlyIfVictoryTriggered = result2;
			}
			else
			{
				CLoggerManager.Log("ToggleHUD OnlyIfVictoryTriggered: could not parse " + xAttribute.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
			}
		}
	}
}
