using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphBonusSellingRatioEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "BonusSellingRatio";

	public GlyphBonusSellingRatioEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
