using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlidePlaySoundDefinition : AnimatedCutsceneSlideItemDefinition
{
	public class Constants
	{
		public const string Id = "PlaySound";
	}

	public string ClipAssetName { get; private set; }

	public float Delay { get; private set; }

	public AnimatedCutsceneSlidePlaySoundDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("ClipAssetName");
		ClipAssetName = xAttribute.Value;
		XElement xElement = obj.Element("Delay");
		if (xElement != null)
		{
			if (!float.TryParse(xElement.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log("Unable to parse " + xElement.Value + " to a valid float value.", LogType.Error);
			}
			else
			{
				Delay = result;
			}
		}
	}
}
