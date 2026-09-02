using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "EvolutiveLevelArtSetActiveCurrentStage";
	}

	public bool Value { get; private set; }

	public EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		if (bool.TryParse(xAttribute.Value, out var result))
		{
			Value = result;
		}
		else
		{
			CLoggerManager.Log("EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition Unable to parse " + xAttribute.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
		}
	}
}
