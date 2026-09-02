using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphIncreaseStartingResourcesEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "IncreaseStartingResources";

	public Node GoldBonusExpression { get; private set; }

	public Node MaterialsBonusExpression { get; private set; }

	public GlyphIncreaseStartingResourcesEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in GlyphIncreaseStartingResourcesEffectDefinition.");
		XElement xElement = obj.Element("GoldBonus");
		GoldBonusExpression = ((xElement != null) ? Parser.Parse(xElement.Value, base.TokenVariables) : null);
		XElement xElement2 = obj.Element("MaterialsBonus");
		MaterialsBonusExpression = ((xElement2 != null) ? Parser.Parse(xElement2.Value, base.TokenVariables) : null);
	}
}
