using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class ChangeMusicCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "ChangeMusic";
	}

	public bool Instant { get; private set; }

	public ChangeMusicCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Instant");
		if (xAttribute != null)
		{
			if (bool.TryParse(xAttribute.Value, out var result))
			{
				Instant = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse ChangeMusicCutsceneDefinition Instant Attribute value " + xAttribute.Value + " as a valid bool value.");
			}
		}
	}
}
