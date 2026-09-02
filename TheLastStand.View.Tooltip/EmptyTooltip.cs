using TheLastStand.View.Generic;

namespace TheLastStand.View.Tooltip;

public class EmptyTooltip : TooltipBase
{
	protected override bool CanBeDisplayed()
	{
		return true;
	}

	protected override void RefreshContent()
	{
	}
}
