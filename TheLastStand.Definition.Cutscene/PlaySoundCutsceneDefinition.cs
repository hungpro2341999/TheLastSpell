using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class PlaySoundCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "PlaySound";
	}

	public string AudioClipPath { get; private set; }

	public float Delay { get; private set; }

	public PlaySoundCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("Path");
		AudioClipPath = xElement.Value;
		XElement xElement2 = obj.Element("Delay");
		if (xElement2 != null)
		{
			if (float.TryParse(xElement2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				Delay = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse " + xElement2.Value + " as a valid float value in PlaySoundCutsceneDefinition!");
			}
		}
	}
}
