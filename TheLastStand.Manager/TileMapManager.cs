using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition.TileMap;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Pathfinding;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Manager;

public class TileMapManager : Manager<TileMapManager>
{
	public delegate void TileDirtyHandler(Tile dirtyTile);

	private static class Constants
	{
		public const string TileMapTextAssetPathFormat = "TextAssets/Cities/{0}/{0}_TileMap";

		public const string TileMapLevelEditorTextAssetPathFormat = "TextAssets/Cities/Level Editor/{0}/{0}_TileMap";

		public const string TileFlagTilesNbDetailsText = "Details (flag : total tiles)";

		public const string TileFlagTilesNbText = "{0} : {1}";
	}

	[SerializeField]
	[Range(11f, 77f)]
	private int defaultLevelWidth = 39;

	[SerializeField]
	[Range(11f, 77f)]
	private int defaultLevelHeight = 39;

	[SerializeField]
	private int loadingSpeed = 4;

	[SerializeField]
	private TileView tileViewPrefab;

	[SerializeField]
	private Transform tileViewsTransform;

	[SerializeField]
	private TileFlagDefinition[] tileFlagDefinitions;

	private TileMap tileMap;

	public static int DefaultLevelHeight => TPSingleton<TileMapManager>.Instance.defaultLevelHeight;

	public static int DefaultLevelWidth => TPSingleton<TileMapManager>.Instance.defaultLevelWidth;

	public static int LoadingSpeed => TPSingleton<TileMapManager>.Instance.loadingSpeed;

	public static TileFlagDefinition[] TileFlagDefinitions => TPSingleton<TileMapManager>.Instance.tileFlagDefinitions;

	public static TileView TileViewPrefab => TPSingleton<TileMapManager>.Instance.tileViewPrefab;

	public static Transform TileViewsTransform => TPSingleton<TileMapManager>.Instance.tileViewsTransform;

	public TileMap TileMap
	{
		get
		{
			if (tileMap == null)
			{
				bool num = ApplicationManager.Application.State.GetName() == "LevelEditor";
				string text = (num ? LevelEditorManager.CityToLoadId : TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.LevelLayoutTileMapId);
				string format = ((num || GameManager.LoadLevelEditorCityAssets) ? "TextAssets/Cities/Level Editor/{0}/{0}_TileMap" : "TextAssets/Cities/{0}/{0}_TileMap");
				TextAsset textAsset = ResourcePooler.LoadOnce<TextAsset>(string.Format(format, text));
				TPSingleton<TileMapManager>.Instance.Log("Loading Level Tilemap layout using Id <b>" + text + "</b> from path <b>" + string.Format(format, text) + "</b> " + (GameManager.LoadLevelEditorCityAssets ? " (in Level but using Level Editor path)" : string.Empty) + ".", CLogLevel.MAJOR, forcePrintInUnity: true);
				TPSingleton<TileMapManager>.Instance.Log("Level Tilemap layout loading " + ((textAsset != null) ? "<color=green>succeeded</color>" : "<color=red>failed</color>") + ".", CLogLevel.MAJOR, forcePrintInUnity: true);
				tileMap = new TileMapController((textAsset != null) ? XDocument.Parse(textAsset.text, LoadOptions.SetBaseUri) : null, TPSingleton<TileMapView>.Instance).TileMap;
			}
			return tileMap;
		}
		set
		{
			tileMap = value;
		}
	}

	public static void ClearBuildingOnTiles(List<Tile> tiles)
	{
		foreach (Tile tile in tiles)
		{
			if (tile.Building != null)
			{
				BuildingManager.DestroyBuilding(tile, updateView: true, addDeadBuilding: false, triggerEvent: true, triggerOnDeathEvent: false);
			}
		}
	}

	public static void ClearEnemiesOnTiles(List<Tile> tiles)
	{
		foreach (Tile tile in tiles)
		{
			if (tile.Unit is EnemyUnit)
			{
				TheLastStand.Model.Unit.Unit unit = tile.Unit;
				unit.UnitController.PrepareForExile();
				unit.UnitController.ExecuteExile();
			}
		}
	}

	public static void FreeTilesFromPlayableUnits(List<Tile> occupiedTiles, Func<Tile, bool> validator = null)
	{
		foreach (Tile occupiedTile in occupiedTiles)
		{
			if (occupiedTile.Unit is PlayableUnit playableUnit)
			{
				Tile closestTileFillingConditions = GetClosestTileFillingConditions(20, occupiedTile, (Tile tile) => !occupiedTiles.Contains(tile) && (tile.Building == null || tile.Building.IsTrap || tile.Building.IsWalkableHandledDefense) && tile.IsCrossable && tile.Unit == null && !tile.HasFog && (validator?.Invoke(tile) ?? true));
				PathfindingData pathfindingData = new PathfindingData
				{
					Unit = playableUnit,
					TargetTiles = new Tile[1] { closestTileFillingConditions },
					MoveRange = -1,
					DistanceFromTargetMin = 0,
					DistanceFromTargetMax = 0,
					IgnoreCanStopOnConstraints = true,
					OverrideUnitMoveMethod = UnitTemplateDefinition.E_MoveMethod.Flying,
					PathfindingStyle = PathfindingDefinition.E_PathfindingStyle.Bresenham
				};
				PlayableUnitManager.MovePath.MovePathController.ComputePath(pathfindingData);
				PlayableUnitManager.MovePath.MovePathController.UpdateState(playableUnit.OriginTile);
				PlayableUnitManager.MovePath.State = MovePath.E_State.Teleport;
				playableUnit.Path = PlayableUnitManager.MovePath.Path;
				playableUnit.PlayableUnitController.PrepareForMovement(0, forceInstant: true).StartTask();
			}
		}
	}

