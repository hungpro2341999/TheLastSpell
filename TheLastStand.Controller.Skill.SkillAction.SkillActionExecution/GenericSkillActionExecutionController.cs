using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;

namespace TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;

public class GenericSkillActionExecutionController : SkillActionExecutionController
{
	public GenericSkillActionExecution GenericSkillActionExecution => base.SkillActionExecution as GenericSkillActionExecution;

	public GenericSkillActionExecutionController(TheLastStand.Model.Skill.Skill skill)
		: base(skill)
	{
		GenericSkillActionExecutionView genericSkillActionExecutionView = new GenericSkillActionExecutionView();
		base.SkillActionExecution = new GenericSkillActionExecution(this, genericSkillActionExecutionView, skill);
		genericSkillActionExecutionView.SkillExecution = base.SkillActionExecution;
	}
}
