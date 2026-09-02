using System.Collections;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager.Unit;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class OverrideCurrentSpawnWaveCutsceneController : CutsceneController
{
	public OverrideCurrentSpawnWaveCutsceneDefinition OverrideCurrentSpawnWaveCutsceneDefinition => base.CutsceneDefinition as OverrideCurrentSpawnWaveCutsceneDefinition;

	public OverrideCurrentSpawnWaveCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		SpawnWaveManager.OverrideCurrentSpawnWave(OverrideCurrentSpawnWaveCutsceneDefinition.WaveId, OverrideCurrentSpawnWaveCutsceneDefinition.DirectionsId);
		yield break;
	}
}
