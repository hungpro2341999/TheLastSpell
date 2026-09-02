using System.Collections;
using TPLib.Yield;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class FadeOutCutsceneController : CutsceneController
{
	public FadeOutCutsceneDefinition FadeOutCutsceneDefinition => base.CutsceneDefinition as FadeOutCutsceneDefinition;

	public FadeOutCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		CanvasFadeManager.FadeOut(FadeOutCutsceneDefinition.Color, FadeOutCutsceneDefinition.Duration);
		if (FadeOutCutsceneDefinition.WaitDuration)
		{
			yield return SharedYields.WaitForSeconds(Mathf.Max(FadeOutCutsceneDefinition.Duration - 0.5f, 0f));
		}
	}
}
