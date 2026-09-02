using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class NotInAnyFogCondition : GoalConditionDefinition
{
	public const string Name = "NotInAnyFog";

	public Node NbTurns { get; private set; }

	public NotInAnyFogCondition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("NbTurns");
		if (xAttribute != null)
		{
			NbTurns = Parser.Parse(xAttribute.Value);
		}
	}
}
