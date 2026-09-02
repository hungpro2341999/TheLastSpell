using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;

namespace TheLastStand.Model.Unit.Pathfinding;

public class AIPathfindingData
{
	public GoalDefinition.E_InterruptionCondition InterruptionCondition { get; set; }

	public SkillActionExecution SkillExecution { get; set; }

	public float SpreadFactor { get; set; }

	public int ThinkingScope { get; set; }
}
