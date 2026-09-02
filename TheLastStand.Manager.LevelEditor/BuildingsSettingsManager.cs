using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.EventSystem;
using TheLastStand.Model.Building;
using TheLastStand.Model.Events;
using TheLastStand.Model.LevelEditor.Conversation;
using TheLastStand.View.LevelEditor;
using UnityEngine;

namespace TheLastStand.Manager.LevelEditor;

public class BuildingsSettingsManager : Manager<BuildingsSettingsManager>
{
	[SerializeField]
	private BuildingSettingsPanel buildingSettingsPanelPrefab;

	public Dictionary<TheLastStand.Model.Building.Building, BuildingSettingsPanel> BuildingSettingsPanels { get; private set; } = new Dictionary<TheLastStand.Model.Building.Building, BuildingSettingsPanel>();

	public void OnBuildingPlaced(TheLastStand.Model.Building.Building building, XContainer buildingContainer)
	{
		BuildingSettingsPanel buildingSettingsPanel = UnityEngine.Object.Instantiate(buildingSettingsPanelPrefab, base.transform);
		buildingSettingsPanel.Init(building, buildingContainer);
		BuildingSettingsPanels.Add(building, buildingSettingsPanel);
	}

	protected override void Awake()
	{
		base.Awake();
		TPSingleton<LevelEditorManager>.Instance.LevelEditorStateChanged += OnLevelEditorStateChanged;
		PlaceBuildingCommand.BuildingPlaced = (PlaceBuildingCommand.BuildingPlacedEventHandler)Delegate.Combine(PlaceBuildingCommand.BuildingPlaced, new PlaceBuildingCommand.BuildingPlacedEventHandler(OnBuildingPlaced));
		EventManager.AddListener(typeof(BuildingDestroyedEvent), OnBuildingDestroyed);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (TPSingleton<LevelEditorManager>.Exist())
		{
			TPSingleton<LevelEditorManager>.Instance.LevelEditorStateChanged -= OnLevelEditorStateChanged;
		}
		PlaceBuildingCommand.BuildingPlaced = (PlaceBuildingCommand.BuildingPlacedEventHandler)Delegate.Remove(PlaceBuildingCommand.BuildingPlaced, new PlaceBuildingCommand.BuildingPlacedEventHandler(OnBuildingPlaced));
	}

	private void DisplayAllSettingsPanelsButtons(bool show)
	{
		foreach (KeyValuePair<TheLastStand.Model.Building.Building, BuildingSettingsPanel> buildingSettingsPanel in BuildingSettingsPanels)
		{
			if (!show)
			{
				buildingSettingsPanel.Value.DisplayPanel(show: false);
			}
			buildingSettingsPanel.Value.gameObject.SetActive(show);
		}
	}

	private void OnBuildingDestroyed(TheLastStand.Framework.EventSystem.Event raisedEvent)
	{
		BuildingDestroyedEvent buildingDestroyedEvent = raisedEvent as BuildingDestroyedEvent;
		if (BuildingSettingsPanels.ContainsKey(buildingDestroyedEvent.Building))
		{
			UnityEngine.Object.Destroy(BuildingSettingsPanels[buildingDestroyedEvent.Building].gameObject);
			BuildingSettingsPanels.Remove(buildingDestroyedEvent.Building);
		}
	}

	private void OnBuildingPlaced(TheLastStand.Model.Building.Building building)
	{
		if (ShouldCreateSettingsPanel(building))
		{
			BuildingSettingsPanel buildingSettingsPanel = UnityEngine.Object.Instantiate(buildingSettingsPanelPrefab, base.transform);
			buildingSettingsPanel.Init(building);
			BuildingSettingsPanels.Add(building, buildingSettingsPanel);
		}
	}

	private void OnLevelEditorStateChanged(LevelEditorManager.E_State previousState, LevelEditorManager.E_State newState)
	{
		DisplayAllSettingsPanelsButtons(newState == LevelEditorManager.E_State.SelectBuilding);
	}

	private bool ShouldCreateSettingsPanel(TheLastStand.Model.Building.Building building)
	{
		if (building.BlueprintModule.IsIndestructible)
		{
			return building.UpgradeModule != null;
		}
		return true;
	}
}
