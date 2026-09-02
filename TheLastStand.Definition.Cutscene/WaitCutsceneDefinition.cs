using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class WaitCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "Wait";
	}

	public float Duration { get; private set; }

	public WaitCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		if (float.TryParse(xElement.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Duration = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse WaitCutsceneDefinition value " + xElement.Value + " as a valid float value.");
		}
	}
}
