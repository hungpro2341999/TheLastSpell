using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class EvolutiveLevelArtSetStageCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "EvolutiveLevelArtSetStage";
	}

	public int StageIndex { get; private set; }

	public EvolutiveLevelArtSetStageCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		if (int.TryParse(xAttribute.Value, out var result))
		{
			StageIndex = result;
		}
		else
		{
			CLoggerManager.Log("EvolutiveLevelArtSetStageCutsceneDefinition Could not parse Value attribute into a valid int : " + xAttribute.Value, LogType.Error, CLogLevel.MAJOR);
		}
	}
}
