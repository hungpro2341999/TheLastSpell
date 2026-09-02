using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class PlayableUnitCloseToCasterConditionDefinition : GoalConditionDefinition
{
	public const string Name = "PlayableUnitCloseToCaster";

	public PlayableUnitCloseToCasterConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
