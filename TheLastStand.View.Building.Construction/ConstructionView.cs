using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Meta;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework;
using TheLastStand.Framework.EventSystem;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Events;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.View.HUD;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.Construction;

public class ConstructionView : TPSingleton<ConstructionView>
{
	[SerializeField]
	private GameObject constructionTitleLayout;

	[SerializeField]
	private TextMeshProUGUI constructionTitleText;

	[SerializeField]
	private ToggleGroup repairDestroyToggleGroup;

	[SerializeField]
	private ConstructionStateButton destroyButton;

	[SerializeField]
	private ConstructionStateButton repairButton;

	[SerializeField]
	private ConstructionStateButton structuresButton;

	[SerializeField]
	private ConstructionStateButton defensesButton;

	[SerializeField]
	private GameObject constructionPanel;

	[SerializeField]
	private RectTransform constructiblesPanelTransform;

	[SerializeField]
	private RectTransform constructiblesListTransform;

	[SerializeField]
	private RectTransform constructiblesListViewport;

	[SerializeField]
	private RectTransform constructiblesListParent;

	[SerializeField]
	private ScrollRect constructiblesListScrollView;

	[SerializeField]
	private GameObject constructibleButtonPrefab;

	[SerializeField]
	private TextMeshProUGUI targetRepairTitleText;

	[SerializeField]
	private TextMeshProUGUI repairAllTitleText;

	[SerializeField]
	private GameObject repairPanel;

	[SerializeField]
	private RepairModeButton repairTargetButton;

	[SerializeField]
	private RepairModeButton repairIdButton;

	[SerializeField]
	private RepairCategoryButton repairCategoryButtonPrefab;

	[SerializeField]
	private RectTransform repairCategoryButtonsParent;

	[SerializeField]
	private ScrollRect reparationScrollView;

	[SerializeField]
	private GameObject repairCategoryPanel;

	[SerializeField]
	private GameObject demolishPanel;

	[SerializeField]
	private DestroyModeButton destroyTargetButton;

	[SerializeField]
	private ScrollRect destroyScrollView;

	[SerializeField]
	private JoystickSkillBar joystickSkillBar;

	private List<BuildingButton> productionBuildingsButtonsList = new List<BuildingButton>();

	private List<BuildingButton> defensiveBuildingsButtonsList = new List<BuildingButton>();

	private List<RepairCategoryButton> repairCategoryButtons = new List<RepairCategoryButton>();

	private Canvas canvas;

	private bool initialized;

	public TheLastStand.Model.Building.Construction Construction { get; set; }

	public BuildingDefinition.E_ConstructionCategory CurrentlyDisplayedCategory { get; private set; }

	public BuildingButton HoveredBuildingButton { get; set; }

	public ConstructionModeButton HoveredConstructionModeButton { get; set; }

	public RepairCategoryButton HoveredRepairCategoryButton { get; set; }

	public JoystickSkillBar JoystickSkillBar => joystickSkillBar;

	public static void ClearRepairTilesFeedback()
	{
		TileMapView.BuildingSelectionFeedbackTilemap.ClearAllTiles();
		TileMapView.BuildingSelectionOutlineTilemap.ClearAllTiles();
	}

	public static void DisplayTilesFeedback(TheLastStand.Model.Building.Building building)
	{
		TPSingleton<TileMapView>.Instance.DisplayBuildingSelectionFeedback(building, show: true);
	}

	public static void OnBuildingButtonClick(BuildingDefinition buildingDefinition)
	{
		TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace = buildingDefinition;
		ConstructionManager.SetState(TheLastStand.Model.Building.Construction.E_State.PlaceBuilding);
		TPSingleton<GameManager>.Instance.Game.Cursor.BuildingToFit = TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace;
	}

