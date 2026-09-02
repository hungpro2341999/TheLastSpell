using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public class LogPerkActionDefinition : APerkActionDefinition
{
	public static class Constants
	{
		public const string Id = "Log";
	}

	public string ExpressionString { get; private set; }

	public string FormattingString { get; private set; }

	public Node ValueExpression { get; private set; }

	public LogPerkActionDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Value");
		ExpressionString = xAttribute.Value;
		ValueExpression = Parser.Parse(xAttribute.Value, base.TokenVariables);
		XAttribute xAttribute2 = obj.Attribute("Formatting");
		if (xAttribute2 != null)
		{
			FormattingString = xAttribute2.Value;
		}
	}
}
