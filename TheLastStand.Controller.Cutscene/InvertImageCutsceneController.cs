using System.Collections;
using TheLastStand.Definition.Cutscene;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class InvertImageCutsceneController : CutsceneController
{
	public InvertImageCutsceneDefinition InvertImageCutsceneDefinition => base.CutsceneDefinition as InvertImageCutsceneDefinition;

	public InvertImageCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		yield return CameraView.CameraVisualEffects.InvertImageCoroutine(InvertImageCutsceneDefinition.Duration, InvertImageCutsceneDefinition.Easing);
		CameraView.CameraVisualEffects.StartCoroutine(CameraView.CameraVisualEffects.ResetInvertCoroutine(InvertImageCutsceneDefinition.Duration, InvertImageCutsceneDefinition.Easing));
	}
}
