using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillAction;

public class ResupplySkillActionDefinition : SkillActionDefinition
{
	public const string Name = "Resupply";

	public ResupplySkillActionDefinition(XContainer container)
		: base(container)
	{
	}
}
