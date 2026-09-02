using TheLastStand.Definition.Building;
using TheLastStand.Framework;
using TheLastStand.View.MetaShops;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class BuildingIcon : OraculumUnlockIcon
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private BuildingTooltipDisplayer BuildingTooltipDisplayer;

	public void Init(BuildingDefinition buildingDefinition, MetaUpgradeLineView containerUpgrade, BuildingConstructionTooltip buildingConstructionTooltip = null, bool isLightShop = false)
	{
		icon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Buildings/Portraits/Icons_Buildings_" + buildingDefinition.Id);
		BuildingTooltipDisplayer.Init(buildingDefinition, buildingConstructionTooltip, isLightShop);
		SetMetaUpgrade(containerUpgrade);
	}

	private void OnDisable()
	{
		if (BuildingTooltipDisplayer.IsDisplayingTargetTooltip)
		{
			BuildingTooltipDisplayer.HideTooltip();
		}
	}
}
