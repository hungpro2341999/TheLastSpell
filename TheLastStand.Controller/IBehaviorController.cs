using System.Collections.Generic;
using TheLastStand.Model;
using TheLastStand.Model.Skill;

namespace TheLastStand.Controller;

public interface IBehaviorController
{
	void ClearCurrentGoal();

	void ComputeCurrentGoals(Dictionary<IDamageable, GroupTargetingInfo> alreadyTargetedTiles = null);

	void ExecuteAllGoals();

	void ExecuteDeathRattle();

	bool ExecuteGoal(ComputedGoal computedGoal);

	void PrepareForDeathRattle();
}
