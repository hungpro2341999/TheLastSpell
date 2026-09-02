using System;
using System.Collections;
using DG.Tweening;
using RedBlueGames.Tools.TextTyper;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Model;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Unit;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View;

public class BarkView : MonoBehaviour
{
	[SerializeField]
	private float scaleInDuration = 0.2f;

	[SerializeField]
	private AnimationCurve scaleInEasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private Transform scaleInTarget;

	[SerializeField]
	private float fadeInDuration = 0.2f;

	[SerializeField]
	private Ease fadeInEasing = Ease.OutQuart;

	[SerializeField]
	private float fadeOutDuration = 1f;

	[SerializeField]
	private Ease fadeOutEasing = Ease.OutQuart;

	[SerializeField]
	private float arrowTweenDuration = 0.2f;

	[SerializeField]
	private Ease arrowTweenEasing = Ease.InOutSine;

	[SerializeField]
	private Transform arrowTransform;

	[SerializeField]
	[Tooltip("Delay between the start of the bark appearance and the start of the text being typed")]
	private float textDisplayDelay = 0.1f;

	[SerializeField]
	[Tooltip("Delay between the end of the text being typed and the start of the bark disappearance")]
	private float postTextApparitionDelay = 0.2f;

	[SerializeField]
	[Tooltip("Min duration of the text being displayed on the screen. If [text being typed duration] < this value, then we still wait this duration before firing the postTextApparitionDelay wait (Prevents small text disappearing too quickly).")]
	private float displayMinDuration = 1.6f;

	[SerializeField]
	private FollowElement followElement;

	[SerializeField]
	private TextTyper textTyper;

	[SerializeField]
	private TextMeshProUGUI sentenceText;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private LocalizedFont localizedFont;

	public Bark Bark { get; set; }

	public void Display(Action callback = null)
	{
		if (textTyper != null)
		{
			textTyper.Init();
			textTyper.CharacterPrinted.RemoveAllListeners();
			textTyper.CharacterPrinted.AddListener(delegate
			{
				SoundManager.PlayAudioClip(UIManager.BarkTextDisplayAudioClip);
			});
		}
		if (followElement != null)
		{
			followElement.ChangeTarget(Bark.Barker.BarkViewFollowTarget);
		}
		localizedFont?.RefreshFont();
		StartCoroutine(DisplayCoroutine(dontDestroy: false, callback));
	}

	private void OnDisable()
	{
		if (textTyper != null)
		{
			textTyper.gameObject.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		if (textTyper != null)
		{
			textTyper.gameObject.SetActive(value: true);
		}
	}

	private IEnumerator DisplayCoroutine(bool dontDestroy = false, Action callback = null)
	{
		string arg = string.Empty;
		if (Bark.Barker is TheLastStand.Model.Unit.Unit unit)
		{
			arg = unit.Name;
		}
		else if (Bark.Barker is BlueprintModule blueprintModule)
		{
			arg = blueprintModule.BuildingParent.BuildingDefinition.Id;
		}
		CLoggerManager.Log($"{Time.time} : {arg} barks {Bark.Sentence}", this, LogType.Log, CLogLevel.DETAILED, forcePrintInUnity: true, "Barks");
		if (arrowTransform != null)
		{
			arrowTransform.DOLocalMoveY(-3f, arrowTweenDuration).SetRelative().SetLoops(-1, LoopType.Yoyo)
				.SetEase(arrowTweenEasing)
				.SetUpdate(isIndependentUpdate: false);
		}
		canvasGroup.alpha = 0f;
		Tweener t = canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEasing).SetUpdate(isIndependentUpdate: false);
		if (scaleInTarget != null)
		{
			scaleInTarget.DOScale(0f, scaleInDuration).From().SetEase(scaleInEasingCurve)
				.SetUpdate(isIndependentUpdate: false);
		}
		if (textTyper != null)
		{
			Sequence s = DOTween.Sequence();
			s.AppendInterval(textDisplayDelay);
			if (Bark == null)
			{
				s.AppendCallback(delegate
				{
					textTyper.TypeText(sentenceText.text);
				});
			}
			else
			{
				s.AppendCallback(delegate
				{
					textTyper.TypeText(Bark.Sentence);
				});
			}
		}
		yield return t.WaitForCompletion();
		float endTime = Time.time + displayMinDuration;
		yield return new WaitWhile(() => textTyper.IsTyping || Time.time < endTime);
		yield return SharedYields.WaitForSeconds(postTextApparitionDelay);
		t = canvasGroup.DOFade(0f, fadeOutDuration).SetEase(fadeOutEasing).SetUpdate(isIndependentUpdate: false);
		yield return t.WaitForCompletion();
		callback?.Invoke();
		if (!dontDestroy)
		{
			TPSingleton<BarkManager>.Instance.RemoveBark(Bark);
		}
	}
}
