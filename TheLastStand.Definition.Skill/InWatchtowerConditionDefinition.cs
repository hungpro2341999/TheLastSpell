using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

public class InWatchtowerConditionDefinition : SkillConditionDefinition
{
	public const string InWatchtowerName = "InWatchtower";

	public override string Name => "InWatchtower";

	public InWatchtowerConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
