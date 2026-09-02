using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class ModifyDefensesDamageEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ModifyDefensesDamage";
	}

	public Node PercentageExpression { get; private set; }

	public Node RangeExpression { get; private set; }

	public ModifyDefensesDamageEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Percentage");
		PercentageExpression = Parser.Parse(xAttribute.Value, base.TokenVariables);
		XAttribute xAttribute2 = obj.Attribute("Range");
		RangeExpression = Parser.Parse(xAttribute2.Value, base.TokenVariables);
	}
}
