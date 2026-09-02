using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;

public class BossPhaseTurnConditionDefinition : TheLastStand.Framework.Serialization.Definition, IBossPhaseConditionDefinition
{
	public Node Expression { get; private set; }

	public BossPhaseTurnConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		Expression = Parser.Parse(xElement.Value);
	}
}
