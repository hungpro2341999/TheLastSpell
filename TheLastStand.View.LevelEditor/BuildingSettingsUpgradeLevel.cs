using System.Collections.Generic;
using TMPro;
using TheLastStand.Definition.Building.BuildingUpgrade;
using UnityEngine;

namespace TheLastStand.View.LevelEditor;

public class BuildingSettingsUpgradeLevel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI upgradeNameText;

	[SerializeField]
	private RectTransform upgradeLevelsButtonsContainer;

	[SerializeField]
	private BuildingSettingsUpgradeLevelButton upgradeLevelButtonPrefab;

	private List<BuildingSettingsUpgradeLevelButton> levelButtons = new List<BuildingSettingsUpgradeLevelButton>();

	public BuildingUpgradeDefinition BuildingUpgradeDefinition { get; private set; }

	public int CurrentUpgradeLevel { get; private set; }

	public void Init(BuildingUpgradeDefinition buildingUpgradeDefinition)
	{
		BuildingUpgradeDefinition = buildingUpgradeDefinition;
		upgradeNameText.text = buildingUpgradeDefinition.Id;
		for (int i = 0; i < BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions.Count + 1; i++)
		{
			BuildingSettingsUpgradeLevelButton buildingSettingsUpgradeLevelButton = Object.Instantiate(upgradeLevelButtonPrefab, upgradeLevelsButtonsContainer);
			buildingSettingsUpgradeLevelButton.Init(i, this);
			levelButtons.Add(buildingSettingsUpgradeLevelButton);
		}
		SetUpgradeLevel(0);
	}

	public void SetUpgradeLevel(int level)
	{
		levelButtons[CurrentUpgradeLevel].Highlight(state: false);
		CurrentUpgradeLevel = level;
		levelButtons[CurrentUpgradeLevel].Highlight(state: true);
	}
}
