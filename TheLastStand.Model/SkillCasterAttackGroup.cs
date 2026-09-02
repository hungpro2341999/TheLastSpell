using System.Collections.Generic;

namespace TheLastStand.Model;

public class SkillCasterAttackGroup
{
	public enum E_Target : byte
	{
		MAGIC_CIRCLE,
		ATTACK_HERO,
		ATTACK_BUILDING,
		ATTACK_ENEMY,
		ATTACK_EMPTY,
		GENERIC,
		SPAWN,
		OTHER
	}

	public readonly string SkillSoundId;

	public readonly List<ComputedGoal> GoalsToExecute = new List<ComputedGoal>();

	public SkillCasterAttackGroup(ComputedGoal computedGoal)
	{
		SkillSoundId = computedGoal.Goal.Skill.SkillDefinition.SoundId;
		GoalsToExecute.Add(computedGoal);
	}
}
