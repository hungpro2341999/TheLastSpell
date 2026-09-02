using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public abstract class GlyphIntValueBasedEffectDefinition : GlyphEffectDefinition
{
	public int Value { get; private set; }

	protected GlyphIntValueBasedEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		string text = xAttribute.Value.Replace(base.TokenVariables);
		if (!int.TryParse(text, out var result))
		{
			CLoggerManager.Log(GetType().FullName + " Unable to parse " + text + " (" + xAttribute.Value + ") into an int", LogType.Error, CLogLevel.MAJOR);
		}
		Value = result;
	}
}
