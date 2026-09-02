using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;

namespace TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;

public class SkipTurnSkillActionExecutionController : SkillActionExecutionController
{
	public SkipTurnSkillActionExecution SkipTurnSkillActionExecution => base.SkillActionExecution as SkipTurnSkillActionExecution;

	public SkipTurnSkillActionExecutionController(TheLastStand.Model.Skill.Skill skill)
		: base(skill)
	{
		SkipTurnSkillActionExecutionView skipTurnSkillActionExecutionView = new SkipTurnSkillActionExecutionView();
		base.SkillActionExecution = new SkipTurnSkillActionExecution(this, skipTurnSkillActionExecutionView, skill);
		skipTurnSkillActionExecutionView.SkillExecution = base.SkillActionExecution;
	}
}
