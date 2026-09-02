using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class TargetIsNotEliteConditionDefinition : GoalConditionDefinition
{
	public const string Name = "TargetIsNotElite";

	public TargetIsNotEliteConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
