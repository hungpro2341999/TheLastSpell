using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Model.Building;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class BuildingSettingsPanel : MonoBehaviour
{
	[SerializeField]
	private Button panelDisplayButton;

	[SerializeField]
	private TextMeshProUGUI buildingNameText;

	[SerializeField]
	private Image buildingIcon;

	[SerializeField]
	private RectTransform settingsLayout;

	[SerializeField]
	private BuildingSettingsUpgradeLevel buildingSettingsUpgradeLevelPrefab;

	[SerializeField]
	private BuildingSettingsHealth buildingSettingsHealth;

	[SerializeField]
	private GameObject panel;

	[SerializeField]
	private Vector2 panelOffset = Vector2.zero;

	private RectTransform rectTransform;

	public TheLastStand.Model.Building.Building Building { get; private set; }

	public BuildingSettingsHealth BuildingSettingsHealth => buildingSettingsHealth;

	public List<BuildingSettingsUpgradeLevel> BuildingSettingsUpgradeLevels { get; } = new List<BuildingSettingsUpgradeLevel>();

	public RectTransform RectTransform
	{
		get
		{
			if (rectTransform == null)
			{
				rectTransform = base.transform as RectTransform;
			}
			return rectTransform;
		}
	}

	public static event Action<BuildingSettingsPanel> PanelOpen;

	public void DisplayPanel(bool show)
	{
		panel.SetActive(show);
		if (show)
		{
			BuildingSettingsPanel.PanelOpen?.Invoke(this);
		}
	}

	public void Init(TheLastStand.Model.Building.Building building)
	{
		Building = building;
		buildingNameText.text = Building.BuildingDefinition.Id;
		buildingIcon.sprite = Building.BuildingView.GetPortraitSprite();
		if (!building.BlueprintModule.IsIndestructible)
		{
			buildingSettingsHealth.Init(building.BuildingDefinition);
		}
		if (building.UpgradeModule != null && building.BuildingDefinition.UpgradeModuleDefinition.BuildingUpgradeDefinitions != null)
		{
			foreach (BuildingUpgradeDefinition buildingUpgradeDefinition in building.BuildingDefinition.UpgradeModuleDefinition.BuildingUpgradeDefinitions)
			{
				BuildingSettingsUpgradeLevel buildingSettingsUpgradeLevel = UnityEngine.Object.Instantiate(buildingSettingsUpgradeLevelPrefab, settingsLayout);
				buildingSettingsUpgradeLevel.Init(buildingUpgradeDefinition);
				BuildingSettingsUpgradeLevels.Add(buildingSettingsUpgradeLevel);
			}
		}
		DisplayPanel(show: false);
		LayoutRebuilder.ForceRebuildLayoutImmediate(settingsLayout);
		base.gameObject.SetActive(value: false);
	}

	public void Init(TheLastStand.Model.Building.Building building, XContainer buildingContainer)
	{
		Init(building);
		XElement xElement = buildingContainer.Element("Health");
		if (xElement != null)
		{
			BuildingSettingsHealth.SetHealth(int.Parse(xElement.Value));
		}
		XElement xElement2 = buildingContainer.Element("UpgradesLevels");
		if (xElement2 == null)
		{
			return;
		}
		foreach (XElement upgradeLevelElement in xElement2.Elements())
		{
			BuildingSettingsUpgradeLevels.Where((BuildingSettingsUpgradeLevel o) => o.BuildingUpgradeDefinition.Id == upgradeLevelElement.Name.LocalName).First().SetUpgradeLevel(int.Parse(upgradeLevelElement.Value));
		}
	}

	private void OnPanelOpen(BuildingSettingsPanel buildingSettingsPanel)
	{
		if (buildingSettingsPanel != this)
		{
			DisplayPanel(show: false);
		}
	}

	private void Awake()
	{
		panelDisplayButton.onClick.AddListener(delegate
		{
			DisplayPanel(!panel.activeSelf);
		});
		PanelOpen += OnPanelOpen;
	}

	private void Update()
	{
		Vector2 vector = Building.BuildingView.transform.position + (Vector3)panelOffset;
		Vector2 vector2 = ACameraView.MainCam.WorldToViewportPoint(vector);
		RectTransform.anchorMin = vector2;
		RectTransform.anchorMax = vector2;
	}

	private void OnDestroy()
	{
		panelDisplayButton.onClick.RemoveAllListeners();
		PanelOpen -= OnPanelOpen;
	}
}
