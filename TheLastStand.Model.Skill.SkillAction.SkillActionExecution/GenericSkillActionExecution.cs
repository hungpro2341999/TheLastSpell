using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;

namespace TheLastStand.Model.Skill.SkillAction.SkillActionExecution;

public class GenericSkillActionExecution : SkillActionExecution
{
	public GenericSkillActionExecution(GenericSkillActionExecutionController skillExecutionController, SkillActionExecutionView skillExecutionView, Skill skill)
		: base(skillExecutionController, skillExecutionView, skill)
	{
	}
}
