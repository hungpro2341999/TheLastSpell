using System.Collections.Generic;

namespace TheLastStand.Model.Skill;

public class GroupTargetingInfo
{
	public float MinDamage { get; set; }

	public float MaxDamage { get; set; }

	public List<int> EntitiesIdTargeting { get; private set; }

	public GroupTargetingInfo(float minDamages, float maxDamages, int entityId)
	{
		MinDamage = minDamages;
		MaxDamage = maxDamages;
		EntitiesIdTargeting = new List<int> { entityId };
	}
}
