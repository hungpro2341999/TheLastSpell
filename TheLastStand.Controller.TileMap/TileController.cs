using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Definition;
using TheLastStand.Definition.Building;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Maths;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Controller.TileMap;

public class TileController : ITileObjectController
{
	public enum E_SegmentEndsComputationType
	{
		Center,
		OppositeCorners,
		CenterToOppositeCorner
	}

	public Tile Tile { get; }

	public TileController(int x, int y, GroundDefinition groundDefinition)
	{
		TileView tileView = Object.Instantiate(TileMapManager.TileViewPrefab, TileMapManager.TileViewsTransform);
		tileView.transform.position = TileMapView.GetWorldPosition(new Vector2Int(x, y));
		Tile = new Tile(this, tileView, x, y, groundDefinition);
		tileView.Tile = Tile;
	}

	public static void ComputeSegmentEndsPositions(Tile sourceTile, Tile destTile, E_SegmentEndsComputationType computationType, out Vector2 sourcePos, out Vector2 destPos)
	{
		Vector2Int vector2Int = destTile.Position - sourceTile.Position;
		switch (computationType)
		{
		case E_SegmentEndsComputationType.OppositeCorners:
			if (vector2Int.x > 0)
			{
				if (vector2Int.y > 0)
				{
					sourcePos = sourceTile.Position;
					destPos = destTile.Position + Vector2Int.one;
				}
				else
				{
					sourcePos = sourceTile.Position + Vector2Int.up;
					destPos = destTile.Position + Vector2Int.right;
				}
			}
			else if (vector2Int.y > 0)
			{
				sourcePos = sourceTile.Position + Vector2Int.right;
				destPos = destTile.Position + Vector2Int.up;
			}
			else
			{
				sourcePos = sourceTile.Position + Vector2Int.one;
				destPos = destTile.Position;
			}
			break;
		case E_SegmentEndsComputationType.CenterToOppositeCorner:
			sourcePos = sourceTile.Position + Vector2.up * 0.5f + Vector2.right * 0.5f;
			if (vector2Int.x > 0)
			{
				if (vector2Int.y > 0)
				{
					destPos = destTile.Position + Vector2Int.one;
				}
				else
				{
					destPos = destTile.Position + Vector2Int.right;
				}
			}
			else if (vector2Int.y > 0)
			{
				destPos = destTile.Position + Vector2Int.up;
			}
			else
			{
				destPos = destTile.Position;
			}
			break;
		default:
			sourcePos = sourceTile.Position + Vector2.up * 0.5f + Vector2.right * 0.5f;
			destPos = destTile.Position + Vector2.up * 0.5f + Vector2.right * 0.5f;
			break;
		}
	}

