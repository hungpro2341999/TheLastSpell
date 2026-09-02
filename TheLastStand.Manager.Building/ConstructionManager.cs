using System.Collections;
using System.Collections.Generic;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Framework;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Sound;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization.Building;
using TheLastStand.View;
using TheLastStand.View.Building.Construction;
using TheLastStand.View.Building.UI;
using TheLastStand.View.Cursor;
using TheLastStand.View.Skill.SkillAction;
using TheLastStand.View.Skill.SkillAction.UI;
using TheLastStand.View.TileMap;
using TheLastStand.View.ToDoList;
using UnityEngine;

namespace TheLastStand.Manager.Building;

public class ConstructionManager : Manager<ConstructionManager>
{
	[SerializeField]
	private DestroyBuildingFeedback destroyBuildingFeedback;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioClip constructionCancelAudioClip;

	private bool initialized;

	private Dictionary<string, int> instantProductionBonus = new Dictionary<string, int>();

	[SerializeField]
	private bool debugForceConstructionAllowed;

	public Construction Construction { get; private set; }

	public TilesInRangeInfos LineOfSightTiles { get; } = new TilesInRangeInfos();

	public bool IsConstructionButtonHovered { get; set; }

	public static bool DebugForceConstructionAllowed => TPSingleton<ConstructionManager>.Instance.debugForceConstructionAllowed;

	public static bool CanRepairBuilding(ConstructionModule buildingConstructionModule)
	{
		if (!buildingConstructionModule.NeedRepair)
		{
			return false;
		}
		int repairCost = buildingConstructionModule.RepairCost;
		if (!buildingConstructionModule.CostsGold)
		{
			return TPSingleton<ResourceManager>.Instance.Materials >= repairCost;
		}
		return TPSingleton<ResourceManager>.Instance.Gold >= repairCost;
	}

