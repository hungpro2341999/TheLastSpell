using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class TargetInRangeConditionDefinition : GoalConditionDefinition
{
	public const string Name = "TargetInRange";

	public Node MaxExpression { get; private set; }

	public Node MinExpression { get; private set; }

	public TargetInRangeConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public int EvalToInt(Node node, InterpreterContext interpreterContext = null)
	{
		if (interpreterContext == null)
		{
			return node?.EvalToInt() ?? 0;
		}
		return node?.EvalToInt(interpreterContext) ?? 0;
	}

	public int GetMinEvalToInt(InterpreterContext interpreterContext = null)
	{
		return EvalToInt(MinExpression, interpreterContext);
	}

	public int GetMaxEvalToInt(InterpreterContext interpreterContext = null)
	{
		return EvalToInt(MaxExpression, interpreterContext);
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Min");
		if (xAttribute != null)
		{
			MinExpression = Parser.Parse(xAttribute.Value);
		}
		XAttribute xAttribute2 = obj.Attribute("Max");
		if (xAttribute2 != null)
		{
			MaxExpression = Parser.Parse(xAttribute2.Value);
		}
	}
}