	public static void OnConstructionStateChange(TheLastStand.Model.Building.Construction.E_State newState, TheLastStand.Model.Building.Construction.E_State oldState)
	{
		if (newState == oldState)
		{
			return;
		}
		switch (oldState)
		{
		case TheLastStand.Model.Building.Construction.E_State.ChooseBuilding:
			TPSingleton<ConstructionView>.Instance.constructionPanel.SetActive(value: false);
			break;
		case TheLastStand.Model.Building.Construction.E_State.PlaceBuilding:
		{
			GameView.BottomScreenPanel.BottomLeftPanel.RefreshConstructionBackground();
			TPSingleton<ConstructionView>.Instance.repairDestroyToggleGroup.gameObject.SetActive(value: true);
			TPSingleton<ConstructionView>.Instance.constructionTitleLayout.SetActive(value: true);
			for (int i = 0; i < TPSingleton<TileMapManager>.Instance.TileMap.Width; i++)
			{
				for (int j = 0; j < TPSingleton<TileMapManager>.Instance.TileMap.Height; j++)
				{
					TileMapView.SetTile(TileMapView.OccupationVolumeBuildingTilemap, TileMapManager.GetTile(i, j));
				}
			}
			break;
		}
		case TheLastStand.Model.Building.Construction.E_State.Repair:
			TPSingleton<ConstructionView>.Instance.repairPanel.SetActive(value: false);
			break;
		case TheLastStand.Model.Building.Construction.E_State.Destroy:
			TPSingleton<ConstructionView>.Instance.demolishPanel.SetActive(value: false);
			break;
		case TheLastStand.Model.Building.Construction.E_State.None:
			TPSingleton<ConstructionView>.Instance.constructionTitleText.text = Localizer.Get("ConstructionPanel_Title");
			break;
		}
		switch (newState)
		{
		case TheLastStand.Model.Building.Construction.E_State.None:
			TPSingleton<ConstructionView>.Instance.canvas.enabled = false;
			TPSingleton<ConstructionView>.Instance.constructiblesListScrollView.horizontalNormalizedPosition = 0f;
			TPSingleton<ConstructionView>.Instance.reparationScrollView.horizontalNormalizedPosition = 0f;
			TPSingleton<ConstructionView>.Instance.destroyScrollView.horizontalNormalizedPosition = 0f;
			TPSingleton<ConstructionView>.Instance.constructiblesListScrollView.enabled = false;
			TPSingleton<ConstructionView>.Instance.reparationScrollView.enabled = false;
			TPSingleton<ConstructionView>.Instance.destroyScrollView.enabled = false;
			ClearRepairTilesFeedback();
			break;
		case TheLastStand.Model.Building.Construction.E_State.ChooseBuilding:
			TPSingleton<ConstructionView>.Instance.canvas.enabled = true;
			TPSingleton<ConstructionView>.Instance.constructionPanel.SetActive(value: true);
			TPSingleton<ConstructionView>.Instance.constructiblesListScrollView.enabled = true;
			TPSingleton<ConstructionView>.Instance.RefreshJoystickSkills();
			if (InputManager.IsLastControllerJoystick)
			{
				if (InputManager.JoystickConfig.HUDNavigation.SelectBuildingButtonAfterConstruction)
				{
					TPSingleton<ConstructionView>.Instance.SelectPreviousBuildingButton();
				}
				else
				{
					TPSingleton<ConstructionView>.Instance.SelectFirstBuildingButton();
				}
			}
			if (oldState == TheLastStand.Model.Building.Construction.E_State.ChooseBuilding || oldState == TheLastStand.Model.Building.Construction.E_State.None)
			{
				Vector2 anchoredPosition = TPSingleton<ConstructionView>.Instance.constructiblesPanelTransform.anchoredPosition;
				anchoredPosition.x = 0f;
				TPSingleton<ConstructionView>.Instance.constructiblesPanelTransform.anchoredPosition = anchoredPosition;
			}
			break;
		case TheLastStand.Model.Building.Construction.E_State.PlaceBuilding:
			GameView.BottomScreenPanel.BottomLeftPanel.RefreshConstructionBackground();
			TPSingleton<ConstructionView>.Instance.repairDestroyToggleGroup.gameObject.SetActive(value: false);
			TPSingleton<ConstructionView>.Instance.constructionTitleLayout.SetActive(value: false);
			TPSingleton<ConstructionView>.Instance.RefreshOccupationBuildingArea();
			TPSingleton<ConstructionManager>.Instance.DisplayBuildingGhost();
			TPSingleton<ConstructionManager>.Instance.IsConstructionButtonHovered = true;
			TPSingleton<ConstructionView>.Instance.JoystickSkillBar.DeselectCurrentSkill();
			break;
		case TheLastStand.Model.Building.Construction.E_State.Repair:
			TPSingleton<ConstructionView>.Instance.targetRepairTitleText.text = Localizer.Get("ConstructionPanel_RepairTargetTitle");
			TPSingleton<ConstructionView>.Instance.repairAllTitleText.text = Localizer.Get("ConstructionPanel_RepairCategoryTitle");
			TPSingleton<ConstructionView>.Instance.CurrentlyDisplayedCategory = BuildingDefinition.E_ConstructionCategory.None;
			TPSingleton<ConstructionView>.Instance.RefreshRepairButtons();
			TPSingleton<ConstructionView>.Instance.reparationScrollView.enabled = true;
			TPSingleton<ConstructionView>.Instance.repairPanel.SetActive(value: true);
			break;
		case TheLastStand.Model.Building.Construction.E_State.Destroy:
			TPSingleton<ConstructionView>.Instance.CurrentlyDisplayedCategory = BuildingDefinition.E_ConstructionCategory.None;
			TPSingleton<ConstructionView>.Instance.destroyScrollView.enabled = true;
			TPSingleton<ConstructionView>.Instance.demolishPanel.SetActive(value: true);
			break;
		}
		BuildingManager.BuildingRepairTooltip.Hide();
		TPSingleton<ConstructionView>.Instance.RefreshStateButtons();
	}

