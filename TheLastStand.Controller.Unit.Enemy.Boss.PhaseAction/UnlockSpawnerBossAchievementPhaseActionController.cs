using System.Collections;
using TPLib;
using TheLastStand.DRM.Achievements;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Manager.Achievements;
using TheLastStand.Model.Unit.Boss;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class UnlockSpawnerBossAchievementPhaseActionController : ABossPhaseActionController
{
	public UnlockSpawnerBossAchievementPhaseActionController(UnlockSpawnerBossAchievementPhaseActionDefinition unlockSpawnerBossAchievementPhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(unlockSpawnerBossAchievementPhaseActionDefinition, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_KILL_TWO_THIRD_DRYADS);
		yield break;
	}
}
