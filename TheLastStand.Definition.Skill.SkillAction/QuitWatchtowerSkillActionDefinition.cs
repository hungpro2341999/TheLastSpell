using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillAction;

public class QuitWatchtowerSkillActionDefinition : SkillActionDefinition
{
	public const string Name = "QuitWatchtower";

	public QuitWatchtowerSkillActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
	}
}
