using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;

namespace TheLastStand.Model.Skill.SkillAction.SkillActionExecution;

public class SkipTurnSkillActionExecution : SkillActionExecution
{
	public SkipTurnSkillActionExecution(SkillActionExecutionController skillExecutionController, SkillActionExecutionView skillExecutionView, Skill skill)
		: base(skillExecutionController, skillExecutionView, skill)
	{
	}
}
