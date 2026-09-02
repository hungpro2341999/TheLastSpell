using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class SelectableTooltipDisplayer : GenericTooltipDisplayer
{
	private enum E_DisplayContext
	{
		AlwaysDisplayOnHover,
		OnlyDisplayTooltipWhenInteractable,
		OnlyDisplayTooltipWhenNotInteractable
	}

	[SerializeField]
	private E_DisplayContext displayContext;

	[SerializeField]
	[FormerlySerializedAs("button")]
	private Selectable selectable;

	public override bool CanDisplayTooltip()
	{
		if (displayContext != E_DisplayContext.AlwaysDisplayOnHover && (displayContext != E_DisplayContext.OnlyDisplayTooltipWhenInteractable || !selectable.interactable))
		{
			if (displayContext == E_DisplayContext.OnlyDisplayTooltipWhenNotInteractable)
			{
				return !selectable.interactable;
			}
			return false;
		}
		return true;
	}
}
