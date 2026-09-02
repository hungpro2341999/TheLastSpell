using System;
using System.Collections;
using DG.Tweening;
using TPLib;
using TheLastStand.Manager.Meta;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.WorldMap.Glyphs;

public class SelectedGlyphDisplay : AGlyphDisplay, IPointerClickHandler, IEventSystemHandler, ISubmitHandler
{
	private static class Constants
	{
		public static readonly int Selected = Animator.StringToHash("Selected");

		public static readonly int DestroySelf = Animator.StringToHash("DestroySelf");
	}

	[SerializeField]
	private Animator selectionFeedback;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private float fadeDuration = 0.25f;

	public bool DestroyingSelf { get; private set; }

	public void OnPointerClick(PointerEventData eventData)
	{
		TPSingleton<GlyphSelectionPanel>.Instance.UnselectGlyph(this);
		TPSingleton<GlyphManager>.Instance.GlyphTooltip.Hide();
	}

	public void OnSubmit(BaseEventData eventData)
	{
		TPSingleton<GlyphSelectionPanel>.Instance.OnSelectedGlyphJoystickSubmit(this);
		OnPointerClick(null);
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		TPSingleton<GlyphSelectionPanel>.Instance.OnSelectedGlyphJoystickSelect(this);
	}

	public IEnumerator DestroySelf(Func<IEnumerator> coroutineToCall, bool instantly = false)
	{
		DestroyingSelf = true;
		if (!instantly)
		{
			Tween t = canvasGroup.DOFade(0f, fadeDuration);
			yield return t.WaitForCompletion();
		}
		DestroyingSelf = false;
		UnityEngine.Object.Destroy(base.gameObject);
		yield return coroutineToCall?.Invoke();
	}
}
