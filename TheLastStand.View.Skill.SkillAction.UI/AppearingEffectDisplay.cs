using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class AppearingEffectDisplay : EffectDisplay
{
	[SerializeField]
	protected CanvasGroup fadeTarget;

	[SerializeField]
	protected float fadeInDuration = 0.3f;

	[SerializeField]
	protected float fadeOutDuration = 0.1f;

	[SerializeField]
	protected Transform translateTarget;

	[SerializeField]
	protected float translateDuration = 0.4f;

	[SerializeField]
	protected float translateOffsetY = 50f;

	[SerializeField]
	protected Ease translateEasing = Ease.OutQuart;

	protected float internalTranslateOffsetY;

	protected override float DisplayDuration => Mathf.Max(base.DisplayDuration, fadeInDuration, translateDuration);

	protected override IEnumerator DisplayCoroutine()
	{
		if (translateDuration < 0f)
		{
			translateDuration = displayDuration;
		}
		if (fadeInDuration < 0f)
		{
			fadeInDuration = displayDuration;
		}
		if (fadeInDuration > 0f && fadeTarget != null)
		{
			fadeTarget.DOFade(0f, fadeInDuration).From();
		}
		if (translateDuration > 0f && translateTarget != null)
		{
			translateTarget.DOLocalMoveY(0f - internalTranslateOffsetY, translateDuration).From(isRelative: true).SetEase(translateEasing);
		}
		yield return base.DisplayCoroutine();
		if (fadeOutDuration > 0f && fadeTarget != null)
		{
			yield return fadeTarget.DOFade(0f, fadeOutDuration).WaitForCompletion();
		}
	}

	public virtual void Init()
	{
		internalTranslateOffsetY = translateOffsetY;
	}

	public virtual void Init(int value)
	{
		Init();
	}
}
