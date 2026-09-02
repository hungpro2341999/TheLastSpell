using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphDamnedSoulsScavengingPercentageModifierEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "DamnedSoulsScavengingPercentageModifier";

	public GlyphDamnedSoulsScavengingPercentageModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