	public static Tile GetTile(int x, int y)
	{
		if (x >= 0 && y >= 0 && x < TPSingleton<TileMapManager>.Instance.TileMap.Width && y < TPSingleton<TileMapManager>.Instance.TileMap.Height)
		{
			return TPSingleton<TileMapManager>.Instance.TileMap.Tiles[x * TPSingleton<TileMapManager>.Instance.TileMap.Height + y];
		}
		return null;
	}

	public static Tile GetRandomTile()
	{
		return RandomManager.GetRandomElement(TPSingleton<TileMapManager>.Instance, TPSingleton<TileMapManager>.Instance.TileMap.Tiles);
	}

	public static List<Tile> GetTilesWithFlag(TileFlagDefinition.E_TileFlagTag tileFlag)
	{
		if (!TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag.ContainsKey(tileFlag))
		{
			TPSingleton<TileMapManager>.Instance.LogWarning(string.Format("{0} doesn't have key {1}.", "TilesWithFlag", tileFlag));
			return new List<Tile>();
		}
		return TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag[tileFlag];
	}

	public static Tile GetRandomTileWithFlag(TileFlagDefinition.E_TileFlagTag tileFlag)
	{
		List<Tile> tilesWithFlag = GetTilesWithFlag(tileFlag);
		return RandomManager.GetRandomElement(TPSingleton<TileMapManager>.Instance, tilesWithFlag);
	}

	public static Tile GetRandomSpawnableTileWithFlag(TileFlagDefinition.E_TileFlagTag tileFlag, UnitTemplateDefinition unitTemplateDefinition, bool isPhaseActor = false)
	{
		IEnumerable<Tile> enumerable = from t in GetTilesWithFlag(tileFlag)
			where unitTemplateDefinition.CanSpawnOn(t, isPhaseActor)
			select t;
		if (!enumerable.Any())
		{
			return null;
		}
		return RandomManager.GetRandomElement(TPSingleton<TileMapManager>.Instance, enumerable);
	}

	public static Tile GetRandomSpawnableTileWithFlag(TileFlagDefinition.E_TileFlagTag tileFlag, UnitTemplateDefinition unitTemplateDefinition, Predicate<Tile> predicate, bool isPhaseActor = false)
	{
		IEnumerable<Tile> enumerable = from t in GetTilesWithFlag(tileFlag).FindAll(predicate)
			where unitTemplateDefinition.CanSpawnOn(t, isPhaseActor)
			select t;
		if (!enumerable.Any())
		{
			return null;
		}
		return RandomManager.GetRandomElement(TPSingleton<TileMapManager>.Instance, enumerable);
	}

	public static Tile GetRandomTileWithFlag(TileFlagDefinition.E_TileFlagTag tileFlag, Predicate<Tile> predicate)
	{
		List<Tile> list = GetTilesWithFlag(tileFlag).FindAll(predicate);
		if (list.Count <= 0)
		{
			return null;
		}
		return RandomManager.GetRandomElement(TPSingleton<TileMapManager>.Instance, list);
	}

	public static Tile GetClosestTileFillingConditions(int maxRange, Tile sourceTile, Func<Tile, bool> condition, bool cardinalOnly = false)
	{
		for (int i = 0; i <= maxRange; i++)
		{
			for (int j = -i; j <= i; j++)
			{
				int num = i - Mathf.Abs(j);
				if (!cardinalOnly || j == 0 || num == 0)
				{
					Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(sourceTile.Position.x + j, sourceTile.Position.y + num);
					Tile tile2 = null;
					if (num != 0)
					{
						tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(sourceTile.Position.x + j, sourceTile.Position.y - num);
					}
					if (tile != null && condition(tile))
					{
						return tile;
					}
					if (tile2 != null && condition(tile2))
					{
						return tile2;
					}
				}
			}
		}
		return null;
	}

