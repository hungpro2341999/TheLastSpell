using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public abstract class InstantiateEffectDisplayDefinition : APerkActionDefinition
{
	public Node ValueExpression { get; private set; }

	public InstantiateEffectDisplayDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		ValueExpression = Parser.Parse(xAttribute.Value, base.TokenVariables);
	}
}
