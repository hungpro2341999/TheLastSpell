using TMPro;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Building.UI;

public class BuildingUpgradeTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI upgradeName;

	[SerializeField]
	private TextMeshProUGUI upgradeDescription;

	[SerializeField]
	private TextMeshProUGUI loreDescription;

	[SerializeField]
	private TextMeshProUGUI costText;

	private BuildingUpgrade buildingUpgrade;

	private BuildingUpgradeDefinition buildingUpgradeDefinition;

	private int upgradeLevel = -1;

	private bool useDefaultValues;

	public Transform FollowTarget
	{
		set
		{
			base.FollowElement.ChangeTarget(value);
		}
	}

	public void SetContent(BuildingUpgrade newBuildingUpgrade = null, BuildingUpgradeDefinition newBuildingUpgradeDefinition = null, bool newUseDefaultValues = false)
	{
		buildingUpgrade = newBuildingUpgrade;
		buildingUpgradeDefinition = buildingUpgrade?.BuildingUpgradeDefinition ?? newBuildingUpgradeDefinition;
		useDefaultValues = newUseDefaultValues;
		RefreshContent();
	}

	protected override bool CanBeDisplayed()
	{
		return buildingUpgradeDefinition != null;
	}

	protected override void RefreshContent()
	{
		upgradeLevel = Mathf.Clamp((buildingUpgrade != null) ? (buildingUpgrade.UpgradeLevel + 1) : 0, 0, buildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions.Count - 1);
		upgradeName.text = buildingUpgradeDefinition.GetNameAtLevel(upgradeLevel);
		upgradeDescription.text = buildingUpgradeDefinition.GetDescriptionAtLevel(upgradeLevel, buildingUpgrade?.Building, buildingUpgrade != null && buildingUpgrade.UpgradeLevel + 1 != upgradeLevel);
		loreDescription.text = buildingUpgradeDefinition.LoreDescription;
		costText.text = ComputeCostText();
	}

	private string ComputeCostText()
	{
		BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinition leveledBuildingUpgradeDefinition = buildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[upgradeLevel];
		string text = string.Empty;
		int num = (useDefaultValues ? leveledBuildingUpgradeDefinition.DefaultGoldCost : leveledBuildingUpgradeDefinition.GoldCost);
		int num2 = (useDefaultValues ? leveledBuildingUpgradeDefinition.DefaultMaterialsCost : leveledBuildingUpgradeDefinition.MaterialCost);
		if (num > 0)
		{
			text += $"<style=\"Gold\"><style=\"Number\">{num}</style></style>";
			if (num2 > 0)
			{
				text += "\n";
			}
		}
		if (num2 > 0)
		{
			text += $"<style=\"Materials\"><style=\"Number\">{num2}</style></style>";
		}
		return text;
	}
}
