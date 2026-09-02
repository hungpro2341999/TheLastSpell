using System.Collections;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class PlayRippleEffectCutsceneController : CutsceneController
{
	public PlayRippleEffectCutsceneDefinition PlayRippleEffectCutsceneDefinition => base.CutsceneDefinition as PlayRippleEffectCutsceneDefinition;

	public PlayRippleEffectCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (!cutsceneData.Position.HasValue)
		{
			TPSingleton<CutsceneManager>.Instance.LogError("Tried to play a PlayRippleEffectCutsceneController but position is null.");
		}
		else
		{
			CameraView.RippleEffect.RippleAtWorldPosition(cutsceneData.Position.Value);
		}
		yield break;
	}
}
