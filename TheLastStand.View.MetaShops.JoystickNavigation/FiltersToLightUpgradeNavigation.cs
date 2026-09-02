using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.View.MetaShops.JoystickNavigation;

public class FiltersToLightUpgradeNavigation : AFiltersToUpgradeNavigation
{
	protected override List<MetaUpgradeLineView> GetSortedLines()
	{
		return TPSingleton<LightShopManager>.Instance.SortedLines;
	}
}
