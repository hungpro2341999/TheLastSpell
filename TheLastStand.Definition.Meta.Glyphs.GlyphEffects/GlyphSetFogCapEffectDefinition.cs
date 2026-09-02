using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphSetFogCapEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "SetFogCap";

	public string IndexName { get; private set; }

	public GlyphSetFogCapEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Index");
		IndexName = xAttribute.Value.Replace(base.TokenVariables);
	}
}
