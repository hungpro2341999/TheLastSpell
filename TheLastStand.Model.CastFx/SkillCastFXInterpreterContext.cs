using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;

namespace TheLastStand.Model.CastFx;

public class SkillCastFXInterpreterContext : CastFXInterpreterContext
{
	private SkillActionExecution skillExecution;

	private float NbPropagations
	{
		get
		{
			if (!(skillExecution.Skill.SkillAction is AttackSkillAction attackSkillAction) || attackSkillAction.SkillActionExecution.PropagationAffectedUnits == null)
			{
				return 0f;
			}
			if (attackSkillAction.SkillActionExecution.PropagationAffectedUnits.TryGetValue(attackSkillAction.SkillActionExecution.HitIndex, out var value))
			{
				return value.Count;
			}
			return 0f;
		}
	}

	public SkillCastFXInterpreterContext(object targetObject)
		: base(targetObject)
	{
		skillExecution = targetObject as SkillActionExecution;
	}
}
