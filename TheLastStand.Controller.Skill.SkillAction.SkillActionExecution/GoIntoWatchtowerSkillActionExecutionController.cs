using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;

namespace TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;

public class GoIntoWatchtowerSkillActionExecutionController : SkillActionExecutionController
{
	public GoIntoWatchtowerSkillActionExecution GoIntoWatchtowerSkillActionExecution => base.SkillActionExecution as GoIntoWatchtowerSkillActionExecution;

	public GoIntoWatchtowerSkillActionExecutionController(TheLastStand.Model.Skill.Skill skill)
		: base(skill)
	{
		GoIntoWatchtowerSkillActionExecutionView goIntoWatchtowerSkillActionExecutionView = new GoIntoWatchtowerSkillActionExecutionView();
		base.SkillActionExecution = new GoIntoWatchtowerSkillActionExecution(this, goIntoWatchtowerSkillActionExecutionView, skill);
		goIntoWatchtowerSkillActionExecutionView.SkillExecution = base.SkillActionExecution;
	}
}
