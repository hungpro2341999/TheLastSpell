using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;

namespace TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;

public class QuitWatchtowerSkillActionExecutionController : SkillActionExecutionController
{
	public QuitWatchtowerSkillActionExecution QuitWatchtowerSkillActionExecution => base.SkillActionExecution as QuitWatchtowerSkillActionExecution;

	public QuitWatchtowerSkillActionExecutionController(TheLastStand.Model.Skill.Skill skill)
		: base(skill)
	{
		QuitWatchtowerSkillActionExecutionView quitWatchtowerSkillActionExecutionView = new QuitWatchtowerSkillActionExecutionView();
		base.SkillActionExecution = new QuitWatchtowerSkillActionExecution(this, quitWatchtowerSkillActionExecutionView, skill);
		quitWatchtowerSkillActionExecutionView.SkillExecution = base.SkillActionExecution;
	}
}