	public void AdjustScrollbar(RectTransform item)
	{
		GUIHelpers.AdjustHorizontalScrollViewToFocusedItem(item, constructiblesListViewport, constructiblesListParent, 0.01f);
	}

	public void ChangeBuildingListContent(BuildingDefinition.E_ConstructionCategory buildingCategory = BuildingDefinition.E_ConstructionCategory.All)
	{
		CurrentlyDisplayedCategory = buildingCategory;
		if (productionBuildingsButtonsList == null || defensiveBuildingsButtonsList == null)
		{
			return;
		}
		JoystickSkillBar.DeselectCurrentSkill();
		List<UnlockBuildingMetaEffectDefinition> list = null;
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockBuildingMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			list = effects.ToList();
		}
		bool debugForceConstructionAllowed = ConstructionManager.DebugForceConstructionAllowed;
		for (int i = 0; i < productionBuildingsButtonsList.Count; i++)
		{
			string buildingId = productionBuildingsButtonsList[i].BuildingDefinition.Id;
			productionBuildingsButtonsList[i].Display(debugForceConstructionAllowed || (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production && buildingCategory.HasFlag(BuildingDefinition.E_ConstructionCategory.Production) && (MetaUpgradesManager.IsThisBuildingUnlockedByDefault(buildingId) || (list?.Any((UnlockBuildingMetaEffectDefinition x) => x.BuildingId == buildingId) ?? false))));
		}
		for (int num = 0; num < defensiveBuildingsButtonsList.Count; num++)
		{
			string buildingId2 = defensiveBuildingsButtonsList[num].BuildingDefinition.Id;
			defensiveBuildingsButtonsList[num].Display(debugForceConstructionAllowed || (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production && buildingCategory.HasFlag(BuildingDefinition.E_ConstructionCategory.Defensive) && (MetaUpgradesManager.IsThisBuildingUnlockedByDefault(buildingId2) || (list?.Any((UnlockBuildingMetaEffectDefinition x) => x.BuildingId == buildingId2) ?? false))));
		}
		switch (buildingCategory)
		{
		case BuildingDefinition.E_ConstructionCategory.Defensive:
			TPSingleton<ConstructionView>.Instance.defensesButton.Toggle(state: true);
			break;
		case BuildingDefinition.E_ConstructionCategory.Production:
			TPSingleton<ConstructionView>.Instance.structuresButton.Toggle(state: true);
			break;
		}
		Refresh();
		RefreshStateButtons();
		LayoutRebuilder.ForceRebuildLayoutImmediate(constructiblesListTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(constructiblesPanelTransform);
	}

	public void CreateBuildingGameObjectsList()
	{
		productionBuildingsButtonsList.Clear();
		if (Construction == null)
		{
			Construction = TPSingleton<ConstructionManager>.Instance.Construction;
		}
		for (int i = 0; i < Construction.AvailableBuildings.Count; i++)
		{
			BuildingDefinition buildingDefinition = Construction.AvailableBuildings[i];
			BuildingButton component = Object.Instantiate(TPSingleton<ConstructionView>.Instance.constructibleButtonPrefab, TPSingleton<ConstructionView>.Instance.constructiblesListTransform, worldPositionStays: false).GetComponent<BuildingButton>();
			component.BuildingDefinition = buildingDefinition;
			if (buildingDefinition.ConstructionModuleDefinition.NativeMaterialsCost > 0)
			{
				defensiveBuildingsButtonsList.Add(component);
			}
			else
			{
				productionBuildingsButtonsList.Add(component);
			}
		}
		Refresh();
	}

