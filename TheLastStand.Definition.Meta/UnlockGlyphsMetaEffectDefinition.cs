using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockGlyphsMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockGlyphs";

	public List<string> GlyphIds { get; private set; }

	public UnlockGlyphsMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		GlyphIds = new List<string>();
		foreach (XElement item in obj.Elements("GlyphId"))
		{
			GlyphIds.Add(item.Value);
		}
	}
}
