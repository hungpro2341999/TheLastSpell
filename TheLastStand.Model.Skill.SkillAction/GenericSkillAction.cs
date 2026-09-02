using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillAction;

namespace TheLastStand.Model.Skill.SkillAction;

public class GenericSkillAction : SkillAction
{
	public GenericSkillActionController GenericSkillActionController => base.SkillActionController as GenericSkillActionController;

	public GenericSkillActionDefinition GenericSkillActionDefinition => base.SkillActionDefinition as GenericSkillActionDefinition;

	public override string EstimationIconId => "Generic";

	public GenericSkillAction(SkillActionDefinition definition, SkillActionController controller, Skill skill)
		: base(definition, controller, skill)
	{
	}
}
