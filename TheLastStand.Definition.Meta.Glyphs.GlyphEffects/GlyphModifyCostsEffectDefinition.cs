using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyCostsEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "ModifyCosts";

	public ResourceManager.E_PriceModifierType Type { get; private set; }

	public GlyphModifyCostsEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("Type");
		GlyphDefinition.AssertIsTrue(xAttribute != null, "Type attribute is missing in ModifyCosts");
		GlyphDefinition.AssertIsTrue(Enum.TryParse<ResourceManager.E_PriceModifierType>(xAttribute.Value.Replace(base.TokenVariables), out var result), "Could not parse Type attribute into an E_PriceModifierType in ModifyCosts");
		Type = result;
	}
}
