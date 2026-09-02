using DG.Tweening;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand;

public class TestSix : MonoBehaviour
{
	public RectTransform globalRect;

	public Ease toShopEase = Ease.Linear;

	public Ease toHubEase = Ease.Linear;

	private Tween tween;

	private void Transition(Vector2 pivot, Ease ease)
	{
		globalRect.SetAnchors(pivot);
		globalRect.SetPivot(pivot);
		tween = globalRect.DOAnchorPosX(0f, 1f).SetEase(ease);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			Transition(new Vector2(1f, 0.5f), toShopEase);
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			Transition(new Vector2(0f, 0.5f), toShopEase);
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			Transition(new Vector2(0.5f, 0.5f), toHubEase);
		}
	}
}
