using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyBuildLimitEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "ModifyBuildLimit";

	public string BuildLimitId { get; private set; }

	public GlyphModifyBuildLimitEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("BuildLimitId");
		GlyphDefinition.AssertIsTrue(xAttribute != null, "BuildLimitId attribute is missing in ModifyBuildLimit");
		BuildLimitId = xAttribute.Value.Replace(base.TokenVariables);
	}
}
