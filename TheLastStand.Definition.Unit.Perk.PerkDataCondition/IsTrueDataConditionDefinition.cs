using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Perk.PerkDataCondition;

public class IsTrueDataConditionDefinition : APerkDataConditionDefinition
{
	public static class Constants
	{
		public const string Id = "IsTrue";
	}

	public Node ValueExpression { get; private set; }

	public IsTrueDataConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		ValueExpression = Parser.Parse(xAttribute.Value, base.TokenVariables);
	}
}
