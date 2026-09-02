using System.Collections;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class StopMusicCutsceneController : CutsceneController
{
	public StopMusicCutsceneDefinition StopMusicCutsceneDefinition => base.CutsceneDefinition as StopMusicCutsceneDefinition;

	public StopMusicCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (!StopMusicCutsceneDefinition.OnlyIfVictoryTriggered || TPSingleton<BossManager>.Instance.ShouldTriggerBossWaveVictory)
		{
			TPSingleton<SoundManager>.Instance.StopMusic();
		}
		yield break;
	}
}
