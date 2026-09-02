using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillAction;

namespace TheLastStand.Model.Skill.SkillAction;

public class GoIntoWatchtowerSkillAction : SkillAction
{
	public override string EstimationIconId => "GoIntoWatchtower";

	public GoIntoWatchtowerSkillAction(SkillActionDefinition definition, SkillActionController controller, Skill skill)
		: base(definition, controller, skill)
	{
	}
}
