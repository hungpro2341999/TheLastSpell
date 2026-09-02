using System.Collections;
using Sirenix.OdinInspector;
using TPLib.Yield;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class EffectDisplay : SerializedMonoBehaviour, IDisplayableEffect
{
	public static class Constants
	{
		public const string EffectDisplayPath = "Prefab/Displayable Effect/UI Effect Displays/";
	}

	[SerializeField]
	protected CanvasGroup canvasGroup;

	[SerializeField]
	protected float displayDuration = 0.6f;

	[SerializeField]
	protected float delayBeforeDestruction;

	private bool isBeingDisplayed;

	private FollowElement followElement;

	protected virtual float DisplayDuration => displayDuration;

	public FollowElement FollowElement
	{
		get
		{
			if (followElement == null)
			{
				followElement = GetComponent<FollowElement>();
				if (followElement == null)
				{
					followElement = base.gameObject.AddComponent<FollowElement>();
				}
			}
			return followElement;
		}
	}

	public bool IsBeingDisplayed => isBeingDisplayed;

	public Coroutine Display()
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 1f;
		}
		if (FollowElement != null)
		{
			FollowElement.AutoMove();
		}
		return StartCoroutine(DisplayAndDestroyCoroutine());
	}

	protected virtual void OnEnable()
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 0f;
		}
	}

	protected virtual IEnumerator DisplayCoroutine()
	{
		if (displayDuration > 0f)
		{
			yield return SharedYields.WaitForSeconds(displayDuration);
		}
	}

	protected virtual IEnumerator DisplayAndDestroyCoroutine()
	{
		isBeingDisplayed = true;
		yield return StartCoroutine(DisplayCoroutine());
		if (delayBeforeDestruction > 0f)
		{
			yield return SharedYields.WaitForSeconds(delayBeforeDestruction);
		}
		isBeingDisplayed = false;
		base.gameObject.SetActive(value: false);
	}
}
