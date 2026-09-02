using TheLastStand.Definition.Building;
using TheLastStand.View.Building.UI;

namespace TheLastStand.View.Tooltip;

public class BuildingTooltipDisplayer : IconTooltipDisplayer
{
	private BuildingDefinition buildingDefinition;

	private BuildingConstructionTooltip BuildingConstructionTooltip => targetTooltip as BuildingConstructionTooltip;

	public override void DisplayTooltip()
	{
		BuildingConstructionTooltip.Init(buildingDefinition, newUseDefaultValues: true);
		base.DisplayTooltip();
	}

	public void Init(BuildingDefinition newBuildingDefinition, BuildingConstructionTooltip buildingTooltip = null, bool newIsFromLightShop = false)
	{
		buildingDefinition = newBuildingDefinition;
		Init(buildingTooltip, newIsFromLightShop);
	}
}
