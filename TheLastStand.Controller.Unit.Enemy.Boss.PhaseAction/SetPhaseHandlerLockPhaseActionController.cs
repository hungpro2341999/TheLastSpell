using System.Collections;
using TPLib;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit.Boss;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class SetPhaseHandlerLockPhaseActionController : ABossPhaseActionController
{
	public SetPhaseHandlerLockPhaseActionDefinition SetPhaseHandlerLockPhaseActionDefinition => base.ABossPhaseActionDefinition as SetPhaseHandlerLockPhaseActionDefinition;

	public SetPhaseHandlerLockPhaseActionController(SetPhaseHandlerLockPhaseActionDefinition setPhaseHandlerLockPhaseAction, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(setPhaseHandlerLockPhaseAction, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		foreach (BossPhaseHandler value in TPSingleton<BossManager>.Instance.CurrentBossPhase.BossPhaseHandlers.Values)
		{
			if (value.BossPhaseHandlerDefinition.Id == SetPhaseHandlerLockPhaseActionDefinition.HandlerId)
			{
				value.IsLocked = SetPhaseHandlerLockPhaseActionDefinition.LockValue;
			}
		}
		yield break;
	}
}
