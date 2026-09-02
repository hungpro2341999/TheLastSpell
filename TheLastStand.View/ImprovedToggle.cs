using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View;

public class ImprovedToggle : Toggle
{
	[Serializable]
	public class EventDataUnityEvent : UnityEvent<PointerEventData>
	{
	}

	[SerializeField]
	private bool autoDeselection = true;

	[SerializeField]
	private EventDataUnityEvent onBeforePointerClick = new EventDataUnityEvent();

	[SerializeField]
	private UnityEvent onPointerClick = new UnityEvent();

	[SerializeField]
	private UnityEvent onPointerEnter = new UnityEvent();

	[SerializeField]
	private UnityEvent onPointerExit = new UnityEvent();

	public EventDataUnityEvent OnBeforePointerClickEvent => onBeforePointerClick;

	public UnityEvent OnPointerClickEvent => onPointerClick;

	public UnityEvent OnPointerEnterEvent => onPointerEnter;

	public UnityEvent OnPointerExitEvent => onPointerExit;

	public bool ShouldExecuteOnPointerClickEvent { get; set; } = true;

	public override void OnPointerClick(PointerEventData eventData)
	{
		onBeforePointerClick.Invoke(eventData);
		if (ShouldExecuteOnPointerClickEvent)
		{
			base.OnPointerClick(eventData);
			if (base.interactable)
			{
				onPointerClick.Invoke();
				if (autoDeselection)
				{
					EventSystem.current?.SetSelectedGameObject(null);
				}
			}
		}
		else
		{
			ShouldExecuteOnPointerClickEvent = true;
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (base.interactable)
		{
			onPointerEnter.Invoke();
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (base.interactable)
		{
			onPointerExit.Invoke();
		}
	}
}
