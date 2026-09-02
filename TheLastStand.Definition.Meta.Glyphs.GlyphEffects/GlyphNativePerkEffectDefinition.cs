using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphNativePerkEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "NativePerk";

	public PerkDefinition PerkDefinition { get; private set; }

	public bool ForceHideTooltip { get; private set; }

	public GlyphNativePerkEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		string text = obj.Attribute("PerkId").Value.Replace(base.TokenVariables);
		if (PlayableUnitDatabase.PerkDefinitions.TryGetValue(text, out var value))
		{
			PerkDefinition = value;
		}
		else
		{
			CLoggerManager.Log("NativePerk Perk " + text + " was not found!", LogType.Error, CLogLevel.MAJOR);
		}
		XAttribute xAttribute = obj.Attribute("ForceHideTooltip");
		if (xAttribute != null)
		{
			ForceHideTooltip = bool.Parse(xAttribute.Value);
		}
	}
}
