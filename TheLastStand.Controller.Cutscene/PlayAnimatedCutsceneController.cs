using System.Collections;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class PlayAnimatedCutsceneController : CutsceneController
{
	public PlayAnimatedCutsceneDefinition PlayAnimatedCutsceneDefinition => base.CutsceneDefinition as PlayAnimatedCutsceneDefinition;

	public PlayAnimatedCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		yield return AnimatedCutsceneManager.PlayAnimatedCutsceneWithFade(PlayAnimatedCutsceneDefinition.CutsceneId);
	}
}
