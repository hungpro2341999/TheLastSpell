using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;
using TheLastStand.View.MetaShops;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class BuildingActionIcon : OraculumUnlockIcon
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private BuildingActionTooltipDisplayer BuildingActionTooltipDisplayer;

	public void Init(BuildingActionDefinition buildingActionDefinition, MetaUpgradeLineView containerUpgrade, BuildingActionTooltip buildingActionTooltip = null, bool isLightShop = false)
	{
		icon.sprite = BuildingActionPanel.GetActionSprite(buildingActionDefinition.Id);
		BuildingActionTooltipDisplayer.Init(buildingActionDefinition, buildingActionTooltip, isLightShop);
		SetMetaUpgrade(containerUpgrade);
	}

	private void OnDisable()
	{
		if (BuildingActionTooltipDisplayer.IsDisplayingTargetTooltip)
		{
			BuildingActionTooltipDisplayer.HideTooltip();
		}
	}
}
