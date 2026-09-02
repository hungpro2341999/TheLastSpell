using System;
using DG.Tweening;
using TPLib;
using TheLastStand.Framework.Automaton;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Manager;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFadeManager : Manager<CanvasFadeManager>
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Ease defaultEase = Ease.InCubic;

	[SerializeField]
	private int defaultSortingOrder = 9999;

	[SerializeField]
	private float fadeDuration;

	[SerializeField]
	private Image image;

	private bool hasBeenInitialized;

	private Tween currentTween;

	public CanvasGroup CanvasGroup => canvasGroup;

	public bool FadeIsOver { get; private set; }

	public float FadeDuration => fadeDuration;

	public static void FadeIn(float duration = -1f, int sortingOrder = -1, Ease ease = Ease.Unset, CanvasGroup canvasGroup = null, Action callback = null)
	{
		TPSingleton<CanvasFadeManager>.Instance.DoFadeTween(Color.black, 1f, duration, sortingOrder, ease, canvasGroup, callback);
	}

	public static void FadeIn(Color color, float duration = -1f, int sortingOrder = -1, Ease ease = Ease.Unset, CanvasGroup canvasGroup = null, Action callback = null)
	{
		TPSingleton<CanvasFadeManager>.Instance.DoFadeTween(color, 1f, duration, sortingOrder, ease, canvasGroup, callback);
	}

	public static void FadeOut(float duration = -1f, int sortingOrder = -1, Ease ease = Ease.Unset, CanvasGroup canvasGroup = null, Action callback = null)
	{
		TPSingleton<CanvasFadeManager>.Instance.DoFadeTween(Color.black, 0f, duration, sortingOrder, ease, canvasGroup, callback);
	}

	public static void FadeOut(Color color, float duration = -1f, int sortingOrder = -1, Ease ease = Ease.Unset, CanvasGroup canvasGroup = null, Action callback = null)
	{
		TPSingleton<CanvasFadeManager>.Instance.DoFadeTween(color, 0f, duration, sortingOrder, ease, canvasGroup, callback);
	}

	public static void FadeTo(float alpha, float duration = -1f, int sortingOrder = -1, Ease ease = Ease.Unset, CanvasGroup canvasGroup = null, Action callback = null)
	{
		TPSingleton<CanvasFadeManager>.Instance.DoFadeTween(Color.black, Mathf.Clamp(alpha, 0f, 1f), duration, sortingOrder, ease, canvasGroup, callback);
	}

	public void OnApplicationStateChange(State state)
	{
		string text = state.GetName();
		if (text != null)
		{
			switch (text)
			{
			case "Credits":
			case "AnimatedCutscene":
			case "GameLobby":
			case "Game":
			case "LevelEditor":
			case "MetaShops":
			case "WorldMap":
				FadeOut();
				break;
			}
		}
	}

	private void DoFadeTween(Color color, float alpha, float duration = -1f, int sortingOrder = -1, Ease ease = Ease.Unset, CanvasGroup canvasGroup = null, Action callback = null)
	{
		TPSingleton<CanvasFadeManager>.Instance.canvas.sortingOrder = ((sortingOrder == -1) ? TPSingleton<CanvasFadeManager>.Instance.defaultSortingOrder : sortingOrder);
		TPSingleton<CanvasFadeManager>.Instance.FadeIsOver = false;
		TPSingleton<CanvasFadeManager>.Instance.image.color = color;
		if (duration < 0f)
		{
			duration = TPSingleton<CanvasFadeManager>.Instance.FadeDuration;
		}
		if (canvasGroup == null)
		{
			canvasGroup = TPSingleton<CanvasFadeManager>.Instance.canvasGroup;
		}
		if (ease == Ease.Unset)
		{
			ease = TPSingleton<CanvasFadeManager>.Instance.defaultEase;
		}
		if (TPSingleton<CanvasFadeManager>.Instance.currentTween != null)
		{
			TPSingleton<CanvasFadeManager>.Instance.currentTween.Kill();
			TPSingleton<CanvasFadeManager>.Instance.currentTween = null;
		}
		TPSingleton<CanvasFadeManager>.Instance.currentTween = this.canvasGroup.DOFade(alpha, duration).SetEase(ease).OnComplete(delegate
		{
			TPSingleton<CanvasFadeManager>.Instance.FadeIsOver = true;
			TPSingleton<CanvasFadeManager>.Instance.currentTween = null;
			callback?.Invoke();
		});
	}

	private void Init()
	{
		if (!hasBeenInitialized)
		{
			hasBeenInitialized = true;
			FadeIsOver = true;
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent += OnApplicationStateChange;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (TPSingleton<ApplicationManager>.Exist())
		{
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent -= OnApplicationStateChange;
		}
	}
}
