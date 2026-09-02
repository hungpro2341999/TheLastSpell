using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class NotInFogCondition : GoalConditionDefinition
{
	public const string Name = "NotInFog";

	public Node NbTurns { get; private set; }

	public NotInFogCondition(XContainer container)
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
