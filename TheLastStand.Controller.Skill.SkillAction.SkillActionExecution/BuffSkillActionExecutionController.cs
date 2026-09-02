using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;

namespace TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;

public class BuffSkillActionExecutionController : SkillActionExecutionController
{
	public BuffSkillActionExecution BuffSkillActionExecution => base.SkillActionExecution as BuffSkillActionExecution;

	public BuffSkillActionExecutionController(TheLastStand.Model.Skill.Skill skill)
		: base(skill)
	{
		BuffSkillActionExecutionView buffSkillActionExecutionView = new BuffSkillActionExecutionView();
		base.SkillActionExecution = new BuffSkillActionExecution(this, buffSkillActionExecutionView, skill);
		buffSkillActionExecutionView.SkillExecution = base.SkillActionExecution;
	}
}
