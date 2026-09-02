using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class PlayCamShakeEffectCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "PlayCamShakeEffect";

		public const string DataAnimationCurvePath = "AnimationCurves/";
	}

	public DataAnimationCurve DataAnimationCurve { get; private set; }

	public float DelayBetweenEachShake { get; private set; }

	public float Duration { get; private set; }

	public float DurationOfEachShake { get; private set; }

	public float IntensityMultiplier { get; private set; }

	public bool WaitCamShake { get; private set; }

	public PlayCamShakeEffectCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("DataAnimationCurve");
		DataAnimationCurve = ResourcePooler.LoadOnce<DataAnimationCurve>(Path.Combine("AnimationCurves/", xAttribute.Value));
		XAttribute xAttribute2 = obj.Attribute("DelayBetweenEachShake");
		if (float.TryParse(xAttribute2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			DelayBetweenEachShake = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse PlayCamShakeEffectCutsceneDefinition value " + xAttribute2.Value + " as a valid float value.");
		}
		XAttribute xAttribute3 = obj.Attribute("Duration");
		if (float.TryParse(xAttribute3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			Duration = result2;
		}
		else
		{
			CLoggerManager.Log("Could not parse PlayCamShakeEffectCutsceneDefinition value " + xAttribute3.Value + " as a valid float value.");
		}
		XAttribute xAttribute4 = obj.Attribute("DurationOfEachShake");
		if (float.TryParse(xAttribute4.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
		{
			DurationOfEachShake = result3;
		}
		else
		{
			CLoggerManager.Log("Could not parse PlayCamShakeEffectCutsceneDefinition value " + xAttribute4.Value + " as a valid float value.");
		}
		XAttribute xAttribute5 = obj.Attribute("IntensityMultiplier");
		if (float.TryParse(xAttribute5.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result4))
		{
			IntensityMultiplier = result4;
		}
		else
		{
			CLoggerManager.Log("Could not parse PlayCamShakeEffectCutsceneDefinition value " + xAttribute5.Value + " as a valid float value.");
		}
		XAttribute xAttribute6 = obj.Attribute("WaitCamShake");
		if (xAttribute6 != null)
		{
			if (bool.TryParse(xAttribute6.Value, out var result5))
			{
				WaitCamShake = result5;
			}
			else
			{
				CLoggerManager.Log("Could not parse PlayCamShakeEffectCutsceneDefinition value " + xAttribute6.Value + " as a valid bool value.");
			}
		}
	}
}
