using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphGoldScavengingPercentageModifierEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "GoldScavengingPercentageModifier";

	public GlyphGoldScavengingPercentageModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
