using System.Collections;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class PlayPillarsCutsceneController : CutsceneController
{
	public PlayPillarsCutsceneDefinition PlayPillarsCutsceneDefinition => base.CutsceneDefinition as PlayPillarsCutsceneDefinition;

	public PlayPillarsCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		TPSingleton<CutsceneManager>.Instance.PillarsCutsceneView.PlayPillarsCutsceneDefinition = PlayPillarsCutsceneDefinition;
		yield return TPSingleton<CutsceneManager>.Instance.StartCoroutine(TPSingleton<CutsceneManager>.Instance.PillarsCutsceneView.PlayCutscene());
	}
}