	public bool TryGenerateBonePile(Dictionary<string, int> percentages)
	{
		if (Tile.HasFog || !Tile.IsCrossable || TPSingleton<BuildingManager>.Instance.WillTileBeUsedForRandomBuilding(Tile) || (Tile.Building != null && GenericDatabase.IdsListDefinitions.TryGetValue("BlockingBonePilesBuildings", out var value) && value.Ids.Contains(Tile.Building.Id)))
		{
			return false;
		}
		foreach (KeyValuePair<string, int> building in BonePileDatabase.BonePileGeneratorsDefinition.Buildings)
		{
			if (!percentages.TryGetValue(building.Key, out var value2) || value2 == 0 || value2 < building.Value)
			{
				continue;
			}
			if (TPSingleton<BuildingManager>.Instance.BonePileGenerationLimits.Count > 0)
			{
				int num = TPSingleton<BuildingManager>.Instance.BonePileGenerationLimits[building.Key];
				int value3;
				int num2 = (TPSingleton<BuildingManager>.Instance.BonePileGenerationCounter.TryGetValue(building.Key, out value3) ? value3 : 0);
				if (num > -1 && num2 >= num)
				{
					continue;
				}
			}
			int randomRange = RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, 100);
			if (value2 >= randomRange)
			{
				GenerateBonePile(building.Key);
				return true;
			}
		}
		return false;
	}

	public void GenerateBonePile(string buildingId)
	{
		if (Tile.Building != null && (Tile.Building.IsTrap || Tile.Building.IsWalkableHandledDefense) && !TPSingleton<BuildingManager>.Instance.BuildingsToRestore.ContainsKey(Tile))
		{
			BuildingToRestore value = new BuildingToRestore(Tile, Tile.Building.Id, Tile.Building.BattleModule.RemainingTrapCharges);
			TPSingleton<BuildingManager>.Instance.BuildingsToRestore.Add(Tile, value);
		}
		BuildingDefinition buildingDefinition = BuildingDatabase.BuildingDefinitions[buildingId];
		if (Tile.Building?.BuildingView != null && Tile.Building.BuildingView.HandledDefensesHUD != null)
		{
			Tile.Building.BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: false);
		}
		TileMapManager.ClearBuildingOnTiles(Tile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition));
		BuildingManager.CreateBuilding(buildingDefinition, Tile, updateView: true, playSound: false, instantly: false, triggerEvent: true, null, recomputeReachableTiles: true, isGeneratingBonePile: true);
		TPSingleton<BuildingManager>.Instance.BonePileGenerationCounter.AddValueOrCreateKey(buildingId, 1, (int a, int b) => a + b);
		CleanDeadBodies(instant: true);
	}

	public void AddDeadBody(EnemyUnit enemy)
	{
		Tile.TileView.InstantiateDeadBody(enemy);
	}

	public void AddDeadBuilding(BuildingDefinition building)
	{
		Tile.TileView.InstantiateDeadBuilding(building);
	}

	public void AddForbiddenBuildingCategory(BuildingDefinition.E_BuildingCategory buildingCategory)
	{
		if (!Tile.ForbiddenBuildingFromCategory.Contains(buildingCategory))
		{
			Tile.ForbiddenBuildingFromCategory.Add(buildingCategory);
		}
	}

	public void ClearForbiddenBuildingCategory()
	{
		Tile.ForbiddenBuildingFromCategory.Clear();
	}

	public int ComputeDistanceToCity()
	{
		if (Tile.IsCityTile)
		{
			Tile.DistanceToCity = 0;
			return Tile.DistanceToCity;
		}
		Tile closestTileFillingConditions = TileMapManager.GetClosestTileFillingConditions(TPSingleton<TileMapManager>.Instance.TileMap.Width, Tile, (Tile tile) => tile.IsCityTile);
		Tile.DistanceToCity = TileMapController.DistanceBetweenTiles(Tile, closestTileFillingConditions);
		return Tile.DistanceToCity;
	}

	public int ComputeDistanceToMagicCircle()
	{
		if (Tile.Building is MagicCircle)
		{
			Tile.DistanceToMagicCircle = 0;
			return Tile.DistanceToMagicCircle;
		}
		Tile tile = BuildingManager.MagicCircle.OccupiedTiles.OrderBy((Tile o) => Mathf.Abs(Tile.X - o.X) + Mathf.Abs(Tile.Y - o.Y)).First();
		Tile.DistanceToMagicCircle = TileMapController.DistanceBetweenTiles(Tile, tile);
		return Tile.DistanceToMagicCircle;
	}

	public bool CheckIsInBuildingOccupationVolume()
	{
		int buildingMaxDeadZoneRange = BuildingManager.GetBuildingMaxDeadZoneRange();
		for (int i = -buildingMaxDeadZoneRange; i <= buildingMaxDeadZoneRange; i++)
		{
			for (int j = -buildingMaxDeadZoneRange; j <= buildingMaxDeadZoneRange; j++)
			{
				Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(Tile.Position.x + i, Tile.Position.y + j);
				if (tile != null && tile != Tile && tile.Building != null && (tile.Building.BlueprintModule.IsIndestructible || !tile.Building.DamageableModule.IsDead) && tile.Building.BuildingDefinition.ConstructionModuleDefinition.OccupationVolumeType == BuildingDefinition.E_OccupationVolumeType.Adjacent)
				{
					int num = Mathf.Max(Mathf.Abs(i), Mathf.Abs(j));
					int buildingDeadZoneRange = BuildingManager.GetBuildingDeadZoneRange(tile.Building.Id);
					if (num <= buildingDeadZoneRange && buildingDeadZoneRange > 0)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void CleanDeadBodies(bool instant = false)
	{
		if (Tile.EnemyUnitDeadBodyViews.Count == 0)
		{
			return;
		}
		List<EnemyUnitDeadBodyView> list = new List<EnemyUnitDeadBodyView>(Tile.EnemyUnitDeadBodyViews);
		for (int i = 0; i < list.Count; i++)
		{
			EnemyUnitDeadBodyView enemyUnitDeadBodyView = list[i];
			if (!(enemyUnitDeadBodyView == null))
			{
				if (instant)
				{
					enemyUnitDeadBodyView.ForceDisappear();
				}
				else
				{
					enemyUnitDeadBodyView.StartWaitDisappear();
				}
			}
		}
	}

	public void FreeOccupiedTiles()
	{
	}

	public List<Tile> GetAdjacentTiles()
	{
		return Tile.GetAdjacentTiles();
	}

	public List<Tile> GetAdjacentTilesWithDiagonals()
	{
		return Tile.GetAdjacentTilesWithDiagonals();
	}

	public List<Tile> GetTilesInRange(int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return Tile.OccupiedTiles.GetTilesInRange(maxRange, minRange, cardinalOnly);
	}

	public Dictionary<Tile, Tile> GetTilesInRangeWithClosestOccupiedTile(int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return Tile.OccupiedTiles.GetTilesInRangeWithClosestOccupiedTile(maxRange, minRange, cardinalOnly);
	}

	public void SetBuilding(TheLastStand.Model.Building.Building building)
	{
		Tile.Building = building;
		Tile.WillBeReachedBy = null;
	}

	public void SetOccupiedByBuildingVolume(bool isInBuildingVolume)
	{
		Tile.IsInBuildingVolume = isInBuildingVolume;
	}

	public void SetUnit(TheLastStand.Model.Unit.Unit unit)
	{
		if (unit == null)
		{
			Tile.WillBeReachedBy = null;
		}
		Tile.Unit = unit;
	}

	public bool SegmentTileIntersection(Vector2 segmentOrigin, Vector2 segmentDestination, float tolerance = 0f)
	{
		Vector2[] array = new Vector2[8]
		{
			Tile.Position,
			Tile.Position + Vector2.up,
			Tile.Position,
			Tile.Position + Vector2.right,
			Tile.Position + Vector2.right,
			Tile.Position + Vector2.right + Vector2.up,
			Tile.Position + Vector2.up,
			Tile.Position + Vector2.up + Vector2.right
		};
		Vector2 vector = TPHelpers.__VECTOR2_ERROR;
		for (int i = 0; i < 4; i++)
		{
			bool hit;
			Vector2 vector2 = Maths.LineSegmentsIntersection(segmentOrigin, segmentDestination, array[i * 2], array[i * 2 + 1], out hit);
			if (hit)
			{
				float magnitude = (array[i * 2] - vector2).magnitude;
				float magnitude2 = (array[i * 2 + 1] - vector2).magnitude;
				if (magnitude <= tolerance)
				{
					vector2 = array[i * 2];
				}
				else if (magnitude2 <= tolerance)
				{
					vector2 = array[i * 2 + 1];
				}
				if ((magnitude > tolerance && magnitude2 > tolerance) || (vector2 != vector && vector != TPHelpers.__VECTOR2_ERROR))
				{
					return true;
				}
				vector = vector2;
			}
		}
		return false;
	}
}
