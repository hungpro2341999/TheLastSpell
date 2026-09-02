using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Perk.PerkCondition;

public class IsTrueConditionDefinition : APerkConditionDefinition
{
	public static class Constants
	{
		public const string Id = "IsTrue";
	}

	public Node ValueExpression { get; private set; }

	public IsTrueConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		ValueExpression = Parser.Parse(xAttribute.Value, base.TokenVariables);
	}
}
