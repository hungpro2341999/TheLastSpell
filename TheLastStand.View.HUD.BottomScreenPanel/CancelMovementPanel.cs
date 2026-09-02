using DG.Tweening;
using TheLastStand.Framework.UI;
using TheLastStand.Manager.Unit;
using UnityEngine;

namespace TheLastStand.View.HUD.BottomScreenPanel;

public class CancelMovementPanel : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private float rectTransformClosedX = -73f;

	[SerializeField]
	private float rectTransformOpenedX = -14f;

	[SerializeField]
	[Range(0f, 1f)]
	private float tweenDuration = 0.3f;

	[SerializeField]
	private BetterButton cancelMovementButton;

	private Tween openTween;

	private Tween closeTween;

	public void Refresh()
	{
		if (PlayableUnitManager.CanUndoLastCommand())
		{
			if (cancelMovementButton.interactable)
			{
				return;
			}
			closeTween = null;
			cancelMovementButton.interactable = true;
			canvas.enabled = true;
			openTween = rectTransform.DOAnchorPosX(rectTransformOpenedX, tweenDuration, snapping: true).SetEase(Ease.OutBack).OnComplete(delegate
			{
				if (cancelMovementButton.interactable)
				{
					canvas.enabled = true;
				}
				openTween = null;
			});
			openTween.Play();
		}
		else
		{
			if (!cancelMovementButton.interactable)
			{
				return;
			}
			openTween = null;
			cancelMovementButton.interactable = false;
			closeTween = rectTransform.DOAnchorPosX(rectTransformClosedX, tweenDuration, snapping: true).SetEase(Ease.InBack).OnComplete(delegate
			{
				if (!cancelMovementButton.interactable)
				{
					canvas.enabled = false;
				}
				closeTween = null;
			});
			closeTween.Play();
		}
	}
}
