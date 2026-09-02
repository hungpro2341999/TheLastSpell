using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphNativePerkPointsBonusEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "NativePerkPointsBonus";

	public GlyphNativePerkPointsBonusEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
