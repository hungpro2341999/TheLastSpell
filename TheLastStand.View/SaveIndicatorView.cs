using System.Collections;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Framework;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View;

public class SaveIndicatorView : MonoBehaviour
{
	[SerializeField]
	private Image indicatorImage;

	[SerializeField]
	private float minimumLifeSpan = 2f;

	[SerializeField]
	private TextMeshProUGUI saveText;

	[SerializeField]
	private DataColor badColor;

	[SerializeField]
	private DataColor progressColor;

	[SerializeField]
	private DataColor goodColor;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private float showDuration = 0.35f;

	[SerializeField]
	private float showPosition = 120f;

	[SerializeField]
	private Ease showEasing = Ease.OutBack;

	[SerializeField]
	private float hideDuration = 0.35f;

	[SerializeField]
	private float hidePosition = 120f;

	[SerializeField]
	private Ease hideEasing = Ease.OutBack;

	[SerializeField]
	private float blinkDuration = 0.35f;

	[SerializeField]
	private float blinkMinValue = 0.7f;

	[SerializeField]
	private Ease blinkEasing = Ease.OutBack;

	private Coroutine hideCoroutine;

	private float openedTime;

	private Tween hideTween;

	private Tween showTween;

	private Tween blinkTween;

	private void Start()
	{
		SaverLoader.OnGameSavingStarts += OnGameSavingStarts;
		SaverLoader.OnGameSavingEnds += OnGameSavingEnds;
	}

	private float GetOpacityValue()
	{
		return Mathf.Abs(Mathf.Sin(Time.time * 3f)) * 0.3f + 0.7f;
	}

	private void Hide(bool isSuccessful = true)
	{
		saveText.text = (isSuccessful ? Localizer.Get("Save_Success") : Localizer.Get("Save_Failure"));
		saveText.color = (isSuccessful ? goodColor._Color : badColor._Color);
		showTween?.Kill();
		hideTween?.Kill();
		hideTween = rectTransform.DOAnchorPosX(hidePosition, hideDuration).SetEase(hideEasing);
		hideTween.Play();
		blinkTween?.Kill();
		indicatorImage.color = new Color(indicatorImage.color.r, indicatorImage.color.g, indicatorImage.color.b, 1f);
		openedTime = 0f;
	}

	private IEnumerator HideAfterTime(bool isSuccessful)
	{
		while (openedTime > Time.time - minimumLifeSpan)
		{
			yield return SharedYields.WaitForEndOfFrame;
		}
		Hide(isSuccessful);
		hideCoroutine = null;
	}

	private void OnDestroy()
	{
		SaverLoader.OnGameSavingStarts -= OnGameSavingStarts;
		SaverLoader.OnGameSavingEnds -= OnGameSavingEnds;
	}

	[ContextMenu("Play Blink Animation")]
	private void BlinkAnimation()
	{
		blinkTween?.Kill();
		indicatorImage.color = new Color(indicatorImage.color.r, indicatorImage.color.g, indicatorImage.color.b, 1f);
		blinkTween = indicatorImage.DOFade(blinkMinValue, blinkDuration).SetEase(blinkEasing).SetLoops(-1, LoopType.Yoyo);
		blinkTween.Play();
	}

	private void OnGameSavingStarts()
	{
		Show();
	}

	private void OnGameSavingEnds(bool isSuccessful)
	{
		if (!isSuccessful)
		{
			GenericPopUp.Open("Popup_SaveWriting_ErrorTitle", "Popup_SaveWriting_ErrorText", ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Stats/Icons/VerySmall_Critical"));
			if (hideCoroutine != null)
			{
				StopCoroutine(hideCoroutine);
				hideCoroutine = null;
			}
			Hide(isSuccessful: false);
		}
		else if (hideCoroutine == null)
		{
			hideCoroutine = StartCoroutine(HideAfterTime(isSuccessful));
		}
	}

	private void Show()
	{
		if (UIManager.DebugToggleUI != false)
		{
			saveText.text = Localizer.Get("Save_Saving");
			saveText.color = progressColor._Color;
			hideTween?.Kill();
			showTween?.Kill();
			showTween = rectTransform.DOAnchorPosX(showPosition, showDuration).SetEase(showEasing);
			showTween.Play();
			BlinkAnimation();
			openedTime = Time.time;
		}
	}
}
