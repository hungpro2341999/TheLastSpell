using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.View.MetaShops.JoystickNavigation;

public class FiltersToDarkUpgradeNavigation : AFiltersToUpgradeNavigation
{
	protected override List<MetaUpgradeLineView> GetSortedLines()
	{
		return TPSingleton<DarkShopManager>.Instance.SortedLines;
	}
}
