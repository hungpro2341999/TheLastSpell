using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using DG.Tweening;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class InvertImageCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "InvertImage";
	}

	public float Duration { get; private set; }

	public Ease Easing { get; private set; }

	public InvertImageCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Duration");
		if (float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Duration = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse InvertImageCutsceneDefinition value " + xAttribute.Value + " as a valid float value.");
		}
		XAttribute xAttribute2 = obj.Attribute("Easing");
		if (Enum.TryParse<Ease>(xAttribute2.Value, out var result2))
		{
			Easing = result2;
		}
		else
		{
			CLoggerManager.Log("Could not parse InvertImageCutsceneDefinition value " + xAttribute2.Value + " as a valid Ease enum.");
		}
	}
}
