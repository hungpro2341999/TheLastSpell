using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition;

public abstract class GoalConditionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public GoalConditionDefinition(XContainer container)
		: base(container)
	{
	}
}
