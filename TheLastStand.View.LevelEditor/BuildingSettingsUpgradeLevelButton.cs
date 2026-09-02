using TheLastStand.Framework.UI;
using UnityEngine;

namespace TheLastStand.View.LevelEditor;

public class BuildingSettingsUpgradeLevelButton : MonoBehaviour
{
	[SerializeField]
	private BetterButton upgradeLevelButton;

	[SerializeField]
	private GameObject highlight;

	[SerializeField]
	private Color baseColor = Color.white;

	[SerializeField]
	private Color highlightColor = Color.white;

	private int level;

	private BuildingSettingsUpgradeLevel buildingSettingsUpgradeLevel;

	public void Highlight(bool state)
	{
		highlight.SetActive(state);
		upgradeLevelButton.ChangeTextColor(state ? highlightColor : baseColor);
	}

	public void Init(int level, BuildingSettingsUpgradeLevel buildingSettingsUpgradeLevel)
	{
		this.level = level;
		this.buildingSettingsUpgradeLevel = buildingSettingsUpgradeLevel;
		upgradeLevelButton.ChangeText(level.ToString());
		Highlight(state: false);
		upgradeLevelButton.onClick.AddListener(OnButtonClick);
	}

	private void OnButtonClick()
	{
		buildingSettingsUpgradeLevel.SetUpgradeLevel(level);
	}
}
