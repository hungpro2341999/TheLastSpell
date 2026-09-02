using System.Collections;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class PlaySoundCutsceneController : CutsceneController
{
	public PlaySoundCutsceneDefinition PlaySoundCutsceneDefinition => base.CutsceneDefinition as PlaySoundCutsceneDefinition;

	public PlaySoundCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		AudioClip audioClip = ResourcePooler.LoadOnce<AudioClip>(PlaySoundCutsceneDefinition.AudioClipPath ?? "");
		SoundManager.PlayAudioClip(TPSingleton<CutsceneManager>.Instance.VictorySequenceView.AudioSource, audioClip, PlaySoundCutsceneDefinition.Delay);
		yield break;
	}
}
