using System;
using DG.Tweening;
using UnityEngine;

namespace TheLastStand.Model.Animation;

[Serializable]
public class BiStatusTweenAnimation<T>
{
	public bool InStatusOne;

	public T StatusOne;

	public T StatusTwo;

	public float TransitionDuration;

	public Ease TransitionEase;

	[HideInInspector]
	public Tween StatusTransitionTween;
}
