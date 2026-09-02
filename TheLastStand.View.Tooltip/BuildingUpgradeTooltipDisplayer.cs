using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.View.Building.UI;

namespace TheLastStand.View.Tooltip;

public class BuildingUpgradeTooltipDisplayer : IconTooltipDisplayer
{
	private BuildingUpgradeDefinition buildingUpgradeDefinition;

	private BuildingUpgradeTooltip BuildingUpgradeTooltip => targetTooltip as BuildingUpgradeTooltip;

	public override void DisplayTooltip()
	{
		BuildingUpgradeTooltip.SetContent(null, buildingUpgradeDefinition, newUseDefaultValues: true);
		base.DisplayTooltip();
	}

	public void Init(BuildingUpgradeDefinition newBuildingUpgradeDefinition, BuildingUpgradeTooltip buildingUpgradeTooltip = null, bool newIsFromLightShop = false)
	{
		buildingUpgradeDefinition = newBuildingUpgradeDefinition;
		Init(buildingUpgradeTooltip, newIsFromLightShop);
	}
}
