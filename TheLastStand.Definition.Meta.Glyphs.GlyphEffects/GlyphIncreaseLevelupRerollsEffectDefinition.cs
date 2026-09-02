using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphIncreaseLevelupRerollsEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "IncreaseLevelupRerolls";

	public Node RerollsBonusExpression { get; private set; }

	public GlyphIncreaseLevelupRerollsEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in GlyphIncreaseStartingResourcesEffectDefinition.");
		XAttribute xAttribute = obj.Attribute("Value");
		GlyphDefinition.AssertIsTrue(xAttribute != null, "Value attribute is missing in IncreaseLevelupRerolls.");
		RerollsBonusExpression = Parser.Parse(xAttribute.Value, base.TokenVariables);
	}
}