	public void Init()
	{
		if (initialized)
		{
			return;
		}
		Awake();
		Construction = TPSingleton<ConstructionManager>.Instance.Construction;
		canvas = GetComponent<Canvas>();
		canvas.enabled = false;
		TPSingleton<ResourceManager>.Instance.OnGoldChange += OnGoldChanged;
		EventManager.AddListener(typeof(MaterialChangeEvent), OnMaterialChanged);
		EventManager.AddListener(typeof(BuildingConstructedEvent), OnConstructionAvailabilityChanged);
		EventManager.AddListener(typeof(BuildingDestroyedEvent), OnConstructionAvailabilityChanged);
		destroyButton.Init(OnDestroyToggleValueChanged);
		repairButton.Init(OnRepairToggleValueChanged);
		structuresButton.Init(OnStructuresToggleValueChanged);
		defensesButton.Init(OnDefensesToggleValueChanged);
		repairTargetButton.Init(OnRepairTargetButtonClicked);
		repairIdButton.Init(OnRepairIdButtonClicked);
		destroyTargetButton.Init(OnDestroyTargetButtonClicked);
		foreach (KeyValuePair<string, List<BuildingDefinition.E_BuildingCategory>> repairCategoryButton in ConstructionDatabase.ConstructionDefinition.RepairCategoryButtons)
		{
			RepairCategoryButton repairCategoryButtonInstance = Object.Instantiate(repairCategoryButtonPrefab, repairCategoryButtonsParent);
			repairCategoryButtonInstance.Init(delegate
			{
				OnRepairCategoryButtonClicked(repairCategoryButtonInstance);
			}, repairCategoryButton.Key, repairCategoryButton.Value);
			repairCategoryButtons.Add(repairCategoryButtonInstance);
			repairCategoryButtonInstance.RefreshDisplay();
		}
		initialized = true;
		repairPanel.SetActive(value: false);
		demolishPanel.SetActive(value: false);
		base.gameObject.SetActive(value: false);
	}

