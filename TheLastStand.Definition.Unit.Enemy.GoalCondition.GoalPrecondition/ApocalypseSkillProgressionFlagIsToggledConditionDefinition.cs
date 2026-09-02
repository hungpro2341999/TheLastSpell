using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class ApocalypseSkillProgressionFlagIsToggledConditionDefinition : GoalConditionDefinition
{
	public const string Name = "ApocalypseSkillProgressionFlagIsToggled";

	public string SkillProgressionFlag { get; private set; }

	public ApocalypseSkillProgressionFlagIsToggledConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Flag");
		SkillProgressionFlag = xAttribute.Value;
	}
}
