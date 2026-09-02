using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using UnityEngine;

namespace TheLastStand.View.Cutscene;

public class PillarsAnimatorEventReceiver : MonoBehaviour
{
	[SerializeField]
	private AudioClip[] pillarAppearSfx;

	[SerializeField]
	private AudioClip[] sunAppearSfx;

	[SerializeField]
	private AudioClip[] fadeOutSfx;

	public void OnAppendNextText()
	{
		TPSingleton<CutsceneManager>.Instance.PillarsCutsceneView.AppendNextText();
	}

	public void OnClearText()
	{
		TPSingleton<CutsceneManager>.Instance.PillarsCutsceneView.ClearText();
	}

	public void OnFadeOutStart()
	{
		PlaySounds(fadeOutSfx);
	}

	public void OnPillarAppear()
	{
		PlaySounds(pillarAppearSfx);
	}

	public void OnSunAppear()
	{
		PlaySounds(sunAppearSfx);
	}

	private void PlaySounds(AudioClip[] clips)
	{
		for (int i = 0; i < clips.Length; i++)
		{
			if (!(clips[i] == null))
			{
				SoundManager.PlayAudioClip(TPSingleton<CutsceneManager>.Instance.VictorySequenceView.AudioSource, clips[i], 0f, doNotInterrupt: true);
			}
		}
	}
}
