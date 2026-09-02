using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillAction;

public class SkipTurnSkillActionDefinition : SkillActionDefinition
{
	public const string Name = "SkipTurn";

	public SkipTurnSkillActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
	}
}
