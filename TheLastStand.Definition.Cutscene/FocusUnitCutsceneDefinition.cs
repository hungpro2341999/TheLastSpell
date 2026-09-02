using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class FocusUnitCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "FocusUnit";
	}

	public bool ZoomIn { get; private set; } = true;

	public FocusUnitCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
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
			CLoggerManager.Log("Could not parse FocusUnitCutsceneDefinition ZoomIn value " + xAttribute.Value + " as a valid bool value.");
			ZoomIn = true;
		}
	}
}
