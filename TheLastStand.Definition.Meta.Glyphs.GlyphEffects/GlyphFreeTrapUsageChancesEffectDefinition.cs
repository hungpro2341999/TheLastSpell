using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphFreeTrapUsageChancesEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "FreeTrapUsageChances";

	public GlyphFreeTrapUsageChancesEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
