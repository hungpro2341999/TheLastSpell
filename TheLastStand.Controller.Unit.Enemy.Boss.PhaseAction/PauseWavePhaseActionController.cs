using System.Collections;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit.Boss;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class PauseWavePhaseActionController : ABossPhaseActionController
{
	public PauseWavePhaseActionDefinition PauseWavePausePhaseActionDefinition => base.ABossPhaseActionDefinition as PauseWavePhaseActionDefinition;

	public PauseWavePhaseActionController(PauseWavePhaseActionDefinition pauseWavePhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(pauseWavePhaseActionDefinition, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		SpawnWaveManager.CurrentSpawnWave.IsPaused = PauseWavePausePhaseActionDefinition.PauseState;
		yield break;
	}
}