	public static void RepairBuilding(TheLastStand.Model.Building.Building building, bool addEffectDisplays = true, bool displayEffects = true)
	{
		if (building.ConstructionModule.CostsGold)
		{
			int repairCost = building.ConstructionModule.RepairCost;
			TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold - repairCost);
			if (addEffectDisplays)
			{
				GainGoldDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainGoldDisplay", ResourcePooler.LoadOnce<GainGoldDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainGoldDisplay"), EffectManager.EffectDisplaysParent);
				pooledComponent.Init(-repairCost);
				building.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
			}
		}
		else if (building.ConstructionModule.CostsMaterials)
		{
			int repairCost2 = building.ConstructionModule.RepairCost;
			TPSingleton<ResourceManager>.Instance.Materials -= repairCost2;
			if (addEffectDisplays)
			{
				GainMaterialDisplay pooledComponent2 = ObjectPooler.GetPooledComponent("GainMaterialDisplay", ResourcePooler.LoadOnce<GainMaterialDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainMaterialDisplay"), EffectManager.EffectDisplaysParent);
				pooledComponent2.Init(-repairCost2);
				building.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent2);
			}
		}
		float healAmount = building.BuildingController.DamageableModuleController.Repair();
		if (building.IsTrap)
		{
			building.BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: false);
		}
		else if (addEffectDisplays)
		{
			HealFeedback healFeedback = building.DamageableModule.DamageableView.HealFeedback;
			healFeedback.AddHealInstance(healAmount, building.DamageableModule.Health);
			building.BuildingController.BlueprintModuleController.AddEffectDisplay(healFeedback);
		}
		if (displayEffects)
		{
			EffectManager.DisplayEffects();
		}
		TPSingleton<ConstructionView>.Instance.RefreshRepairButtons();
		RepairBuildingFeedback.IsDirty = true;
	}

	public static void RepairBuildingsOfCategory(List<BuildingDefinition.E_BuildingCategory> buildingCategories)
	{
		for (int num = TPSingleton<BuildingManager>.Instance.Buildings.Count - 1; num >= 0; num--)
		{
			TheLastStand.Model.Building.Building building = TPSingleton<BuildingManager>.Instance.Buildings[num];
			if (building.ConstructionModule.NeedRepair)
			{
				foreach (BuildingDefinition.E_BuildingCategory buildingCategory in buildingCategories)
				{
					if (building.BlueprintModule.BlueprintModuleDefinition.Category.HasFlag(buildingCategory))
					{
						RepairBuilding(building, addEffectDisplays: true, displayEffects: false);
						break;
					}
				}
			}
		}
		EffectManager.DisplayEffects();
		ConstructionView.ClearRepairTilesFeedback();
	}

	public static int ComputeCategoriesRepairCost(List<BuildingDefinition.E_BuildingCategory> buildingCategories)
	{
		int num = 0;
		for (int num2 = TPSingleton<BuildingManager>.Instance.Buildings.Count - 1; num2 >= 0; num2--)
		{
			TheLastStand.Model.Building.Building building = TPSingleton<BuildingManager>.Instance.Buildings[num2];
			foreach (BuildingDefinition.E_BuildingCategory buildingCategory in buildingCategories)
			{
				if (building.BlueprintModule.BlueprintModuleDefinition.Category.HasFlag(buildingCategory) && building.ConstructionModule.NeedRepair)
				{
					num += building.ConstructionModule.RepairCost;
					break;
				}
			}
		}
		return num;
	}

	public static void ExitConstructionMode()
	{
		if (GameController.CanExitConstructionMode())
		{
			TPSingleton<ConstructionView>.Instance.JoystickSkillBar.DeselectCurrentSkill(deselectConfirmAndSkill: true);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			GameController.SetState(Game.E_State.Management);
		}
	}

	public static void OnGameStateChange(Game.E_State state)
	{
		if (state == Game.E_State.Construction)
		{
			SetState(Construction.E_State.ChooseBuilding);
		}
		else
		{
			SetState(Construction.E_State.None);
		}
	}

	public static void OpenConstructionMode(BuildingDefinition.E_ConstructionCategory buildingCategory = BuildingDefinition.E_ConstructionCategory.All)
	{
		if (!GameController.CanOpenConstructionMode(buildingCategory))
		{
			ExitConstructionMode();
			return;
		}
		ConstructionView instance = TPSingleton<ConstructionView>.Instance;
		if (instance != null)
		{
			instance.ChangeBuildingListContent(buildingCategory);
		}
		GameController.SetState(Game.E_State.Construction);
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			TPSingleton<ConstructionView>.Instance.SelectFirstBuildingButton();
			TPSingleton<ConstructionManager>.Instance.StartCoroutine(SelectFirstBuilding());
		}
	}

	public static void SetState(Construction.E_State newState)
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State == newState)
		{
			return;
		}
		Construction.E_State state = TPSingleton<ConstructionManager>.Instance.Construction.State;
		switch (newState)
		{
		case Construction.E_State.Repair:
			TPSingleton<ConstructionManager>.Instance.Construction.RepairMode = Construction.E_RepairMode.Target;
			break;
		case Construction.E_State.Destroy:
			TPSingleton<ConstructionManager>.Instance.Construction.DestroyMode = Construction.E_DestroyMode.Target;
			break;
		case Construction.E_State.PlaceBuilding:
			TPSingleton<TileMapView>.Instance.StartGhostTilemapsAlphaTweening();
			break;
		}
		switch (state)
		{
		case Construction.E_State.PlaceBuilding:
			SoundManager.PlayAudioClip(TPSingleton<ConstructionManager>.Instance.audioSource, TPSingleton<ConstructionManager>.Instance.constructionCancelAudioClip);
			if (TPSingleton<ConstructionManager>.Instance.Construction.GhostedTile != null)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.ClearBuildingGhost(TPSingleton<ConstructionManager>.Instance.Construction.GhostedTile, TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace);
				TPSingleton<TileMapView>.Instance.EndGhostAlphaTilemapsTweening();
				TPSingleton<ConstructionManager>.Instance.Construction.GhostedTile = null;
			}
			TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace = null;
			CursorView.ClearTiles();
			TPSingleton<GameManager>.Instance.Game.Cursor.BuildingToFit = null;
			TPSingleton<GameManager>.Instance.Game.Cursor.CursorState = TheLastStand.Model.Cursor.E_CursorState.Standard;
			break;
		case Construction.E_State.Repair:
			TPSingleton<ConstructionManager>.Instance.Construction.RepairMode = Construction.E_RepairMode.None;
			break;
		case Construction.E_State.Destroy:
			TPSingleton<ConstructionManager>.Instance.Construction.DestroyMode = Construction.E_DestroyMode.None;
			TPSingleton<ConstructionManager>.Instance.destroyBuildingFeedback.Toggle(toggle: false);
			break;
		}
		TPSingleton<ConstructionManager>.Instance.Construction.State = newState;
		if (TPSingleton<ConstructionManager>.Instance.Construction.State == Construction.E_State.Destroy)
		{
			TPSingleton<ConstructionManager>.Instance.destroyBuildingFeedback.Toggle(toggle: true);
		}
		ConstructionView.OnConstructionStateChange(TPSingleton<ConstructionManager>.Instance.Construction.State, state);
		GameView.TopScreenPanel.TurnPanel.Refresh();
	}

	public void DecrementBuildingCount(BuildingDefinition building)
	{
		if (!Construction.BuildingCount.TryGetValue(building.Id, out var _))
		{
			TPSingleton<ConstructionManager>.Instance.LogError("Trying to decrement a building count while it's currently set to 0.");
			return;
		}
		Construction.BuildingCount[building.Id]--;
		if (Construction.BuildingCount[building.Id] == 0)
		{
			Construction.BuildingCount.Remove(building.Id);
		}
	}

	public void DecrementInstantProductionBonus(TheLastStand.Model.Building.Building building)
	{
		instantProductionBonus[building.Id]--;
	}

	public void DisplayBuildingGhost()
	{
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		if (TPSingleton<ConstructionManager>.Instance.Construction.GhostedTile != null)
		{
			TPSingleton<TileMapView>.Instance.ClearBuildingGhost(TPSingleton<ConstructionManager>.Instance.Construction.GhostedTile, TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace);
			TPSingleton<ConstructionManager>.Instance.Construction.GhostedTile = null;
		}
		if (tile == null)
		{
			TPSingleton<GameManager>.Instance.Game.Cursor.CursorState = TheLastStand.Model.Cursor.E_CursorState.Invalid;
			return;
		}
		bool flag = TileMapController.CanPlaceBuilding(TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace, tile, ignoreUnit: true);
		TPSingleton<GameManager>.Instance.Game.Cursor.CursorState = (flag ? TheLastStand.Model.Cursor.E_CursorState.Valid : TheLastStand.Model.Cursor.E_CursorState.Invalid);
		TPSingleton<ConstructionManager>.Instance.Construction.GhostedTile = tile;
		TPSingleton<TileMapView>.Instance.DisplayBuildingGhost(TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace, TPSingleton<ConstructionManager>.Instance.Construction.GhostedTile);
		TPSingleton<TileMapView>.Instance.ChangeBuildingGhostTileMapsColor(flag);
	}

	public byte GetBuildingCount(ConstructionModuleDefinition buildingDefinitionConstructionModule)
	{
		if (BuildingDatabase.BuildingLimitGroupDefinitions.TryGetValue(buildingDefinitionConstructionModule.BuildLimitGroupId, out var value))
		{
			byte b = 0;
			{
				foreach (string buildingId in value.BuildingIds)
				{
					if (TPSingleton<ConstructionManager>.Instance.Construction.BuildingCount.TryGetValue(buildingId, out var value2))
					{
						b += value2;
					}
				}
				return b;
			}
		}
		if (Construction.BuildingCount.TryGetValue(buildingDefinitionConstructionModule.BuildingDefinition.Id, out var value3))
		{
			return value3;
		}
		return 0;
	}

	public bool HasInstantProductionBonusLeft(TheLastStand.Model.Building.Building building)
	{
		if (instantProductionBonus.TryGetValue(building.Id, out var value))
		{
			return value > 0;
		}
		return false;
	}

	public void IncrementBuildingCount(BuildingDefinition building)
	{
		if (Construction.BuildingCount.TryGetValue(building.Id, out var _))
		{
			Construction.BuildingCount[building.Id]++;
		}
		else
		{
			Construction.BuildingCount.Add(building.Id, 1);
		}
	}

	public void Init()
	{
		if (!initialized)
		{
			initialized = true;
			Awake();
			Construction = new Construction();
		}
	}

	public static bool IsUnderBuildLimit(ConstructionModuleDefinition buildingConstructionModuleDefinition)
	{
		if (buildingConstructionModuleDefinition.GetBuildLimit() < 0)
		{
			return true;
		}
		if (buildingConstructionModuleDefinition.GetBuildLimit() == 0)
		{
			return false;
		}
		if (BuildingDatabase.BuildingLimitGroupDefinitions.TryGetValue(buildingConstructionModuleDefinition.BuildLimitGroupId, out var value))
		{
			byte b = 0;
			foreach (string buildingId in value.BuildingIds)
			{
				if (TPSingleton<ConstructionManager>.Instance.Construction.BuildingCount.TryGetValue(buildingId, out var value2))
				{
					b += value2;
				}
			}
			return b < value.GetBuildLimit();
		}
		if (TPSingleton<ConstructionManager>.Instance.Construction.BuildingCount.TryGetValue(buildingConstructionModuleDefinition.BuildingDefinition.Id, out var value3))
		{
			return value3 < buildingConstructionModuleDefinition.GetBuildLimit();
		}
		return true;
	}

	private void Start()
	{
		CreateConstructionList();
		SetState(Construction.E_State.None);
		InitInstantProductionBonus();
	}

	private void Update()
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Construction)
		{
			return;
		}
		if (InputManager.GetButtonDown(96))
		{
			switch (TPSingleton<ConstructionView>.Instance.CurrentlyDisplayedCategory)
			{
			case BuildingDefinition.E_ConstructionCategory.Production:
				OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Defensive);
				break;
			case BuildingDefinition.E_ConstructionCategory.Defensive:
				SetState(Construction.E_State.Repair);
				break;
			case BuildingDefinition.E_ConstructionCategory.None:
				if (TPSingleton<ConstructionManager>.Instance.Construction.State == Construction.E_State.Repair)
				{
					SetState(Construction.E_State.Destroy);
				}
				else
				{
					OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Production);
				}
				break;
			}
			TPSingleton<ConstructionView>.Instance.RefreshJoystickSkills();
			TPSingleton<ConstructionView>.Instance.SelectFirstBuildingButton();
			return;
		}
		if (InputManager.GetButtonDown(95))
		{
			switch (TPSingleton<ConstructionView>.Instance.CurrentlyDisplayedCategory)
			{
			case BuildingDefinition.E_ConstructionCategory.Production:
				SetState(Construction.E_State.Destroy);
				break;
			case BuildingDefinition.E_ConstructionCategory.Defensive:
				OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Production);
				break;
			case BuildingDefinition.E_ConstructionCategory.None:
				if (TPSingleton<ConstructionManager>.Instance.Construction.State == Construction.E_State.Repair)
				{
					OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Defensive);
				}
				else
				{
					SetState(Construction.E_State.Repair);
				}
				break;
			}
			TPSingleton<ConstructionView>.Instance.RefreshJoystickSkills();
			TPSingleton<ConstructionView>.Instance.SelectFirstBuildingButton();
			return;
		}
		switch (TPSingleton<ConstructionManager>.Instance.Construction.State)
		{
		case Construction.E_State.PlaceBuilding:
			if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
			{
				SetState(Construction.E_State.ChooseBuilding);
			}
			else
			{
				UpdateBuildingPlacement();
			}
			break;
		case Construction.E_State.Repair:
			if (InputManager.GetButtonDown(24))
			{
				UpdateRepairOnClick(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
			}
			else if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
			{
				ExitConstructionMode();
				SoundManager.PlayAudioClip(TPSingleton<ConstructionManager>.Instance.audioSource, TPSingleton<ConstructionManager>.Instance.constructionCancelAudioClip);
			}
			else if (InputManager.GetButtonDown(83))
			{
				TPSingleton<ConstructionView>.Instance.SelectNextBuildingButton(next: false);
			}
			else if (InputManager.GetButtonDown(82))
			{
				TPSingleton<ConstructionView>.Instance.SelectNextBuildingButton(next: true);
			}
			break;
		default:
			if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
			{
				ExitConstructionMode();
				SoundManager.PlayAudioClip(TPSingleton<ConstructionManager>.Instance.audioSource, TPSingleton<ConstructionManager>.Instance.constructionCancelAudioClip);
			}
			else if (InputManager.GetButtonDown(83))
			{
				TPSingleton<ConstructionView>.Instance.SelectNextBuildingButton(next: false);
			}
			else if (InputManager.GetButtonDown(82))
			{
				TPSingleton<ConstructionView>.Instance.SelectNextBuildingButton(next: true);
			}
			break;
		}
	}

	private static void CreateConstructionList()
	{
		TPSingleton<ConstructionManager>.Instance.Construction.AvailableBuildings = new List<BuildingDefinition>();
		foreach (KeyValuePair<string, BuildingDefinition> buildingDefinition in BuildingDatabase.BuildingDefinitions)
		{
			BuildingDefinition value = buildingDefinition.Value;
			if (value.ConstructionModuleDefinition.IsBuyable)
			{
				TPSingleton<ConstructionManager>.Instance.Construction.AvailableBuildings.Add(value);
			}
		}
		if (TPSingleton<ConstructionView>.Exist())
		{
			TPSingleton<ConstructionView>.Instance.CreateBuildingGameObjectsList();
		}
	}

	private void InitInstantProductionBonus()
	{
		foreach (BuildingDefinition value in BuildingDatabase.BuildingDefinitions.Values)
		{
			if (value.UpgradeModuleDefinition != null && !instantProductionBonus.ContainsKey(value.Id))
			{
				instantProductionBonus.Add(value.Id, value.ConstructionModuleDefinition.GetBuildLimit());
			}
		}
	}

	private static void PlaceBuilding(Tile tile, int goldCost, int materialsCost)
	{
		ResourceManager instance = TPSingleton<ResourceManager>.Instance;
		bool flag = tile.Building != null && TileMapController.CanPlaceBuilding(TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace, tile, ignoreUnit: true);
		int materialsDiscount = ((flag && materialsCost > 0) ? tile.Building.ConstructionModule.Cost : 0);
		int goldDiscount = ((flag && goldCost > 0) ? tile.Building.ConstructionModule.Cost : 0);
		instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold - (goldCost - goldDiscount));
		instance.Materials -= materialsCost - materialsDiscount;
		if (flag)
		{
			BuildingManager.ReplaceBuilding(tile, tile.Building, TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace);
		}
		else
		{
			BuildingManager.CreateBuilding(TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace, tile, updateView: true, playSound: true, instantly: false);
		}
		TPSingleton<GameManager>.Instance.GameAnalytics.OnBuildingBuilt(tile.Building);
		TPSingleton<MetaConditionManager>.Instance.IncreaseBuildingsBuilt(tile.Building.BuildingDefinition);
		TPSingleton<TileMapView>.Instance.ChangeBuildingGhostTileMapsColor(isValid: false);
		TPSingleton<TileMapView>.Instance.ClearBuildingGhost(TPSingleton<ConstructionManager>.Instance.Construction.GhostedTile, TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace);
		TPSingleton<ConstructionView>.Instance.RemoveAdjacentAvailableSpaceTile(TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace, tile);
		TPSingleton<ConstructionView>.Instance.RefreshOccupationBuildingArea();
		tile.Building.BuildingView.OnFinalInitFrame = delegate
		{
			if (tile.Building != null)
			{
				if (materialsCost > 0)
				{
					GainMaterialDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainMaterialDisplay", ResourcePooler.LoadOnce<GainMaterialDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainMaterialDisplay"), EffectManager.EffectDisplaysParent);
					pooledComponent.Init(-(materialsCost - materialsDiscount));
					tile.Building.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
					EffectManager.DisplayEffects();
				}
				else if (goldCost > 0)
				{
					GainGoldDisplay pooledComponent2 = ObjectPooler.GetPooledComponent("GainGoldDisplay", ResourcePooler.LoadOnce<GainGoldDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainGoldDisplay"), EffectManager.EffectDisplaysParent);
					pooledComponent2.Init(-(goldCost - goldDiscount));
					tile.Building.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent2);
					EffectManager.DisplayEffects();
				}
			}
		};
		tile.Building.BuildingController.ProductionModuleController?.OnConstruction();
		if (instance.Gold < goldCost || instance.Materials < materialsCost || !IsUnderBuildLimit(TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace.ConstructionModuleDefinition))
		{
			TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.ClearBuildingGhost(TPSingleton<ConstructionManager>.Instance.Construction.GhostedTile, TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace);
			TPSingleton<ConstructionManager>.Instance.Construction.GhostedTile = null;
			TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace = null;
			TPSingleton<GameManager>.Instance.Game.Cursor.CursorState = TheLastStand.Model.Cursor.E_CursorState.Standard;
			CursorView.ClearTiles();
			TPSingleton<GameManager>.Instance.Game.Cursor.BuildingToFit = null;
			SetState(Construction.E_State.ChooseBuilding);
		}
		TPSingleton<ToDoListView>.Instance.RefreshWorkersNotification();
		TPSingleton<AchievementManager>.Instance.HandleBuiltBuilding(tile.Building.BuildingDefinition.Id);
	}

	private static IEnumerator SelectFirstBuilding()
	{
		yield return SharedYields.WaitForEndOfFrame;
		TPSingleton<ConstructionView>.Instance.SelectFirstBuildingButton();
	}

	private static void UpdateBuildingPlacement()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
		{
			TPSingleton<ConstructionManager>.Instance.DisplayBuildingGhost();
		}
		if (!InputManager.GetButtonDown(24))
		{
			return;
		}
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		if (TileMapController.CanPlaceBuilding(TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace, tile, ignoreUnit: true) && (!InputManager.IsLastControllerJoystick || (InputManager.IsLastControllerJoystick && (!TPSingleton<ConstructionManager>.Instance.IsConstructionButtonHovered || !InputManager.JoystickConfig.HUDNavigation.HoverBeforePlacingConstruction))))
		{
			int goldCost = 0;
			int materialsCost = 0;
			if (TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace.ConstructionModuleDefinition.NativeGoldCost > 0)
			{
				goldCost = BuildingManager.ComputeBuildingCost(TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace.ConstructionModuleDefinition);
			}
			else
			{
				materialsCost = BuildingManager.ComputeBuildingCost(TPSingleton<ConstructionManager>.Instance.Construction.BuildingToPlace.ConstructionModuleDefinition);
			}
			PlaceBuilding(tile, goldCost, materialsCost);
		}
		TPSingleton<ConstructionManager>.Instance.IsConstructionButtonHovered = false;
	}

	private void UpdateRepairOnClick(Tile clickedTile)
	{
		TheLastStand.Model.Building.Building building = clickedTile?.Building;
		if (building == null || (Construction.RepairMode == Construction.E_RepairMode.Target && !CanRepairBuilding(building.ConstructionModule)))
		{
			return;
		}
		switch (Construction.RepairMode)
		{
		case Construction.E_RepairMode.Target:
			RepairBuilding(building);
			ConstructionView.ClearRepairTilesFeedback();
			break;
		case Construction.E_RepairMode.Id:
		{
			foreach (TheLastStand.Model.Building.Building item in BuildingManager.GetBuildingsById(building.BuildingDefinition.Id))
			{
				if (CanRepairBuilding(item.ConstructionModule))
				{
					RepairBuilding(item);
					ConstructionView.ClearRepairTilesFeedback();
				}
			}
			break;
		}
		case Construction.E_RepairMode.None:
			TPSingleton<ConstructionManager>.Instance.LogError(string.Format("{0} should not be called when RepairMode is set to {1}!", "UpdateRepairOnClick", Construction.RepairMode));
			break;
		default:
			TPSingleton<ConstructionManager>.Instance.LogError($"Unhandled ConstructionMode {Construction.RepairMode}!");
			break;
		}
	}

	public void Deserialize(ISerializedData container, int saveVersion = -1)
	{
		if (!(container is SerializedConstruction serializedConstruction))
		{
			return;
		}
		instantProductionBonus.Clear();
		foreach (SerializedConstruction.SerializedInstantProduction serializedInstantProductionBonu in serializedConstruction.SerializedInstantProductionBonus)
		{
			instantProductionBonus.Add(serializedInstantProductionBonu.BuildingId, serializedInstantProductionBonu.ProductionBonus);
		}
	}

	public ISerializedData Serialize()
	{
		SerializedConstruction serializedConstruction = new SerializedConstruction();
		foreach (KeyValuePair<string, int> instantProductionBonu in instantProductionBonus)
		{
			serializedConstruction.SerializedInstantProductionBonus.Add(new SerializedConstruction.SerializedInstantProduction
			{
				BuildingId = instantProductionBonu.Key,
				ProductionBonus = instantProductionBonu.Value
			});
		}
		return serializedConstruction;
	}

	[DevConsoleCommand(Name = "ForceConstructionAllowed")]
	public static void DBG_ForceConstructionAllowed(bool forceConstructionAllowed = true)
	{
		TPSingleton<ConstructionManager>.Instance.debugForceConstructionAllowed = forceConstructionAllowed;
		TPSingleton<ConstructionView>.Instance.ChangeBuildingListContent();
	}
}
