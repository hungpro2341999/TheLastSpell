using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class OverrideCurrentSpawnWaveCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "OverrideCurrentSpawnWave";
	}

	public string WaveId;

	public string DirectionsId;

	public OverrideCurrentSpawnWaveCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		WaveId = obj.Attribute("WaveId")?.Value;
		DirectionsId = obj.Attribute("DirectionsId")?.Value;
	}
}
