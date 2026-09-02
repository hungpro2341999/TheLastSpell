using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphIncreaseDefensesDamagesEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "IncreaseDefensesDamages";

	public GlyphIncreaseDefensesDamagesEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
