using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillAction;

namespace TheLastStand.Model.Skill.SkillAction;

public class SkipTurnSkillAction : SkillAction
{
	public override string EstimationIconId => "SkipTurn";

	public SkipTurnSkillAction(SkillActionDefinition definition, SkillActionController controller, Skill skill)
		: base(definition, controller, skill)
	{
	}
}
