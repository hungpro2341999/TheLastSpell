using System;
using System.Collections;
using System.Collections.Generic;
using TPLib;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Cursor;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.Recruitment;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.UI;
using UnityEngine;

namespace TheLastStand.Manager;

public class TileObjectSelectionManager : Manager<TileObjectSelectionManager>
{
	[Flags]
	public enum E_Orientation
	{
		NONE = 0,
		NORTH = 1,
		EAST = 2,
		SOUTH = 4,
		WEST = 8,
		LIMIT = 0x10,
		NORTH_EAST = 0x13,
		NORTH_WEST = 0x19,
		SOUTH_EAST = 0x16,
		SOUTH_WEST = 0x1C
	}

	[SerializeField]
	private SelectedUnitFeedback selectedUnitFeedbackPrefab;

	private SelectedUnitFeedback selectedUnitFeedback;

	public static bool CursorOrientationChanged;

	public static bool ClickedOnBuilding { get; private set; }

	public static bool HasAnythingSelected => SelectedTileObject != null;

	public static bool HasBuildingSelected => SelectedBuilding != null;

	public static bool HasEnemyUnitSelected
	{
		get
		{
			if (HasUnitSelected)
			{
				return SelectedUnit is EnemyUnit;
			}
			return false;
		}
	}

	public static bool HasPlayableUnitSelected
	{
		get
		{
			if (HasUnitSelected)
			{
				return SelectedUnit is PlayableUnit;
			}
			return false;
		}
	}

	public static bool HasTileSelected => SelectedTile != null;

	public static bool HasUnitSelected => SelectedUnit != null;

	public static bool IsProcessingASelection { get; private set; }

	public static PlayableUnit LastSelectedUnit { get; private set; }

	public static TheLastStand.Model.Building.Building PreviouslySelectedBuilding { get; private set; }

	public static PlayableUnit ReliablePlayableUnit => ReturnNullIfNonSelectable(SelectedUnit) ?? ReturnNullIfNonSelectable(LastSelectedUnit) ?? PlayableUnitManager.GetFirstLivingUnit();

	public static TheLastStand.Model.Building.Building SelectedBuilding { get; private set; }

	public static EnemyUnit SelectedEnemyUnit
	{
		get
		{
			if (!HasEnemyUnitSelected)
			{
				return null;
			}
			return SelectedUnit as EnemyUnit;
		}
	}

	public static PlayableUnit SelectedPlayableUnit
	{
		get
		{
			if (!HasPlayableUnitSelected)
			{
				return null;
			}
			return SelectedUnit as PlayableUnit;
		}
	}

	public static Tile SelectedTile { get; private set; }

	public static TheLastStand.Model.Unit.Unit SelectedUnit { get; private set; }

	public static ITileObject SelectedTileObject
	{
		get
		{
			ITileObject selectedUnit = SelectedUnit;
			object obj = selectedUnit;
			if (obj == null)
			{
				selectedUnit = SelectedBuilding;
				obj = selectedUnit ?? SelectedTile;
			}
			return (ITileObject)obj;
		}
	}

	public static E_Orientation CursorOrientationFromSelection { get; private set; }

	public static E_Orientation GuaranteedValidCursorOrientationFromSelection
	{
		get
		{
			if (CursorOrientationFromSelection != E_Orientation.NONE && !CursorOrientationFromSelection.HasFlag(E_Orientation.LIMIT))
			{
				return CursorOrientationFromSelection;
			}
			return PreviousCursorOrientationFromSelection;
		}
	}

	public static E_Orientation PreviousCursorOrientationFromSelection { get; private set; }

	public static SelectedUnitFeedback SelectedUnitFeedback => TPSingleton<TileObjectSelectionManager>.Instance.selectedUnitFeedback;

	public bool HasToWaitForNextFrame { get; set; }

	public static event Action OnUnitSelectionChange;

