using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class ZoomCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "Zoom";
	}

	public bool In { get; private set; }

	public bool Instant { get; private set; }

	public ZoomCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("In");
		if (bool.TryParse(xAttribute.Value, out var result))
		{
			In = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse ZoomCutsceneDefinition In Attribute value " + xAttribute.Value + " as a valid bool value.");
		}
		XAttribute xAttribute2 = obj.Attribute("Instant");
		if (xAttribute2 != null)
		{
			if (bool.TryParse(xAttribute2.Value, out var result2))
			{
				Instant = result2;
			}
			else
			{
				CLoggerManager.Log("Could not parse ZoomCutsceneDefinition Instant Attribute value " + xAttribute2.Value + " as a valid bool value.");
			}
		}
	}
}
