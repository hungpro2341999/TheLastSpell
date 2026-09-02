using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class ButtonTooltipDisplayer : GenericTooltipDisplayer
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
	private Button button;

	public override bool CanDisplayTooltip()
	{
		if (base.HasFocus)
		{
			if (displayContext != E_DisplayContext.AlwaysDisplayOnHover && (displayContext != E_DisplayContext.OnlyDisplayTooltipWhenInteractable || !button.interactable))
			{
				if (displayContext == E_DisplayContext.OnlyDisplayTooltipWhenNotInteractable)
				{
					return !button.interactable;
				}
				return false;
			}
			return true;
		}
		return false;
	}
}
