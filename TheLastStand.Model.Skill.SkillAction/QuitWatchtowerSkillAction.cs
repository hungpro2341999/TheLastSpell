using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillAction;

namespace TheLastStand.Model.Skill.SkillAction;

public class QuitWatchtowerSkillAction : SkillAction
{
	public override string EstimationIconId => "QuitWatchtower";

	public QuitWatchtowerSkillAction(SkillActionDefinition definition, SkillActionController controller, Skill skill)
		: base(definition, controller, skill)
	{
	}
}
