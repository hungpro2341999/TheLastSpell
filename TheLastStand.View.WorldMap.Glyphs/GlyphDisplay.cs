using DG.Tweening;
using TPLib;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.WorldMap.Glyphs.Feedback;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs;

public class GlyphDisplay : AGlyphDisplayWithCost, IPointerClickHandler, IEventSystemHandler, ISubmitHandler
{
	private static class Constants
	{
		public static readonly int SelectionError = Animator.StringToHash("SelectionError");

		public static readonly int SelectionSmall = Animator.StringToHash("SelectionSmall");

		public static readonly int SelectionSmallReverse = Animator.StringToHash("SelectionSmallReverse");
	}

	[SerializeField]
	private Transform parentPivot;

	[SerializeField]
	private Image selectedFeedback;

	[SerializeField]
	private Image lockedFeedback;

	[SerializeField]
	private Animator glyphFeedback;

	[SerializeField]
	private CompletedGlyphFeedback completionFeedback;

	[SerializeField]
	private UnlockedGlyphFeedback unlockFeedback;

	[SerializeField]
	private float shakeDuration = 0.2f;

	[SerializeField]
	private int shakeStrength = 10;

	[SerializeField]
	private int shakeVibrato = 40;

	private bool isSelected;

	public UnlockedGlyphFeedback UnlockFeedback => unlockFeedback;

	public bool IsLocked { get; private set; }

	protected override bool CanDisplayTooltip => !IsLocked;

	public void Init(GlyphDefinition glyphDefinition, bool locked, bool isCompleted, bool triggerCompletionAnimation, bool newlyUnlocked)
	{
		Init(glyphDefinition);
		IsLocked = locked;
		completionFeedback.SetActiveCompletedImage(isCompleted && !triggerCompletionAnimation);
		completionFeedback.SetActiveCompletionAnimation(triggerCompletionAnimation);
		unlockFeedback.UnlockImage.enabled = false;
		unlockFeedback.UnlockAnimator.enabled = false;
		RefreshLockedFeedback(newlyUnlocked);
		RefreshMaterial(false);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (isSelected)
		{
			TPSingleton<GlyphSelectionPanel>.Instance.UnselectGlyph(this);
		}
		else if (!TPSingleton<GlyphSelectionPanel>.Instance.AnySelectedGlyphDestroying)
		{
			if (!IsLocked && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.WorldMapCityController.CanAddGlyph(base.GlyphDefinition))
			{
				TPSingleton<GlyphSelectionPanel>.Instance.SelectGlyph(this);
				return;
			}
			parentPivot.transform.DOComplete();
			parentPivot.transform.DOShakePosition(shakeDuration, Vector3.right * shakeStrength, shakeVibrato, 90f, snapping: false, fadeOut: false);
			glyphFeedback.SetTrigger(Constants.SelectionError);
			TPSingleton<GlyphSelectionPanel>.Instance.PlayErrorClip();
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		OnPointerClick(null);
	}

	public void SetSelection(bool selected)
	{
		isSelected = selected && !IsLocked;
		selectedFeedback.enabled = isSelected;
		unlockFeedback.StopAnimation();
		RefreshMaterial();
	}

	public void TriggerSelectionFeedback()
	{
		glyphFeedback.SetTrigger(GetSelectionTriggerId());
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (!IsLocked)
		{
			base.OnPointerEnter(eventData);
			RefreshMaterial(true);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (!IsLocked)
		{
			base.OnPointerExit(eventData);
			RefreshMaterial();
		}
	}

	private int GetSelectionTriggerId()
	{
		if (!isSelected)
		{
			return Constants.SelectionSmallReverse;
		}
		return Constants.SelectionSmall;
	}

	public void RefreshLockedFeedback(bool hideFeedback)
	{
		glyphCost.enabled = !hideFeedback && !IsLocked;
		customModeFeedback.enabled &= !hideFeedback && !IsLocked;
		glyphIcon.enabled = !hideFeedback && !IsLocked;
		lockedFeedback.enabled = hideFeedback || IsLocked;
		if (hideFeedback || IsLocked)
		{
			lockedFeedback.sprite = TPSingleton<GlyphManager>.Instance.GetRandomLockedIcon();
		}
	}

	private void RefreshMaterial(bool? forceDisplay = null)
	{
		Material material = ((forceDisplay ?? isSelected) ? TPSingleton<GlyphSelectionPanel>.Instance.GlyphSelectedMaterial : TPSingleton<GlyphSelectionPanel>.Instance.GlyphAvailableMaterial);
		glyphIcon.material = material;
		glyphCost.material = material;
		customModeFeedback.material = material;
	}
}
