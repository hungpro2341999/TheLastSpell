using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphDamnedSoulsPercentageModifierEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "DamnedSoulsPercentageModifier";

	public GlyphDamnedSoulsPercentageModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
