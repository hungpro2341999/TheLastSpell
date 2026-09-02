using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

public class NotInBuildingConditionDefinition : SkillConditionDefinition
{
	public const string NotInBuildingName = "NotInBuilding";

	public override string Name => "NotInBuilding";

	public NotInBuildingConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
