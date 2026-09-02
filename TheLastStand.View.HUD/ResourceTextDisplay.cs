using System;
using DG.Tweening;
using TMPro;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;

namespace TheLastStand.View.HUD;

public class ResourceTextDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Ease textEasing = Ease.Linear;

	[SerializeField]
	private float textTweenDuration = 0.5f;

	[SerializeField]
	private float pulseScale = 1.2f;

	[SerializeField]
	private float pulseDuration = 0.5f;

	[SerializeField]
	private int periodCount = 2;

	private int previousValue;

	private int textValue;

	private Tween textTween;

	private Tween pulseTween;

	public void RefreshValue<T>(int value, Func<T> effectDisplayGetter) where T : AppearingEffectDisplay
	{
		if (previousValue != value)
		{
			textTween?.Kill();
			textTween = DOTween.To(() => textValue, delegate(int x)
			{
				textValue = x;
				text.text = x.ToString();
			}, value, textTweenDuration).SetEase(textEasing);
			pulseTween?.Kill(complete: true);
			pulseTween = text.transform.DOScale(pulseScale, pulseDuration).SetEase(Ease.InOutFlash, periodCount * 2).OnComplete(delegate
			{
				text.transform.localScale = Vector3.one;
			});
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.NightReport)
			{
				T val = effectDisplayGetter();
				val.FollowElement.ChangeTarget(base.transform);
				val.Init(value - previousValue);
				val.Display();
			}
			previousValue = value;
		}
	}

	public void SetColor(Color color)
	{
		text.color = color;
	}

	private void Awake()
	{
		text.text = previousValue.ToString();
	}
}
