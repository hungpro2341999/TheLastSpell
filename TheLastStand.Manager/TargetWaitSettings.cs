using System;
using TheLastStand.Model;

namespace TheLastStand.Manager;

[Serializable]
public struct TargetWaitSettings
{
	public SkillCasterAttackGroup.E_Target TargetType;

	public float Duration;

	public TargetWaitSettings(SkillCasterAttackGroup.E_Target targetType, float duration)
	{
		TargetType = targetType;
		Duration = duration;
	}
}
