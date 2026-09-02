using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;

namespace TheLastStand.Model.Skill.SkillAction.SkillActionExecution;

public class ResupplySkillActionExecution : SkillActionExecution
{
	public ResupplySkillActionExecutionController ResupplySkillActionExecutionController => base.SkillExecutionController as ResupplySkillActionExecutionController;

	public ResupplySkillActionExecutionView ResupplySkillActionExecutionView => base.SkillExecutionView as ResupplySkillActionExecutionView;

	public ResupplySkillActionDefinition ResupplySkillActionDefinition { get; set; }

	public ResupplySkillActionExecution(SkillActionExecutionController skillExecutionController, SkillActionExecutionView skillExecutionView, Skill skill)
		: base(skillExecutionController, skillExecutionView, skill)
	{
	}
}
