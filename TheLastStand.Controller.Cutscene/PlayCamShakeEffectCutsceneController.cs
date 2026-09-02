using System.Collections;
using TPLib;
using TPLib.Yield;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class PlayCamShakeEffectCutsceneController : CutsceneController
{
	public PlayCamShakeEffectCutsceneDefinition PlayCamShakeEffectCutsceneDefinition => base.CutsceneDefinition as PlayCamShakeEffectCutsceneDefinition;

	public PlayCamShakeEffectCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public IEnumerator PlayCamShake()
	{
		float timeSpent = 0f;
		while (timeSpent <= PlayCamShakeEffectCutsceneDefinition.Duration)
		{
			ACameraView.Shake(PlayCamShakeEffectCutsceneDefinition.DurationOfEachShake, Vector2.one * PlayCamShakeEffectCutsceneDefinition.IntensityMultiplier * PlayCamShakeEffectCutsceneDefinition.DataAnimationCurve.Evaluate(timeSpent / PlayCamShakeEffectCutsceneDefinition.Duration), 10, 0.5f, 0f, Vector3.zero, 0.05f);
			timeSpent += PlayCamShakeEffectCutsceneDefinition.DelayBetweenEachShake;
			yield return SharedYields.WaitForSeconds(PlayCamShakeEffectCutsceneDefinition.DelayBetweenEachShake);
		}
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (PlayCamShakeEffectCutsceneDefinition.WaitCamShake)
		{
			yield return PlayCamShake();
		}
		else
		{
			TPSingleton<CutsceneManager>.Instance.StartCoroutine(PlayCamShake());
		}
	}
}
