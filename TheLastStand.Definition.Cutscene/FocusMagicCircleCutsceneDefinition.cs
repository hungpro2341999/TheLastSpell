using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class FocusMagicCircleCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "FocusMagicCircle";
	}

	public bool ZoomIn { get; private set; } = true;

	public FocusMagicCircleCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("ZoomIn");
		if (xAttribute != null)
		{
			if (bool.TryParse(xAttribute.Value, out var result))
			{
				ZoomIn = result;
				return;
			}
			CLoggerManager.Log("Could not parse FocusMagicCircleCutsceneDefinition ZoomIn value " + xAttribute.Value + " as a valid bool value.");
			ZoomIn = true;
		}
		else
		{
			ZoomIn = true;
		}
	}
}