	public static void GoThroughTilesInRange(int maxRange, Tile sourceTile, Action<Tile, Vector2Int> actionOnTile, bool cardinalOnly = false, int minRange = 0)
	{
		for (int i = minRange; i <= maxRange; i++)
		{
			for (int j = -i; j <= i; j++)
			{
				int num = i - Mathf.Abs(j);
				if (!cardinalOnly || j == 0 || num == 0)
				{
					Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(sourceTile.Position.x + j, sourceTile.Position.y + num);
					Tile tile2 = null;
					if (num != 0)
					{
						tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(sourceTile.Position.x + j, sourceTile.Position.y - num);
					}
					if (tile != null)
					{
						actionOnTile(tile, new Vector2Int(j, num));
					}
					if (tile2 != null)
					{
						actionOnTile(tile2, new Vector2Int(j, -num));
					}
				}
			}
		}
	}

	public static void GoThroughTilesInDiagonalsRange(int maxRange, Tile sourceTile, Action<Tile, Vector2Int> actionOnTile, int minRange = 0)
	{
		for (int i = -maxRange; i <= maxRange; i++)
		{
			for (int j = -maxRange; j <= maxRange; j++)
			{
				if (Mathf.Abs(i) >= minRange || Mathf.Abs(j) >= minRange)
				{
					Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(sourceTile.Position.x + i, sourceTile.Position.y + j);
					if (tile != null)
					{
						actionOnTile(tile, new Vector2Int(i, j));
					}
				}
			}
		}
	}

	public static bool GoThroughTilesInRangeUntil(int maxRange, Tile sourceTile, Func<Tile, Vector2Int, bool> stopCondition, bool cardinalOnly = false, int minRange = 0)
	{
		for (int i = minRange; i <= maxRange; i++)
		{
			for (int j = -i; j <= i; j++)
			{
				int num = i - Mathf.Abs(j);
				if (!cardinalOnly || j == 0 || num == 0)
				{
					Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(sourceTile.Position.x + j, sourceTile.Position.y + num);
					Tile tile2 = null;
					if (num != 0)
					{
						tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(sourceTile.Position.x + j, sourceTile.Position.y - num);
					}
					if (tile != null && stopCondition(tile, new Vector2Int(j, num)))
					{
						return true;
					}
					if (tile2 != null && stopCondition(tile2, new Vector2Int(j, -num)))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool RemoveTileFlag(Tile tile, TileFlagDefinition.E_TileFlagTag flag)
	{
		if (flag == TileFlagDefinition.E_TileFlagTag.None)
		{
			TPSingleton<TileMapManager>.Instance.LogError($"Trying to remove flag {flag} on tile {tile.X}/{tile.Y} but it should not have been set before.", CLogLevel.MAJOR);
			return false;
		}
		if (!TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag.TryGetValue(flag, out var value))
		{
			TPSingleton<TileMapManager>.Instance.Log($"Tried to erase flag {flag} on tile {tile.X}/{tile.Y} but flag was not recorded in dictionary.", CLogLevel.MAJOR, forcePrintInUnity: true);
			return false;
		}
		if (!value.Contains(tile))
		{
			TPSingleton<TileMapManager>.Instance.LogWarning($"Tried to erase flag {flag} on tile {tile.X}/{tile.Y} but it was not recorded in the tiles list with this flag. This may be normal if you did erase using rect tool, including non-flagged tiles.", CLogLevel.MAJOR);
			return false;
		}
		value.Remove(tile);
		return true;
	}

	public static void SetTileFlag(Tile tile, TileFlagDefinition.E_TileFlagTag flag)
	{
		List<Tile> value;
		if (flag == TileFlagDefinition.E_TileFlagTag.None)
		{
			TPSingleton<TileMapManager>.Instance.LogError($"Trying to set flag {flag} on tile {tile.X}/{tile.Y} but it should not happen.", CLogLevel.MAJOR);
		}
		else if (TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag.TryGetValue(flag, out value))
		{
			if (!value.Contains(tile))
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag[flag].Add(tile);
			}
		}
		else
		{
			TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag.Add(flag, new List<Tile> { tile });
		}
	}

	[DevConsoleCommand(Name = "TilesFlagsDetails")]
	public static void DebugPrintTilesFlagsDetails()
	{
		if (!TPSingleton<TileMapManager>.Exist())
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Details (flag : total tiles)").AppendLine();
		TileFlagDefinition[] array = TileFlagDefinitions;
		foreach (TileFlagDefinition tileFlagDefinition in array)
		{
			if (TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag.ContainsKey(tileFlagDefinition.TileFlagTag))
			{
				stringBuilder.Append(DebugGetFormattedTileFlagTilesNb(tileFlagDefinition.TileFlagTag)).AppendLine();
			}
		}
		TPSingleton<DebugManager>.Instance.LogDevConsole(stringBuilder);
	}

	public static string DebugGetFormattedTileFlagTilesNb(TileFlagDefinition.E_TileFlagTag tileFlagTag)
	{
		if (!TPSingleton<TileMapManager>.Exist())
		{
			return string.Empty;
		}
		int num = 0;
		if (TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag.ContainsKey(tileFlagTag))
		{
			num = TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag[tileFlagTag].Count;
		}
		return $"{tileFlagTag} : {num}";
	}
}
