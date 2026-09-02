using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs.Feedback;

public class UnlockedGlyphFeedback : MonoBehaviour
{
	[SerializeField]
	private GlyphDisplay glyphDisplayParent;

	public Animator UnlockAnimator;

	public Image UnlockImage;

	public void StopAnimation()
	{
		UnlockImage.enabled = false;
		UnlockAnimator.enabled = false;
	}

	public void TriggerUnlockAnimation()
	{
		UnlockAnimator.enabled = true;
	}

	public void GlyphDisplayParentRefreshLockedFeedback()
	{
		glyphDisplayParent.RefreshLockedFeedback(hideFeedback: false);
	}
}
