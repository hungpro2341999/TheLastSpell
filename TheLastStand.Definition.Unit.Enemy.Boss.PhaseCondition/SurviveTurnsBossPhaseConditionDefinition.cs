using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;

public class SurviveTurnsBossPhaseConditionDefinition : TheLastStand.Framework.Serialization.Definition, IBossPhaseConditionDefinition
{
	public Node Expression { get; private set; }

	public SurviveTurnsBossPhaseConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		Expression = Parser.Parse(xAttribute.Value);
	}
}
