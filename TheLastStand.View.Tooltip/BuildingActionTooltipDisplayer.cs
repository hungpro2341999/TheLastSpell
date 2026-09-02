using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.View.Building.UI;

namespace TheLastStand.View.Tooltip;

public class BuildingActionTooltipDisplayer : IconTooltipDisplayer
{
	private BuildingActionDefinition buildingActionDefinition;

	private BuildingActionTooltip BuildingActionTooltip => targetTooltip as BuildingActionTooltip;

	public override void DisplayTooltip()
	{
		BuildingActionTooltip.Init(null, buildingActionDefinition, newFetchDefaultValues: true);
		base.DisplayTooltip();
	}

	public void Init(BuildingActionDefinition newBuildingActionDefinition, BuildingActionTooltip buildingActionTooltip = null, bool newIsFromLightShop = false)
	{
		buildingActionDefinition = newBuildingActionDefinition;
		Init(buildingActionTooltip, newIsFromLightShop);
	}
}
