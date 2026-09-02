using DG.Tweening;
using UnityEngine;

namespace TheLastStand.View.WorldMap;

public class WorldMapAmbientSound : MonoBehaviour
{
	[SerializeField]
	private AudioSource ambientAudioSource;

	[SerializeField]
	[Range(0f, 1f)]
	private float targetVolume = 1f;

	[SerializeField]
	private bool toggleFade = true;

	[SerializeField]
	[Min(0f)]
	private float fadeInDuration = 1f;

	[SerializeField]
	private AnimationCurve fadeInEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	[Min(0f)]
	private float fadeOutDuration = 1f;

	[SerializeField]
	private AnimationCurve fadeOutEase = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	private Tween fadeTween;

	public void FadeIn()
	{
		if (!ambientAudioSource.isPlaying)
		{
			ambientAudioSource.Play();
		}
		if (toggleFade)
		{
			fadeTween?.Kill();
			fadeTween = ambientAudioSource.DOFade(targetVolume, fadeInDuration).SetEase(fadeInEase);
			fadeTween.Play();
		}
		else
		{
			ambientAudioSource.volume = 1f;
		}
	}

	public void FadeOut(float overrideDuration = -1f)
	{
		if (toggleFade)
		{
			fadeTween?.Kill();
			fadeTween = ambientAudioSource.DOFade(0f, (overrideDuration > -1f) ? overrideDuration : fadeOutDuration).SetEase(fadeOutEase);
			fadeTween.Play();
		}
		else
		{
			ambientAudioSource.volume = 0f;
		}
	}

	private void Awake()
	{
		ambientAudioSource.volume = 0f;
	}
}
