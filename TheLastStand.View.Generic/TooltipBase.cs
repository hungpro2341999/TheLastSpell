using System.Collections;
using TPLib.Yield;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public abstract class TooltipBase : MonoBehaviour
{
	[SerializeField]
	private FollowElement followElement;

	[FormerlySerializedAs("rebuildMovingRectLayout")]
	[Tooltip("If true, will trigger a Layout Rebuild on [followElement] whenever the content is refreshed.This is useful if [followElement] has a Layout component that should be refreshed depending on the content.")]
	[SerializeField]
	private bool rebuildMovingLayout;

	[Tooltip("This RectTransform must contain the entire content of the tooltip, and will be used as reference for the final size.\nUsually is the first child.")]
	[SerializeField]
	protected RectTransform tooltipPanel;

	private CanvasGroup canvasGroup;

	private Coroutine delayedRefreshCoroutine;

	protected RectTransform rectTransform;

	private bool isInited;

	public bool Displayed
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		private set
		{
			base.gameObject.SetActive(value);
			canvasGroup.blocksRaycasts = value;
		}
	}

	public FollowElement FollowElement => followElement;

	public RectTransform RectTransform => base.transform as RectTransform;

	public void Hide()
	{
		if (isInited && Displayed)
		{
			Displayed = false;
			OnHide();
		}
	}

	public void HideWithoutClearingData()
	{
		if (isInited && Displayed)
		{
			Displayed = false;
		}
	}

	public void Refresh()
	{
		RefreshContent();
		if (delayedRefreshCoroutine == null)
		{
			if (base.gameObject.activeInHierarchy)
			{
				delayedRefreshCoroutine = StartCoroutine(RefreshLayoutDelayed());
			}
			else
			{
				RefreshLayout(showInstantly: true);
			}
		}
	}

	public void Display()
	{
		if (!CanBeDisplayed())
		{
			Hide();
			return;
		}
		if (!Displayed)
		{
			Displayed = true;
			canvasGroup.alpha = 0f;
			OnDisplay();
		}
		Refresh();
	}

	protected virtual void Awake()
	{
		rectTransform = base.transform as RectTransform;
		canvasGroup = GetComponent<CanvasGroup>();
		tooltipPanel.gameObject.SetActive(value: true);
		isInited = true;
		Hide();
	}

	protected abstract bool CanBeDisplayed();

	protected virtual void OnDisplay()
	{
	}

	protected virtual void OnHide()
	{
	}

	protected abstract void RefreshContent();

	protected virtual void RefreshLayout(bool showInstantly = false)
	{
		Vector2 sizeDelta = rectTransform.sizeDelta;
		sizeDelta.y = tooltipPanel.sizeDelta.y;
		rectTransform.sizeDelta = sizeDelta;
		if (FollowElement != null)
		{
			if (rebuildMovingLayout)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(FollowElement.RectTransform);
			}
			if (!FollowElement.FollowElementDatas.AlwaysFollow)
			{
				FollowElement.AutoMove();
			}
		}
		if (showInstantly)
		{
			canvasGroup.alpha = 1f;
		}
	}

	protected virtual IEnumerator RefreshLayoutDelayed()
	{
		yield return SharedYields.WaitForEndOfFrame;
		RefreshLayout();
		if (FollowElement != null && rebuildMovingLayout)
		{
			yield return SharedYields.WaitForEndOfFrame;
		}
		canvasGroup.alpha = 1f;
		delayedRefreshCoroutine = null;
	}

	private void OnDisable()
	{
		delayedRefreshCoroutine = null;
		FollowElement?.StopUseBy(this);
	}

	private void OnEnable()
	{
		FollowElement?.StartUseBy(this);
	}
}
