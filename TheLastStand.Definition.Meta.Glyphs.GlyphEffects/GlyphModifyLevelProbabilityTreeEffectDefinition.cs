using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyLevelProbabilityTreeEffectDefinition : GlyphModifyRarityProbabilityTreeEffectDefinition
{
	public new const string Name = "ModifyLevelProbabilityTree";

	public GlyphModifyLevelProbabilityTreeEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
