using System.Collections;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class PlaySealAnticipationCutsceneController : CutsceneController
{
	public PlaySealAnticipationCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		BuildingManager.MagicCircle.MagicCircleView.PlaySealAnticipationAnimation();
		ObjectPooler.GetPooledComponent("SFXVictory", BuildingManager.MagicCircle.MagicCircleView.SFXPrefab).Play(GameManager.WinAudioClip, TPSingleton<CutsceneManager>.Instance.VictorySequenceView.VictorySfxDelay);
		yield break;
	}
}
