using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Framework;
using TheLastStand.View.MetaShops;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class BuildingUpgradeIcon : OraculumUnlockIcon
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private BuildingUpgradeTooltipDisplayer BuildingUpgradeTooltipDisplayer;

	public void Init(BuildingUpgradeDefinition buildingUpgradeDefinition, MetaUpgradeLineView containerUpgrade, BuildingUpgradeTooltip buildingUpgradeTooltip = null, bool isLightShop = false)
	{
		icon.sprite = ResourcePooler<Sprite>.LoadOnce("View/Sprites/UI/Buildings/Upgrades/" + buildingUpgradeDefinition.Id + "0");
		BuildingUpgradeTooltipDisplayer.Init(buildingUpgradeDefinition, buildingUpgradeTooltip, isLightShop);
		SetMetaUpgrade(containerUpgrade);
	}

	private void OnDisable()
	{
		if (BuildingUpgradeTooltipDisplayer.IsDisplayingTargetTooltip)
		{
			BuildingUpgradeTooltipDisplayer.HideTooltip();
		}
	}
}
