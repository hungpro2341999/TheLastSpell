using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphMaterialScavengingPercentageModifierEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "MaterialScavengingPercentageModifier";

	public GlyphMaterialScavengingPercentageModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
