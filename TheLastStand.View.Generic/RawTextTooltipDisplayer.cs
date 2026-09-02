using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Generic;

public class RawTextTooltipDisplayer : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public RawTextTooltip TargetTooltip;

	protected bool HasFocus { get; set; }

	public string Text { get; set; }

	public void OnPointerEnter(PointerEventData eventData)
	{
		HasFocus = true;
		if (CanDisplayTooltip())
		{
			DisplayTooltip(display: true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		HasFocus = false;
		DisplayTooltip(display: false);
	}

	public virtual void OnDisable()
	{
		if (HasFocus)
		{
			HasFocus = false;
			DisplayTooltip(display: false);
		}
	}

	protected virtual void DisplayTooltip(bool display)
	{
		if (display)
		{
			TargetTooltip.SetContent(Text);
			TargetTooltip.Display();
		}
		else
		{
			TargetTooltip.Hide();
		}
	}

	protected virtual bool CanDisplayTooltip()
	{
		return true;
	}
}
