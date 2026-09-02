using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphIncreaseBuildingHealthEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "IncreaseBuildingHealth";

	public string IdList { get; private set; }

	public GlyphIncreaseBuildingHealthEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		base.Deserialize(container);
		XAttribute xAttribute = obj.Attribute("IdList");
		IdList = xAttribute.Value.Replace(base.TokenVariables);
	}
}