	public static void DeselectUnit()
	{
		IsProcessingASelection = true;
		if (HasUnitSelected)
		{
			TheLastStand.Model.Unit.Unit selectedUnit = SelectedUnit;
			SelectedUnit = null;
			selectedUnit.UnitView.Selected = false;
			if (selectedUnit is PlayableUnit)
			{
				TileMapView.ClearTiles(TileMapView.ReachableTilesTilemap);
				GameView.TopScreenPanel.UnitPortraitsPanel.DeselectAll();
				LastSelectedUnit = ReturnNullIfNonSelectable(selectedUnit);
			}
		}
		if (PlayableUnitManager.SelectedSkill != null)
		{
			PlayableUnitManager.SelectedSkill = null;
		}
		if (PlayableUnitManager.MovePath != null && PlayableUnitManager.MovePath.MovePathController != null)
		{
			PlayableUnitManager.MovePath.MovePathController.Clear();
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
		UnitManagementView<EnemyUnitManagementView>.Refresh();
		TileObjectSelectionManager.OnUnitSelectionChange?.Invoke();
		IsProcessingASelection = false;
	}

	public static void DeselectAll()
	{
		if (HasUnitSelected)
		{
			DeselectUnit();
		}
		else if (HasBuildingSelected)
		{
			SelectBuilding(null);
		}
		else if (HasTileSelected)
		{
			SelectTile(null);
		}
	}

	public static void EnsureUnitSelection()
	{
		SetSelectedPlayableUnit(ReliablePlayableUnit);
	}

	public static int GetAngleFromOrientation(E_Orientation orientation)
	{
		return orientation switch
		{
			E_Orientation.NORTH => 0, 
			E_Orientation.EAST => 270, 
			E_Orientation.SOUTH => 180, 
			E_Orientation.WEST => 90, 
			E_Orientation.NORTH_EAST => 315, 
			E_Orientation.NORTH_WEST => 45, 
			E_Orientation.SOUTH_EAST => 225, 
			E_Orientation.SOUTH_WEST => 135, 
			_ => 0, 
		};
	}

	public static GameDefinition.E_Direction GetDirectionFromOrientation(E_Orientation orientation)
	{
		return orientation switch
		{
			E_Orientation.NORTH => GameDefinition.E_Direction.North, 
			E_Orientation.EAST => GameDefinition.E_Direction.East, 
			E_Orientation.SOUTH => GameDefinition.E_Direction.South, 
			E_Orientation.WEST => GameDefinition.E_Direction.West, 
			_ => GameDefinition.E_Direction.None, 
		};
	}

	public static List<SpawnDirectionsDefinition.E_Direction> GetSpawnDirectionsFromOrientation(E_Orientation orientation)
	{
		List<SpawnDirectionsDefinition.E_Direction> list = new List<SpawnDirectionsDefinition.E_Direction>();
		if ((orientation & E_Orientation.NORTH) != E_Orientation.NONE)
		{
			list.Add(SpawnDirectionsDefinition.E_Direction.Top);
		}
		else if ((orientation & E_Orientation.SOUTH) != E_Orientation.NONE)
		{
			list.Add(SpawnDirectionsDefinition.E_Direction.Bottom);
		}
		if ((orientation & E_Orientation.EAST) != E_Orientation.NONE)
		{
			list.Add(SpawnDirectionsDefinition.E_Direction.Right);
		}
		else if ((orientation & E_Orientation.WEST) != E_Orientation.NONE)
		{
			list.Add(SpawnDirectionsDefinition.E_Direction.Left);
		}
		return list;
	}

	public static E_Orientation GetOrientationFromDirection(GameDefinition.E_Direction direction)
	{
		return direction switch
		{
			GameDefinition.E_Direction.North => E_Orientation.NORTH, 
			GameDefinition.E_Direction.East => E_Orientation.EAST, 
			GameDefinition.E_Direction.South => E_Orientation.SOUTH, 
			GameDefinition.E_Direction.West => E_Orientation.WEST, 
			_ => E_Orientation.NONE, 
		};
	}

	public static E_Orientation GetOppositeOrientation(E_Orientation orientation)
	{
		return orientation switch
		{
			E_Orientation.NORTH => E_Orientation.SOUTH, 
			E_Orientation.EAST => E_Orientation.WEST, 
			E_Orientation.SOUTH => E_Orientation.NORTH, 
			E_Orientation.WEST => E_Orientation.EAST, 
			E_Orientation.NORTH_EAST => E_Orientation.SOUTH_WEST, 
			E_Orientation.NORTH_WEST => E_Orientation.SOUTH_EAST, 
			E_Orientation.SOUTH_EAST => E_Orientation.NORTH_WEST, 
			E_Orientation.SOUTH_WEST => E_Orientation.NORTH_EAST, 
			_ => orientation, 
		};
	}

	public static E_Orientation GetOrientationFromPosToPos(Vector2Int fromPosition, Vector2Int toPosition)
	{
		Vector2Int vector2Int = toPosition - fromPosition;
		if (Mathf.Abs(vector2Int.x) > Mathf.Abs(vector2Int.y))
		{
			if (vector2Int.x >= 0)
			{
				return E_Orientation.EAST;
			}
			return E_Orientation.WEST;
		}
		if (Mathf.Abs(vector2Int.x) < Mathf.Abs(vector2Int.y))
		{
			if (vector2Int.y >= 0)
			{
				return E_Orientation.NORTH;
			}
			return E_Orientation.SOUTH;
		}
		if (Mathf.Abs(vector2Int.x) == Mathf.Abs(vector2Int.y))
		{
			if (toPosition.y > fromPosition.y)
			{
				if (toPosition.x <= fromPosition.x)
				{
					return E_Orientation.NORTH_WEST;
				}
				return E_Orientation.NORTH_EAST;
			}
			if (toPosition.y < fromPosition.y)
			{
				if (toPosition.x <= fromPosition.x)
				{
					return E_Orientation.SOUTH_WEST;
				}
				return E_Orientation.SOUTH_EAST;
			}
			return E_Orientation.LIMIT;
		}
		return E_Orientation.NONE;
	}

	public static E_Orientation GetOrientationFromSelectionToPos(Vector2Int tilePos)
	{
		if (HasAnythingSelected)
		{
			return GetOrientationFromPosToPos(SelectedTileObject.OriginTile.Position, tilePos);
		}
		return E_Orientation.NONE;
	}

	public static E_Orientation GetOrientationFromSelectionToTile(Tile tile)
	{
		if (HasAnythingSelected && tile != null)
		{
			return GetOrientationFromPosToPos(SelectedTileObject.OccupiedTiles.GetFirstClosestTile(tile).tile.Position, tile.Position);
		}
		return E_Orientation.NONE;
	}

	public static E_Orientation GetOrientationFromTileToTile(Tile fromTile, Tile toTile)
	{
		if (toTile != null && fromTile != null)
		{
			return GetOrientationFromPosToPos(fromTile.Position, toTile.Position);
		}
		return E_Orientation.NONE;
	}

	public static Tile GetTileFromTileToOrientation(Tile source, E_Orientation orientation, int distance = 1)
	{
		return orientation switch
		{
			E_Orientation.NORTH => TileMapManager.GetTile(source.X, source.Y + distance), 
			E_Orientation.EAST => TileMapManager.GetTile(source.X + distance, source.Y), 
			E_Orientation.SOUTH => TileMapManager.GetTile(source.X, source.Y - distance), 
			E_Orientation.WEST => TileMapManager.GetTile(source.X - distance, source.Y), 
			E_Orientation.NORTH_EAST => TileMapManager.GetTile(source.X + distance, source.Y + distance), 
			E_Orientation.NORTH_WEST => TileMapManager.GetTile(source.X - distance, source.Y + distance), 
			E_Orientation.SOUTH_EAST => TileMapManager.GetTile(source.X + distance, source.Y - distance), 
			E_Orientation.SOUTH_WEST => TileMapManager.GetTile(source.X - distance, source.Y - distance), 
			_ => source, 
		};
	}

	public static void OnGameStateChange(Game.E_State state, Game.E_State previousState)
	{
		switch (state)
		{
		case Game.E_State.Construction:
			DeselectAll();
			break;
		case Game.E_State.CharacterSheet:
		case Game.E_State.Recruitment:
		case Game.E_State.Shopping:
		case Game.E_State.BuildingUpgrade:
		case Game.E_State.NightReport:
		case Game.E_State.ProductionReport:
		case Game.E_State.HowToPlay:
		case Game.E_State.GameOver:
			SelectBuilding(null);
			SelectTile(null);
			break;
		}
	}

	public static void ResetStaticProperties()
	{
		SelectedBuilding = null;
		SelectedUnit = null;
		SelectedTile = null;
	}

	public static void SelectLastSelectedUnit()
	{
		SetSelectedPlayableUnit(LastSelectedUnit);
	}

	public static void SelectBuilding(TheLastStand.Model.Building.Building building, bool focusCameraOnBuilding = false)
	{
		if (SelectedBuilding == building)
		{
			return;
		}
		if (building != null)
		{
			DeselectUnit();
			SelectTile(null);
		}
		TheLastStand.Model.Building.Building selectedBuilding = SelectedBuilding;
		SelectedBuilding = building;
		if (selectedBuilding != null)
		{
			selectedBuilding.BuildingView.Selected = false;
			if (!selectedBuilding.BuildingView.Hovered && (selectedBuilding.IsHandledDefense || (selectedBuilding.IsTrap && (selectedBuilding.BattleModule.IsTrapFullyCharged() || TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Construction))))
			{
				selectedBuilding.BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: false);
			}
		}
		GameView.BottomScreenPanel.BottomLeftPanel.Refresh();
		if (SelectedBuilding == null)
		{
			GameView.BottomScreenPanel.BuildingManagementPanel.Close();
			PlayableUnitManagementView.OnSelectedBuildingChange();
			if (SelectedUnit != null)
			{
				SelectedUnitFeedback.Display(display: true);
				if (TPSingleton<GameManager>.Instance.Game.Cursor.Tile != null && HasPlayableUnitSelected)
				{
					SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
					TPSingleton<PlayableUnitManager>.Instance.RefreshUnitMovePath(SelectedPlayableUnit);
				}
			}
			return;
		}
		SelectedBuilding.BuildingView.Selected = true;
		SelectedUnitFeedback.Display(display: false);
		if (!SelectedBuilding.BlueprintModule.IsIndestructible && SelectedBuilding.DamageableModule.Health <= 0f)
		{
			return;
		}
		Game.E_State state = TPSingleton<GameManager>.Instance.Game.State;
		if (state == Game.E_State.Management || state == Game.E_State.Wait)
		{
			if (focusCameraOnBuilding && CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(SelectedBuilding.BuildingView.transform.position, new Vector3(0f, 1f)))
			{
				ACameraView.MoveTo(SelectedBuilding.BuildingView.transform);
			}
			PlayableUnitManagementView.OnSelectedBuildingChange();
			GameView.BottomScreenPanel.BuildingManagementPanel.Open();
		}
		SelectedBuilding.BuildingView.Selected = true;
		if (SelectedBuilding.IsTrap || SelectedBuilding.IsHandledDefense)
		{
			SelectedBuilding.BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: true);
		}
		if (InputManager.IsLastControllerJoystick && InputManager.JoystickConfig.HUDNavigation.SelectFirstBuildingCapacity)
		{
			TPSingleton<TileObjectSelectionManager>.Instance.StartCoroutine(SelectFirstBuildingCapacity());
		}
	}

	public static void SetSelectedEnemyUnit(EnemyUnit enemyUnit)
	{
		SelectBuilding(null);
		SelectTile(null);
		DeselectUnit();
		if (!IsUnitSelectable(enemyUnit))
		{
			DeselectUnit();
			return;
		}
		enemyUnit.EnemyUnitView.Selected = true;
		IsProcessingASelection = true;
		if (HasPlayableUnitSelected)
		{
			LastSelectedUnit = ReturnNullIfNonSelectable(SelectedUnit);
		}
		SelectedUnit = enemyUnit;
		UnitManagementView<EnemyUnitManagementView>.Refresh();
		TileObjectSelectionManager.OnUnitSelectionChange?.Invoke();
		IsProcessingASelection = false;
	}

	public static void SelectTile(Tile tile, bool focusCameraOnTile = false)
	{
		if (SelectedTile == tile)
		{
			return;
		}
		if (tile != null)
		{
			DeselectUnit();
		}
		if (SelectedTile != null)
		{
			TileMapView.SetTile(TileMapView.BuildingSelectionFeedbackTilemap, SelectedTile);
		}
		SelectedTile = tile;
		GameView.BottomScreenPanel.BottomLeftPanel.Refresh();
		if (SelectedTile == null)
		{
			GameView.BottomScreenPanel.GroundManagementPanel.Close();
			if (SelectedUnit != null)
			{
				SelectedUnitFeedback.Display(display: true);
				if (TPSingleton<GameManager>.Instance.Game.Cursor.Tile != null && HasPlayableUnitSelected)
				{
					SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
					TPSingleton<PlayableUnitManager>.Instance.RefreshUnitMovePath(SelectedPlayableUnit);
				}
			}
			return;
		}
		TileMapView.SetTile(TileMapView.BuildingSelectionFeedbackTilemap, tile, "View/Tiles/Feedbacks/BuildingSelectionFeedback");
		SelectedUnitFeedback.Display(display: false);
		Game.E_State state = TPSingleton<GameManager>.Instance.Game.State;
		if (state == Game.E_State.Management || state == Game.E_State.Wait)
		{
			if (focusCameraOnTile && CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(SelectedBuilding.BuildingView.transform.position, new Vector3(0f, 1f)))
			{
				ACameraView.MoveTo(SelectedTile.TileView.transform);
			}
			GameView.BottomScreenPanel.GroundManagementPanel.Open();
		}
	}

	public static void SetSelectedPlayableUnit(PlayableUnit unit, bool focusCameraOnUnit = false)
	{
		LastSelectedUnit = ReturnNullIfNonSelectable(SelectedUnit);
		SelectBuilding(null);
		SetSelectedEnemyUnit(null);
		SelectTile(null);
		if (!IsUnitSelectable(unit))
		{
			if (unit != null && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.GameOver)
			{
				SelectedUnit = unit;
				unit.PlayableUnitController.AssignItemSlotViewsToUnit();
			}
			else
			{
				DeselectUnit();
			}
			return;
		}
		IsProcessingASelection = true;
		TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles = SelectedUnit != unit;
		SelectedUnit = unit;
		unit.UnitView.Selected = true;
		unit.State = TheLastStand.Model.Unit.Unit.E_State.Ready;
		GameView.TopScreenPanel.UnitPortraitsPanel.ToggleSelectedUnit(unit);
		unit.PlayableUnitController.AssignItemSlotViewsToUnit();
		TPSingleton<TileObjectSelectionManager>.Instance.selectedUnitFeedback.Unit = unit;
		Game.E_State state = TPSingleton<GameManager>.Instance.Game.State;
		if ((uint)state <= 1u || state == Game.E_State.Wait)
		{
			if (SelectedUnit != null)
			{
				if (LastSelectedUnit == unit)
				{
					TPSingleton<PlayableUnitManager>.Instance.FocusCamOnUnit(SelectedUnit);
				}
				else
				{
					TPSingleton<CursorView>.Instance.SnapJoystickCursorToTile(SelectedUnit.OriginTile);
					TPSingleton<GameManager>.Instance.Game.Cursor?.CursorController.SetTile();
					if (TPSingleton<SettingsManager>.Instance.Settings.FocusCamOnSelections && focusCameraOnUnit)
					{
						TPSingleton<PlayableUnitManager>.Instance.FocusCamOnUnit(SelectedUnit);
					}
				}
			}
			if (TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles && HasPlayableUnitSelected)
			{
				SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
			}
			TPSingleton<PlayableUnitManager>.Instance.SetSkillsHotkeys();
			GameView.BottomScreenPanel.BuildingManagementPanel.Close();
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
		TileObjectSelectionManager.OnUnitSelectionChange?.Invoke();
		IsProcessingASelection = false;
		if (PlayableUnitManager.StatTooltip.Displayed)
		{
			PlayableUnitManager.StatTooltip.Refresh();
		}
	}

	public static void UpdateCursorOrientationFromSelection(Tile cursorTile = null)
	{
		if (!CursorOrientationFromSelection.HasFlag(E_Orientation.LIMIT) && CursorOrientationFromSelection != E_Orientation.NONE)
		{
			PreviousCursorOrientationFromSelection = CursorOrientationFromSelection;
		}
		else if (CursorOrientationFromSelection.HasFlag(E_Orientation.LIMIT) && !CursorOrientationFromSelection.HasFlag(PreviousCursorOrientationFromSelection))
		{
			PreviousCursorOrientationFromSelection = (E_Orientation)((int)PreviousCursorOrientationFromSelection * 4 % 15);
		}
		E_Orientation orientationFromSelectionToTile = GetOrientationFromSelectionToTile(cursorTile);
		if (CursorOrientationFromSelection != orientationFromSelectionToTile)
		{
			CursorOrientationChanged = true;
		}
		CursorOrientationFromSelection = orientationFromSelectionToTile;
	}

	public static void SwitchPreviousCursorOrientation()
	{
		if (CursorOrientationFromSelection.HasFlag(E_Orientation.LIMIT))
		{
			PreviousCursorOrientationFromSelection = CursorOrientationFromSelection ^ E_Orientation.LIMIT ^ PreviousCursorOrientationFromSelection;
		}
	}

	public void Init()
	{
		selectedUnitFeedback = UnityEngine.Object.Instantiate(selectedUnitFeedbackPrefab, GameManager.ViewTransform);
	}

	private static bool IsOnSkillBarSelection()
	{
		if (!TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.JoystickSkillBar.IsOnSkillBarSelection() && !TPSingleton<EnemyUnitManagementView>.Instance.SkillBar.JoystickSkillBar.IsOnSkillBarSelection())
		{
			return GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.JoystickSkillBar.IsOnSkillBarSelection();
		}
		return true;
	}

	private static bool IsUnitSelectable(TheLastStand.Model.Unit.Unit unit)
	{
		if (unit is EnemyUnit enemyUnit && enemyUnit.OriginTile.HasFog && !(enemyUnit is BossUnit))
		{
			return false;
		}
		if (unit != null && unit.Health > 0f && !unit.IsDead)
		{
			return unit.UnitView != null;
		}
		return false;
	}

	private static PlayableUnit ReturnNullIfNonSelectable(TheLastStand.Model.Unit.Unit unit)
	{
		if (!IsUnitSelectable(unit) || !(unit is PlayableUnit result))
		{
			return null;
		}
		return result;
	}

	private static IEnumerator SelectFirstBuildingCapacity()
	{
		yield return SharedYields.WaitForEndOfFrame;
		GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.SelectFirstBuildingCapacity();
	}

	private bool CanSelectBuildingOnTile(Tile tile)
	{
		if (tile != null && !tile.HasFog)
		{
			return tile.Building != null;
		}
		return false;
	}

	private void Update()
	{
		ClickedOnBuilding = false;
		if (HasToWaitForNextFrame)
		{
			HasToWaitForNextFrame = false;
		}
		else
		{
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.PlayableUnits)
			{
				return;
			}
			Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
			Game.E_State state = TPSingleton<GameManager>.Instance.Game.State;
			if (state <= Game.E_State.UnitPreparingSkill)
			{
				if (state != Game.E_State.Management)
				{
					_ = 3;
					return;
				}
				if (tile != null)
				{
					TheLastStand.Model.Unit.Unit selectedUnit = SelectedUnit;
					if (selectedUnit != null && selectedUnit.State == TheLastStand.Model.Unit.Unit.E_State.Ready && SelectedBuilding == null && (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged || PlayableUnitManager.MovePath.Path.Count == 0) && HasPlayableUnitSelected && !IsOnSkillBarSelection())
					{
						TPSingleton<PlayableUnitManager>.Instance.RefreshUnitMovePath(SelectedPlayableUnit);
					}
					if (InputManager.GetButtonDown(24))
					{
						if (HasPlayableUnitSelected && SelectedUnit.OriginTile != tile && tile.Unit == null && SelectedUnit.State == TheLastStand.Model.Unit.Unit.E_State.Ready && PathfindingManager.Pathfinding.ReachableTiles.ContainsKey(tile) && SelectedUnit.CanStopOn(tile) && !IsOnSkillBarSelection())
						{
							TPSingleton<PlayableUnitManager>.Instance.MoveUnit(SelectedPlayableUnit);
						}
						else if (!TPSingleton<HUDJoystickNavigationManager>.Instance.HUDNavigationOn && !IsOnSkillBarSelection())
						{
							SelectThingsOnTile();
						}
						TPSingleton<CursorView>.Instance.SnapJoystickViewToCursorTile();
						return;
					}
				}
				if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
				{
					if (!TPSingleton<SettingsManager>.Instance.HasClosedSettingsThisFrame && !TPSingleton<HUDJoystickNavigationManager>.Instance.HUDNavigationOn && !CharacterSheetPanel.HasClosedThisFrame && !GenericBlockingPopup.IsWaitingForInput() && !IsOnSkillBarSelection())
					{
						DeselectAll();
					}
				}
				else if (InputManager.GetButtonDown(0))
				{
					PlayableUnitManager.SelectNextUnit();
				}
				else if (InputManager.GetButtonDown(11))
				{
					PlayableUnitManager.SelectPreviousUnit();
				}
				else if (InputManager.GetButtonDown(60))
				{
					if (TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution != null)
					{
						TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution.SkillExecutionController.Reset();
						TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution = null;
					}
					TPSingleton<PlayableUnitManager>.Instance.ChangeEquipment();
					TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.CheckSkillButtonsFocus();
				}
				else if (InputManager.GetButtonDown(53))
				{
					if (TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution != null)
					{
						TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution.SkillExecutionController.Reset();
						TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution = null;
					}
					PlayableUnitManager.UndoLastCommand();
					TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.CheckSkillButtonsFocus();
				}
				else
				{
					TPSingleton<PlayableUnitManager>.Instance.UpdateSkillHotkeysInput();
					int unitIndexHotkeyPressed = TPSingleton<PlayableUnitManager>.Instance.GetUnitIndexHotkeyPressed();
					if (unitIndexHotkeyPressed != -1)
					{
						SetSelectedPlayableUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed], CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed].UnitView.transform.position));
					}
				}
				return;
			}
			switch (state)
			{
			case Game.E_State.Construction:
			{
				Construction.E_State state2 = TPSingleton<ConstructionManager>.Instance.Construction.State;
				if ((uint)(state2 - 1) > 1u && state2 != Construction.E_State.PlaceBuilding)
				{
					if ((InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137)) && SelectedBuilding != null)
					{
						SelectBuilding(null);
					}
					if (!InputManager.IsLastControllerJoystick && InputManager.GetButtonDown(24) && tile != null)
					{
						GameController.SetState(Game.E_State.Management);
						SelectThingsOnTile(forceCameraFocus: true);
						TPSingleton<CursorView>.Instance.SnapJoystickViewToCursorTile();
					}
				}
				break;
			}
			case Game.E_State.PlaceUnit:
				if (TPSingleton<GameManager>.Instance.Game.Cursor.Tile != null)
				{
					if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
					{
						TPSingleton<PlayableUnitManager>.Instance.SelectedPlayableUnitGhost.Display(displayed: true);
						TPSingleton<PlayableUnitManager>.Instance.SelectedPlayableUnitGhost.FollowMouse();
						bool flag = TileMapController.CanPlaceUnit(TPSingleton<GameManager>.Instance.Game.Cursor.Tile, Tile.E_UnitAccess.Hero);
						TPSingleton<GameManager>.Instance.Game.Cursor.CursorState = (flag ? TheLastStand.Model.Cursor.E_CursorState.Valid : TheLastStand.Model.Cursor.E_CursorState.Invalid);
						TPSingleton<PlayableUnitManager>.Instance.SelectedPlayableUnitGhost.ChangeColors(flag);
					}
					if (InputManager.GetButtonDown(24) && TileMapController.CanPlaceUnit(TPSingleton<GameManager>.Instance.Game.Cursor.Tile, Tile.E_UnitAccess.Hero))
					{
						RecruitmentController.RecruitUnit(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
						TPSingleton<GameManager>.Instance.Game.Cursor.CursorState = TheLastStand.Model.Cursor.E_CursorState.Undefined;
						TPSingleton<PlayableUnitManager>.Instance.SelectedPlayableUnitGhost.Display(displayed: false);
						GameController.SetState(Game.E_State.Management);
					}
				}
				if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
				{
					TPSingleton<RecruitmentView>.Instance.UnitPlacingDoneThisFrame = true;
					RecruitmentController.OpenRecruitmentPanel();
					TPSingleton<GameManager>.Instance.Game.Cursor.CursorState = TheLastStand.Model.Cursor.E_CursorState.Undefined;
					TPSingleton<PlayableUnitManager>.Instance.SelectedPlayableUnitGhost.Display(displayed: false);
				}
				break;
			}
		}
	}

	private void SelectThingsOnTile(bool forceCameraFocus = false)
	{
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		bool flag = tile.Unit != null;
		bool flag2 = tile.Building != null;
		bool flag3 = tile.Unit == SelectedUnit && SelectedUnit != null;
		bool flag4 = tile.Building == SelectedBuilding && SelectedBuilding != null;
		if ((!HasAnythingSelected || (HasBuildingSelected && !flag4) || (HasUnitSelected && !flag3) || HasTileSelected) && flag && IsUnitSelectable(tile.Unit))
		{
			if (tile.Unit is PlayableUnit)
			{
				SelectPlayableUnitBehaviour(forceCameraFocus);
			}
			else if (tile.Unit is EnemyUnit)
			{
				SelectEnemyUnitBehaviour();
			}
		}
		else if ((!HasAnythingSelected || (HasBuildingSelected && !flag4) || HasUnitSelected || HasTileSelected) && flag2 && CanSelectBuildingOnTile(tile))
		{
			SelectBuildingBehaviour(forceCameraFocus);
		}
		else
		{
			SelectTileBehaviour();
		}
	}

	private void SelectEnemyUnitBehaviour()
	{
		if (SelectedBuilding != null)
		{
			SelectBuilding(null);
		}
		if (SelectedTile != null)
		{
			SelectTile(null);
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
		{
			PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
		}
		SetSelectedEnemyUnit(TPSingleton<GameManager>.Instance.Game.Cursor.Tile.Unit as EnemyUnit);
		TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnEnemyUnitSelected);
	}

	private void SelectBuildingBehaviour(bool forceCameraFocus = false)
	{
		if (SelectedUnit != null)
		{
			SetSelectedPlayableUnit(null);
		}
		if (SelectedTile != null)
		{
			SelectTile(null);
		}
		if (CanSelectBuildingOnTile(TPSingleton<GameManager>.Instance.Game.Cursor.Tile))
		{
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction)
			{
				GameController.SetState(Game.E_State.Management);
			}
			SelectBuilding(TPSingleton<GameManager>.Instance.Game.Cursor.Tile.Building, SelectedBuilding == null || forceCameraFocus);
			if (SelectedBuilding.IsBonePile)
			{
				TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnBonePileSelected);
			}
		}
	}

	private void SelectPlayableUnitBehaviour(bool forceCameraFocus = false)
	{
		if (SelectedBuilding != null)
		{
			SelectBuilding(null);
		}
		if (SelectedTile != null)
		{
			SelectTile(null);
		}
		if (TPSingleton<GameManager>.Instance.Game.Cursor.Tile.Unit == SelectedUnit && SelectedBuilding == null)
		{
			SelectedUnit.State = TheLastStand.Model.Unit.Unit.E_State.Ready;
		}
		else
		{
			if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
			{
				PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
			}
			SetSelectedPlayableUnit(TPSingleton<GameManager>.Instance.Game.Cursor.Tile.Unit as PlayableUnit, CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(TPSingleton<GameManager>.Instance.Game.Cursor.Tile) || forceCameraFocus);
		}
		if (TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles && HasUnitSelected && HasPlayableUnitSelected)
		{
			SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
		}
		PlayableUnitManager.MovePath.MovePathController.Clear();
		TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnPlayableUnitSelected);
	}

	private void SelectTileBehaviour()
	{
		if (SelectedUnit != null)
		{
			SetSelectedPlayableUnit(null);
		}
		if (SelectedBuilding != null)
		{
			SelectBuilding(null);
		}
		SelectTile(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
	}

	public void UpdateUnitInfoPanel(TheLastStand.Model.Unit.Unit unit)
	{
		if (UIManager.DebugToggleUI == false)
		{
			return;
		}
		if (unit is PlayableUnit content)
		{
			PlayableUnitManager.PlayableUnitTooltip.SetContent(content);
			PlayableUnitManager.PlayableUnitTooltip.Display();
		}
		else
		{
			PlayableUnitManager.PlayableUnitTooltip.Hide();
		}
		if (unit is EnemyUnit enemyUnit)
		{
			if (enemyUnit is EliteEnemyUnit)
			{
				EnemyUnitManager.EnemyUnitInfoPanel.Hide();
			}
			else
			{
				EnemyUnitManager.EliteEnemyUnitInfoPanel.Hide();
			}
			EnemyUnitTooltip tooltipForEnemy = EnemyUnitManager.GetTooltipForEnemy(enemyUnit);
			tooltipForEnemy.SetContent(enemyUnit);
			tooltipForEnemy.Display();
		}
		else
		{
			EnemyUnitManager.HideInfoPanels();
		}
	}
}
