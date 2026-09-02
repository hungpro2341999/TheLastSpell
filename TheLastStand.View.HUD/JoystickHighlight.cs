using System;
using System.Collections;
using DG.Tweening;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class JoystickHighlight : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Image image;

	[SerializeField]
	private GamepadButtonsDisplayLayout gamepadButtonsDisplayLayout;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Vector2 sizeDeltaIncrement = Vector2.zero;

	[SerializeField]
	private Vector2 minSize = new Vector2(1000f, 1000f);

	private bool alwaysFollow;

	private Tween highlightPositionTween;

	private Tween highlightSizeTween;

	private JoystickHighlighter currentHighlighter;

	public bool IsMoving => highlightPositionTween.IsPlaying();

	public RectTransform RectTransform { get; private set; }

	public Vector2 SizeDeltaIncrement => sizeDeltaIncrement;

	public bool AlwaysFollow
	{
		get
		{
			return alwaysFollow;
		}
		private set
		{
			if (alwaysFollow != value)
			{
				alwaysFollow = value;
				if (alwaysFollow)
				{
					StartCoroutine(FollowHighlighterCoroutine());
				}
			}
		}
	}

	public bool Displayed { get; private set; }

	public void ToggleAlwaysFollow(bool state, bool force = false)
	{
		if (force || InputManager.IsLastControllerJoystick)
		{
			AlwaysFollow = state;
		}
	}

	public void Display(bool state, bool force = false)
	{
		if (Displayed != state || force)
		{
			animator.enabled = state;
			image.enabled = state;
			if (state)
			{
				gamepadButtonsDisplayLayout.Show();
			}
			else
			{
				gamepadButtonsDisplayLayout.Hide();
			}
			Displayed = state;
		}
	}

	public void UpdateGamepadInputDisplays()
	{
		gamepadButtonsDisplayLayout.ToggleLayoutGroup(state: true);
		gamepadButtonsDisplayLayout.RefreshGamepadInputDisplays(currentHighlighter.GamepadButtonTypes);
	}

	public void FollowHighlighter(JoystickHighlighter highlighter)
	{
		currentHighlighter = highlighter;
		UpdateGamepadInputDisplays();
		highlightPositionTween?.Kill();
		highlightSizeTween?.Kill();
		Vector3 targetPosition = currentHighlighter.RectTransform.GetWorldRect().center;
		gamepadButtonsDisplayLayout.TargetPosition = currentHighlighter.RectTransform.GetWorldRect().min;
		Vector3 vector = currentHighlighter.RectTransform.rect.size + SizeDeltaIncrement;
		vector.x = Mathf.Max(vector.x, minSize.x);
		vector.y = Mathf.Max(vector.y, minSize.y);
		Func<Vector3> func = () => (!AlwaysFollow) ? targetPosition : ((Vector3)currentHighlighter.RectTransform.GetWorldRect().center);
		highlightPositionTween = base.transform.DOMove(func(), InputManager.JoystickConfig.HUDNavigation.HighlightTweenDuration).SetEase(InputManager.JoystickConfig.HUDNavigation.HighlightTweenEase);
		highlightSizeTween = RectTransform.DOSizeDelta(vector, InputManager.JoystickConfig.HUDNavigation.HighlightTweenDuration).SetEase(InputManager.JoystickConfig.HUDNavigation.HighlightTweenEase).OnComplete(delegate
		{
			gamepadButtonsDisplayLayout.ToggleLayoutGroup(state: false);
		});
		Display(state: true);
	}

	public void ForcePositionUpdate()
	{
		if (currentHighlighter != null)
		{
			base.transform.position = currentHighlighter.RectTransform.GetWorldRect().center;
		}
	}

	private void Awake()
	{
		gamepadButtonsDisplayLayout.Init();
		RectTransform = base.transform as RectTransform;
		Display(state: false, force: true);
	}

	private IEnumerator Start()
	{
		canvas.sortingOrder++;
		yield return null;
		canvas.sortingOrder--;
	}

	private IEnumerator FollowHighlighterCoroutine()
	{
		while (AlwaysFollow)
		{
			yield return null;
			ForcePositionUpdate();
		}
	}

	private void DebugToggleAlwaysFollow()
	{
		ToggleAlwaysFollow(state: true, force: true);
	}

	private void DebugRemoveAlwaysFollow()
	{
		ToggleAlwaysFollow(state: false, force: true);
	}
}
