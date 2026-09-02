using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPostcondition;

public class TargetsCountCondition : GoalConditionDefinition
{
	public const string Name = "TargetsCount";

	public int Max { get; private set; }

	public int Min { get; private set; }

	public TargetsCountCondition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Min");
		if (xAttribute != null)
		{
			Min = int.Parse(xAttribute.Value);
		}
		XAttribute xAttribute2 = obj.Attribute("Max");
		if (xAttribute2 != null)
		{
			Max = int.Parse(xAttribute2.Value);
		}
	}
}
