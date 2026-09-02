using DG.Tweening;
using UnityEngine;

namespace TheLastStand.View.Building.Construction;

public class GamepadInputFeedback : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 10f)]
	private float targetStretchOrShrinkScale = 1.2f;

	[SerializeField]
	[Range(0.01f, 10f)]
	private float tweenDuration = 0.5f;

	protected virtual void Start()
	{
		base.transform.DOScale(targetStretchOrShrinkScale, tweenDuration).SetLoops(-1, LoopType.Yoyo);
	}
}