	public bool IsRepairCategoryButtonAvailable(List<BuildingDefinition.E_BuildingCategory> categories)
	{
		foreach (ConstructionModule item in TPSingleton<BuildingManager>.Instance.Buildings.Select((TheLastStand.Model.Building.Building o) => o.ConstructionModule))
		{
			if (!item.ConstructionModuleDefinition.IsRepairable)
			{
				continue;
			}
			foreach (BuildingDefinition.E_BuildingCategory category in categories)
			{
				if (item.BuildingParent.BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(category))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RefreshOccupationBuildingArea()
	{
		Construction.BuildingAvailableSpaceTiles.Clear();
		if (!Construction.BuildingToPlace.ConstructionModuleDefinition.ShouldDisplayConstructionTileFeedback)
		{
			for (int i = 0; i < TPSingleton<TileMapManager>.Instance.TileMap.Width; i++)
			{
				for (int j = 0; j < TPSingleton<TileMapManager>.Instance.TileMap.Height; j++)
				{
					if (TPSingleton<TileMapManager>.Instance.TileMap.GetTile(i, j).ForbiddenBuildingFromCategory.Contains(Construction.BuildingToPlace.BlueprintModuleDefinition.Category))
					{
						TileMapView.SetTile(TileMapView.OccupationVolumeBuildingTilemap, TileMapManager.GetTile(i, j), "View/Tiles/Feedbacks/Forbidden Building");
					}
				}
			}
			return;
		}
		for (int k = 0; k < TPSingleton<TileMapManager>.Instance.TileMap.Width; k++)
		{
			for (int l = 0; l < TPSingleton<TileMapManager>.Instance.TileMap.Height; l++)
			{
				bool flag = false;
				if (!TileMapController.IsTileValidForBuilding(Construction.BuildingToPlace, TileMapManager.GetTile(k, l), Tile.E_UnitAccess.Blocked, ignoreUnit: true))
				{
					continue;
				}
				if (Construction.BuildingToPlace.ConstructionModuleDefinition.OccupationVolumeType == BuildingDefinition.E_OccupationVolumeType.Adjacent)
				{
					int buildingDeadZoneRange = BuildingManager.GetBuildingDeadZoneRange(Construction.BuildingToPlace.Id);
					for (int m = -buildingDeadZoneRange; m <= buildingDeadZoneRange; m++)
					{
						for (int n = -buildingDeadZoneRange; n <= buildingDeadZoneRange; n++)
						{
							if (m != 0 || n != 0)
							{
								Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(TileMapManager.GetTile(k, l).Position.x + m, TileMapManager.GetTile(k, l).Position.y + n);
								if (tile?.Building != null && tile.Building.BuildingDefinition.ConstructionModuleDefinition.OccupationVolumeType != BuildingDefinition.E_OccupationVolumeType.Ignore)
								{
									flag = true;
									break;
								}
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
				if (!flag && !Construction.BuildingAvailableSpaceTiles.Contains(TileMapManager.GetTile(k, l)))
				{
					Construction.BuildingAvailableSpaceTiles.Add(TileMapManager.GetTile(k, l));
					TileMapView.SetTile(TileMapView.OccupationVolumeBuildingTilemap, TileMapManager.GetTile(k, l), "View/Tiles/Feedbacks/Occupation Volume");
				}
			}
		}
	}

	public void RemoveAdjacentAvailableSpaceTile(BuildingDefinition buildingDefinition, Tile originTile)
	{
		List<Tile> occupiedTiles = originTile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition);
		for (int i = 0; i < occupiedTiles.Count; i++)
		{
			Construction.BuildingAvailableSpaceTiles.Remove(occupiedTiles[i]);
			TileMapView.SetTile(TileMapView.OccupationVolumeBuildingTilemap, occupiedTiles[i]);
			if (buildingDefinition.ConstructionModuleDefinition.OccupationVolumeType != BuildingDefinition.E_OccupationVolumeType.Adjacent)
			{
				continue;
			}
			int buildingDeadZoneRange = BuildingManager.GetBuildingDeadZoneRange(buildingDefinition.Id);
			for (int j = -buildingDeadZoneRange; j <= buildingDeadZoneRange; j++)
			{
				for (int k = -buildingDeadZoneRange; k <= buildingDeadZoneRange; k++)
				{
					Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(occupiedTiles[i].Position.x + j, occupiedTiles[i].Position.y + k);
					if (!occupiedTiles.Contains(tile) && Construction.BuildingAvailableSpaceTiles.Contains(tile))
					{
						Construction.BuildingAvailableSpaceTiles.Remove(tile);
						TileMapView.SetTile(TileMapView.OccupationVolumeBuildingTilemap, tile);
					}
				}
			}
		}
	}

	public void SelectFirstBuildingButton()
	{
		joystickSkillBar.SelectFirstSkill();
	}

	public void SelectNextBuildingButton(bool next)
	{
		joystickSkillBar.SelectNextSkill(next, 0);
	}

	public void SelectPreviousBuildingButton()
	{
		joystickSkillBar.SelectCurrentOrPreviousSkill();
	}

	public void RefreshJoystickSkills()
	{
		joystickSkillBar.Clear();
		switch (CurrentlyDisplayedCategory)
		{
		case BuildingDefinition.E_ConstructionCategory.Defensive:
			joystickSkillBar.Register(defensiveBuildingsButtonsList.Where((BuildingButton button) => button.isActiveAndEnabled));
			break;
		case BuildingDefinition.E_ConstructionCategory.Production:
			joystickSkillBar.Register(productionBuildingsButtonsList.Where((BuildingButton button) => button.isActiveAndEnabled));
			break;
		case BuildingDefinition.E_ConstructionCategory.None:
			if (TPSingleton<ConstructionManager>.Instance.Construction.State == TheLastStand.Model.Building.Construction.E_State.Repair)
			{
				joystickSkillBar.Register(repairTargetButton);
				joystickSkillBar.Register(repairIdButton);
				joystickSkillBar.Register(repairCategoryButtons.Where((RepairCategoryButton button) => button.isActiveAndEnabled));
			}
			else
			{
				joystickSkillBar.Register(destroyTargetButton);
			}
			break;
		}
	}

	private void RefreshStateButtons()
	{
		repairButton.Refresh();
		destroyButton.Refresh();
		structuresButton.Refresh();
		defensesButton.Refresh();
	}

	private void OnConstructionAvailabilityChanged(TheLastStand.Framework.EventSystem.Event e)
	{
		Refresh();
	}

	private void OnDestroy()
	{
		if (TPSingleton<ResourceManager>.Exist())
		{
			TPSingleton<ResourceManager>.Instance.OnGoldChange -= OnGoldChanged;
		}
		if (SingletonBehaviour<EventManager>.Instance != null)
		{
			EventManager.RemoveListener(typeof(MaterialChangeEvent), OnMaterialChanged);
			EventManager.RemoveListener(typeof(BuildingConstructedEvent), OnConstructionAvailabilityChanged);
			EventManager.RemoveListener(typeof(BuildingDestroyedEvent), OnConstructionAvailabilityChanged);
		}
	}

	private void OnDestroyToggleValueChanged()
	{
		ConstructionManager.SetState(TheLastStand.Model.Building.Construction.E_State.Destroy);
	}

	private void OnRepairToggleValueChanged()
	{
		ConstructionManager.SetState(TheLastStand.Model.Building.Construction.E_State.Repair);
	}

	private void OnStructuresToggleValueChanged()
	{
		ConstructionManager.OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Production);
	}

	private void OnDefensesToggleValueChanged()
	{
		ConstructionManager.OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Defensive);
	}

	private void OnGoldChanged(int gold)
	{
		Refresh();
		RefreshRepairButtons();
	}

	private void OnMaterialChanged(TheLastStand.Framework.EventSystem.Event e)
	{
		Refresh();
		RefreshRepairButtons();
	}

	private void OnDestroyTargetButtonClicked()
	{
		if (Construction.DestroyMode != TheLastStand.Model.Building.Construction.E_DestroyMode.Target)
		{
			Construction.DestroyMode = TheLastStand.Model.Building.Construction.E_DestroyMode.Target;
		}
	}

	private void OnRepairTargetButtonClicked()
	{
		if (Construction.RepairMode != TheLastStand.Model.Building.Construction.E_RepairMode.Target)
		{
			Construction.RepairMode = TheLastStand.Model.Building.Construction.E_RepairMode.Target;
		}
	}

	private void OnRepairIdButtonClicked()
	{
		if (Construction.RepairMode != TheLastStand.Model.Building.Construction.E_RepairMode.Id)
		{
			Construction.RepairMode = TheLastStand.Model.Building.Construction.E_RepairMode.Id;
		}
	}

	private void OnRepairCategoryButtonClicked(RepairCategoryButton repairCategoryButton)
	{
		ConstructionManager.RepairBuildingsOfCategory(repairCategoryButton.BuildingCategories);
	}

	public void RefreshRepairButtons()
	{
		TheLastStand.Model.Building.Construction.E_UnusableActionCause unusableActionCause;
		bool flag = Construction.AnyTargetReparationAffordable(out unusableActionCause);
		TheLastStand.Model.Building.Construction.E_UnusableActionCause unusableActionCause2;
		bool flag2 = Construction.AnyIdReparationAffordable(out unusableActionCause2);
		repairTargetButton.SetInteractable(flag, unusableActionCause);
		repairIdButton.SetInteractable(flag2, unusableActionCause2);
		switch (Construction.RepairMode)
		{
		case TheLastStand.Model.Building.Construction.E_RepairMode.Id:
			if (!flag2)
			{
				Construction.RepairMode = TheLastStand.Model.Building.Construction.E_RepairMode.None;
			}
			break;
		case TheLastStand.Model.Building.Construction.E_RepairMode.Target:
			if (!flag)
			{
				Construction.RepairMode = TheLastStand.Model.Building.Construction.E_RepairMode.None;
			}
			break;
		}
		for (int num = repairCategoryButtons.Count - 1; num >= 0; num--)
		{
			repairCategoryButtons[num].RefreshDisplay();
			repairCategoryButtons[num].RefreshAvailability();
		}
		repairCategoryPanel.SetActive(repairCategoryButtons.Any((RepairCategoryButton o) => o.gameObject.activeSelf));
	}

	private void Refresh()
	{
		productionBuildingsButtonsList.ForEach(delegate(BuildingButton o)
		{
			o.Refresh();
		});
		defensiveBuildingsButtonsList.ForEach(delegate(BuildingButton o)
		{
			o.Refresh();
		});
	}
}
