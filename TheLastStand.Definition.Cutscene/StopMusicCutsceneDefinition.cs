using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class StopMusicCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "StopMusic";
	}

	public bool OnlyIfVictoryTriggered { get; private set; }

	public StopMusicCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("OnlyIfVictoryTriggered");
		if (!string.IsNullOrEmpty(xAttribute?.Value))
		{
			if (bool.TryParse(xAttribute.Value, out var result))
			{
				OnlyIfVictoryTriggered = result;
			}
			else
			{
				CLoggerManager.Log("StopMusic OnlyIfVictoryTriggered: could not parse " + xAttribute.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
			}
		}
	}
}
