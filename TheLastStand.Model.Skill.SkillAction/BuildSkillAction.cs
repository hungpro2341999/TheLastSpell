using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillAction;

namespace TheLastStand.Model.Skill.SkillAction;

public class BuildSkillAction : SkillAction
{
	public override string EstimationIconId => "Build";

	public BuildSkillActionDefinition BuildSkillActionDefinition => base.SkillActionDefinition as BuildSkillActionDefinition;

	public BuildSkillAction(SkillActionDefinition definition, SkillActionController controller, Skill skill)
		: base(definition, controller, skill)
	{
	}
}
