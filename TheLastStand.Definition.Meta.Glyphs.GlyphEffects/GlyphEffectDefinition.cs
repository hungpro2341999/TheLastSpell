using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public abstract class GlyphEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	protected GlyphEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
