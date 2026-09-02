using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphIncreaseStartingGearLevelEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "IncreaseStartingGearLevel";

	public string LevelTreeId { get; private set; }

	public Dictionary<int, int> WeightBonusByLevelProbability { get; set; } = new Dictionary<int, int>();

	public GlyphIncreaseStartingGearLevelEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute == null || string.IsNullOrEmpty(xAttribute.Value))
		{
			CLoggerManager.Log("IncreaseStartingGearLevel has an invalid Id or Id doesn't exist !", LogType.Error, CLogLevel.MAJOR);
			return;
		}
		LevelTreeId = xAttribute.Value.Replace(base.TokenVariables);
		foreach (XElement item in xElement.Elements("Probability"))
		{
			XAttribute xAttribute2 = item.Attribute("Weight");
			if (xAttribute2 == null || !int.TryParse(xAttribute2.Value.Replace(base.TokenVariables), out var result))
			{
				CLoggerManager.Log("IncreaseStartingGearLevel Probability has an invalid Weight or Weight doesn't exist !", LogType.Error, CLogLevel.MAJOR);
				continue;
			}
			if (!int.TryParse(item.Value.Replace(base.TokenVariables), out var result2))
			{
				CLoggerManager.Log("IncreaseStartingGearLevel Probability has an invalid Value !", LogType.Error, CLogLevel.MAJOR);
				continue;
			}
			WeightBonusByLevelProbability.AddValueOrCreateKey(result2, result, (int a, int b) => a + b);
		}
	}
}
