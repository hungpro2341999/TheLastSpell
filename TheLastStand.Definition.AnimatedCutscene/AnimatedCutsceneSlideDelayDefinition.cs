using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlideDelayDefinition : AnimatedCutsceneSlideItemDefinition
{
	public class Constants
	{
		public const string Id = "Delay";
	}

	public float Delay { get; private set; }

	public bool Unskippable { get; private set; }

	public AnimatedCutsceneSlideDelayDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Duration");
		if (!float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			CLoggerManager.Log("Unable to parse " + xAttribute.Value + " to a valid float value.", LogType.Error);
			return;
		}
		Delay = result;
		Unskippable = xElement.Element("Unskippable") != null;
	}
}
