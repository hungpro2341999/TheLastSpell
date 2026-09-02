using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace TheLastStand.View.Camera;

public class CameraVisualEffects : MonoBehaviour
{
	[SerializeField]
	private AmplifyColorEffect invertColor;

	[SerializeField]
	private AmplifyColorEffect redFlashColor;

	[SerializeField]
	private float redFlashInitialBlend = 0.6f;

	[SerializeField]
	private float redFlashDuration = 0.5f;

	[SerializeField]
	private Ease redFlashOutEase = Ease.InQuart;

	[SerializeField]
	private PostProcessVolume pillarChromaticAberrationVolume;

	private Tween invertTween;

	private Tween redFlashTween;

	public void InvertImage(float targetValue = 1f)
	{
		SetBlendAmount(invertColor, targetValue);
	}

	public void ResetInvertEffect()
	{
		SetBlendAmount(invertColor, 0f);
	}

	public void ToggleChromaticAberration(bool state)
	{
		pillarChromaticAberrationVolume.enabled = state;
	}

	public IEnumerator InvertImageCoroutine(float duration, Ease ease, float targetValue = 1f)
	{
		invertTween?.Kill();
		invertTween = DOTween.To(() => invertColor.BlendAmount, delegate(float x)
		{
			SetBlendAmount(invertColor, x);
		}, targetValue, duration).SetEase(ease);
		yield return invertTween.WaitForCompletion();
	}

	public IEnumerator RedFlashCoroutine(float duration = -1f, Ease ease = Ease.Unset)
	{
		redFlashColor.BlendAmount = redFlashInitialBlend;
		redFlashTween?.Kill();
		if (duration == -1f)
		{
			duration = redFlashDuration;
		}
		if (ease == Ease.Unset)
		{
			ease = redFlashOutEase;
		}
		redFlashTween = DOTween.To(() => redFlashColor.BlendAmount, delegate(float x)
		{
			SetBlendAmount(redFlashColor, x);
		}, 0f, duration).SetEase(ease);
		yield return redFlashTween.WaitForCompletion();
	}

	public IEnumerator ResetInvertCoroutine(float duration, Ease ease)
	{
		invertTween?.Kill();
		invertTween = DOTween.To(() => invertColor.BlendAmount, delegate(float x)
		{
			SetBlendAmount(invertColor, x);
		}, 0f, duration).SetEase(ease);
		yield return invertTween.WaitForCompletion();
	}

	private void SetBlendAmount(AmplifyColorEffect colorEffect, float blendAmount)
	{
		colorEffect.BlendAmount = blendAmount;
	}
}
