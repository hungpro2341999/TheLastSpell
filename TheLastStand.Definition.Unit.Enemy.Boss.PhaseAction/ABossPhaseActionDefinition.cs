using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public abstract class ABossPhaseActionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Node DelayExpression { get; private set; }

	protected ABossPhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Delay");
		if (!string.IsNullOrEmpty(xAttribute?.Value))
		{
			DelayExpression = Parser.Parse(xAttribute.Value);
		}
	}
}
