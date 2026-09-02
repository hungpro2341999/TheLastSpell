using System.Collections;
using TPLib;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit.Boss;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class SetPhasePhaseActionController : ABossPhaseActionController
{
	public SetPhasePhaseActionDefinition SetPhasePhaseActionDefinition => base.ABossPhaseActionDefinition as SetPhasePhaseActionDefinition;

	public SetPhasePhaseActionController(SetPhasePhaseActionDefinition setPhasePhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(setPhasePhaseActionDefinition, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		TPSingleton<BossManager>.Instance.NextBossPhaseId = SetPhasePhaseActionDefinition.PhaseId;
		TPSingleton<BossManager>.Instance.NextBossPhaseDelay = base.Delay;
		yield break;
	}
}
