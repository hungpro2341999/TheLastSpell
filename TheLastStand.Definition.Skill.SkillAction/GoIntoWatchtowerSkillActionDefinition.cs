using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillAction;

public class GoIntoWatchtowerSkillActionDefinition : SkillActionDefinition
{
	public const string Name = "GoIntoWatchtower";

	public GoIntoWatchtowerSkillActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
	}
}
