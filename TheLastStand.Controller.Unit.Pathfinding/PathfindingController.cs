using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Pathfinding;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.Assertions;

namespace TheLastStand.Controller.Unit.Pathfinding;

public class PathfindingController
{
	public TheLastStand.Model.Unit.Pathfinding.Pathfinding Pathfinding { get; private set; }

	public PathfindingController()
	{
		Pathfinding = new TheLastStand.Model.Unit.Pathfinding.Pathfinding(this);
	}

	public static bool CanCross(TheLastStand.Model.Unit.Unit unit, Tile currentTile)
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && unit is PlayableUnit && currentTile.HasFog)
		{
			return false;
		}
		if (!currentTile.IsCrossable)
		{
			return false;
		}
		if (currentTile.Building != null && (currentTile.Building.BlueprintModule.IsIndestructible || !currentTile.Building.DamageableModule.IsDead) && !currentTile.IsCrossableByHeroOnly && !currentTile.IsCrossableByEveryone && currentTile != unit.OriginTile)
		{
			return false;
		}
		if (currentTile.Unit != null && !currentTile.Unit.IsDead && unit.GetType() != currentTile.Unit.GetType())
		{
			return false;
		}
		return true;
	}

	public static List<Tile> ComputeBresenhamLine(Tile originTile, Tile destinationTile)
	{
		List<Tile> list = new List<Tile> { originTile };
		int num = originTile.X;
		int num2 = originTile.Y;
		int x = destinationTile.X;
		int y = destinationTile.Y;
		int num3 = Mathf.Abs(x - num);
		int num4 = ((num < x) ? 1 : (-1));
		int num5 = -Mathf.Abs(y - num2);
		int num6 = ((num2 < y) ? 1 : (-1));
		int num7 = num3 + num5;
		while (num != x || num2 != y)
		{
			int num8 = 2 * num7;
			if (num8 >= num5)
			{
				num7 += num5;
				num += num4;
				list.Add(TPSingleton<TileMapManager>.Instance.TileMap.GetTile(num, num2));
			}
			if (num8 <= num3)
			{
				num7 += num3;
				num2 += num6;
				list.Add(TPSingleton<TileMapManager>.Instance.TileMap.GetTile(num, num2));
			}
		}
		return list;
	}

	public static List<Tile> ComputePath(PathfindingData pathfindingData)
	{
		UnitTemplateDefinition.E_MoveMethod e_MoveMethod = ((pathfindingData.OverrideUnitMoveMethod == UnitTemplateDefinition.E_MoveMethod.Undefined) ? pathfindingData.Unit.UnitTemplateDefinition.MoveMethod : pathfindingData.OverrideUnitMoveMethod);
		List<Tile> list = null;
		Dictionary<Tile, Node> dictionary = null;
		if (pathfindingData.PathfindingStyle == PathfindingDefinition.E_PathfindingStyle.Bresenham)
		{
			list = ComputeBresenhamLine(pathfindingData.Unit.OriginTile, pathfindingData.TargetTiles[0]);
			dictionary = new Dictionary<Tile, Node>();
		}
		List<Node> list2 = new List<Node>
		{
			new Node(pathfindingData.Unit.OriginTile)
		};
		list2[0].DistanceFromTarget = ComputeDistanceFromTargets(list2[0], pathfindingData.TargetTiles, pathfindingData.PathfindingStyle);
		List<Node> list3 = new List<Node>();
		Dictionary<Tile, Node> dictionary2 = new Dictionary<Tile, Node> { 
		{
			pathfindingData.Unit.OriginTile,
			list2[0]
		} };
		Node value = null;
		int num = 0;
		Tile tile = null;
		Node value2 = null;
		List<Tile> path = new List<Tile>();
		while (list2.Count > 0)
		{
			List<Tile> list4 = null;
			if (value != null && pathfindingData.PathfindingStyle == PathfindingDefinition.E_PathfindingStyle.Bresenham)
			{
				int num2 = list.IndexOf(value.Tile);
				if (num2 != -1 && num2 != list.Count - 1)
				{
					Tile key = list[num2 + 1];
					dictionary.TryGetValue(key, out value);
				}
			}
			if (value == null || list3.Contains(value))
			{
				float lowestScore = list2.Min((Node node2) => node2.Score);
				value = list2.First((Node node2) => TPHelpers.IsApproxEqual(lowestScore, node2.Score));
			}
			list3.Add(value);
			bool flag = false;
			if (pathfindingData.AIPathfindingData != null && pathfindingData.AIPathfindingData.InterruptionCondition != GoalDefinition.E_InterruptionCondition.None)
			{
				list4 = value.Tile.GetAdjacentTiles();
				flag = MustBeInterrupted(pathfindingData, list4);
			}
			float num3 = (flag ? 0f : ComputeDistanceFromTargets(value, pathfindingData.TargetTiles));
			if (flag || (num3 >= (float)pathfindingData.DistanceFromTargetMin && num3 <= (float)pathfindingData.DistanceFromTargetMax && (pathfindingData.IgnoreCanStopOnConstraints || pathfindingData.Unit.CanStopOn(value.Tile))))
			{
				bool flag2 = true;
				if (!flag && pathfindingData.AIPathfindingData != null && pathfindingData.AIPathfindingData.SkillExecution != null)
				{
					if (pathfindingData.AIPathfindingData.SkillExecution.Skill.SkillDefinition.CardinalDirectionOnly || !pathfindingData.AIPathfindingData.SkillExecution.Skill.SkillAction.HasEffect("IgnoreLineOfSight") || pathfindingData.AIPathfindingData.SkillExecution.Skill.SkillDefinition.Range.x > 1)
					{
						pathfindingData.AIPathfindingData.SkillExecution.SkillExecutionController.PrepareSkill(pathfindingData.Unit, value.Tile);
					}
					flag2 = pathfindingData.AIPathfindingData.SkillExecution.Skill.SkillController.HasAtLeastOneTileInRange(value.Tile, pathfindingData.TargetTiles) && (pathfindingData.AIPathfindingData.SkillExecution.Skill.SkillAction.HasEffect("IgnoreLineOfSight") || pathfindingData.AIPathfindingData.SkillExecution.SkillExecutionController.HasAtLeastOneTileInLineOfSight(pathfindingData.TargetTiles, value.Tile));
				}
				if (flag2)
				{
					bool flag3 = true;
					if (EnemyUnitManager.CheckValidPathMoveRange)
					{
						for (Node node = value; node != null; node = node.Parent)
						{
							path.Insert(0, node.Tile);
						}
						while (pathfindingData.MoveRange != -1 && path.Count - 1 > pathfindingData.MoveRange)
						{
							path.RemoveAt(path.Count - 1);
						}
						if (pathfindingData.IgnoreCanStopOnConstraints || pathfindingData.Unit.CanStopOn(path[path.Count - 1]))
						{
							return path;
						}
						path.Clear();
						flag3 = false;
					}
					if (flag3)
					{
						break;
					}
				}
			}
			list2.Remove(value);
			if (list4 == null)
			{
				list4 = value.Tile.GetAdjacentTiles();
			}
			for (int num4 = 0; num4 < list4.Count; num4++)
			{
				tile = list4[num4];
				if (!pathfindingData.Unit.CanTravelThrough(tile, e_MoveMethod))
				{
					continue;
				}
				if (!dictionary2.TryGetValue(tile, out value2))
				{
					float spreadFactor = 1f;
					if (pathfindingData.AIPathfindingData != null && value.DistanceFromStart < (float)pathfindingData.MoveRange)
					{
						spreadFactor = pathfindingData.AIPathfindingData.SpreadFactor;
					}
					value2 = new Node(tile, spreadFactor);
					dictionary2.Add(tile, value2);
				}
				if (list3.Contains(value2))
				{
					continue;
				}
				if (!list2.Contains(value2))
				{
					value2.DistanceFromStart = value.DistanceFromStart + 1f;
					value2.DistanceFromTarget = ComputeDistanceFromTargets(value2, pathfindingData.TargetTiles, pathfindingData.PathfindingStyle);
					value2.Parent = value;
					list2.Insert(0, value2);
					if (list != null && list.Contains(tile) && !dictionary.ContainsKey(tile))
					{
						dictionary.Add(tile, value2);
					}
				}
				else if (value.DistanceFromStart + 1f + value2.DistanceFromTarget < value2.Score)
				{
					value2.DistanceFromStart = value.DistanceFromStart + 1f;
					value2.Parent = value;
				}
			}
			if (pathfindingData.AIPathfindingData != null && ++num >= pathfindingData.AIPathfindingData.ThinkingScope)
			{
				break;
			}
		}
		if (pathfindingData.AIPathfindingData != null && (num >= pathfindingData.AIPathfindingData.ThinkingScope || !pathfindingData.TargetTiles.Contains(list3[^1].Tile)))
		{
			float lowestDistance = list3.Min((Node node2) => node2.DistanceFromTarget);
			value = list3.FirstOrDefault((Node node2) => node2.DistanceFromTarget == lowestDistance);
		}
		else
		{
			value = list3[^1];
		}
		while (value != null)
		{
			path.Insert(0, value.Tile);
			value = value.Parent;
		}
		if (pathfindingData.MoveRange != -1 && path.Count - 1 > pathfindingData.MoveRange)
		{
			path.RemoveRange(pathfindingData.MoveRange + 1, path.Count - 1 - pathfindingData.MoveRange);
		}
		HashSet<Tile> triedTiles = new HashSet<Tile>(path);
		Tile sourceTile = path[path.Count - 1];
		if (pathfindingData.MoveRange > 0)
		{
			while (!pathfindingData.IgnoreCanStopOnConstraints && !pathfindingData.Unit.CanStopOn(path[path.Count - 1]))
			{
				Tile tile2 = path[path.Count - 1];
				path.RemoveAt(path.Count - 1);
				try
				{
				}
				catch (AssertionException)
				{
					TPSingleton<PathfindingManager>.Instance.LogError("!!\r\n" + $"Unit {pathfindingData.Unit.UniqueIdentifier} can't stop on {tile2}" + "\r\n" + $"Move range = {pathfindingData.MoveRange}" + "\r\n" + $"Pathfinding data ignores can stop on constraints : {pathfindingData.IgnoreCanStopOnConstraints}" + "\r\n" + $"Unit can stop here : {pathfindingData.Unit.CanStopOn(tile2)}.\r\n" + "What's on the tile ? " + (tile2.IsEmpty() ? "Nothing" : (tile2.Unit.UniqueIdentifier + ")")) + ".", CLogLevel.DETAILED);
					TPSingleton<PathfindingManager>.Instance.LogError($"Will the tile be reached : {tile2.WillBeReached}. By whom : {tile2.WillBeReachedBy}");
				}
			}
		}
		if (path.Count == 1 && e_MoveMethod == UnitTemplateDefinition.E_MoveMethod.Flying)
		{
			TileMapManager.GoThroughTilesInRangeUntil(pathfindingData.MoveRange, sourceTile, delegate(Tile currentTile, Vector2Int position)
			{
				if (triedTiles.Contains(currentTile))
				{
					return false;
				}
				triedTiles.Add(currentTile);
				if (Mathf.Abs(position.x) + Mathf.Abs(position.y) > pathfindingData.MoveRange)
				{
					return false;
				}
				if (pathfindingData.Unit.CanStopOn(currentTile))
				{
					path = ComputeBresenhamLine(pathfindingData.Unit.OriginTile, currentTile);
					return true;
				}
				return false;
			});
		}
		return path;
	}

	public void ClearReachableTiles(bool clearView = true)
	{
		if (clearView)
		{
			TileMapView.ClearTiles(TileMapView.ReachableTilesTilemap);
		}
		Pathfinding.ReachableTiles.Clear();
	}

	public void ComputeReachableTiles(TheLastStand.Model.Unit.Unit unit, int movePoints = -1)
	{
		Pathfinding.ReachableTiles.Clear();
		ComputeReachableTiles(unit, unit.OriginTile, movePoints);
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night || !(unit is PlayableUnit))
		{
			return;
		}
		foreach (KeyValuePair<Tile, int> reachableTile in Pathfinding.ReachableTiles)
		{
			if (unit.OriginTile != reachableTile.Key)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayReachableTile(reachableTile.Key);
			}
		}
	}

	public void AddAllReachableTilesToPath(TheLastStand.Model.Unit.Unit unit)
	{
		Pathfinding.ReachableTiles.Clear();
		Pathfinding.ReachableTiles = TPSingleton<TileMapManager>.Instance.TileMap.Tiles.Where((Tile tile) => CanCross(unit, tile)).ToDictionary((Tile tile) => tile, (Tile tile) => 0);
	}

	private static float ComputeDistanceFromTargets(Node node, Tile[] targetTiles, PathfindingDefinition.E_PathfindingStyle pathfindingStyle = PathfindingDefinition.E_PathfindingStyle.Manhattan)
	{
		float num = 2.1474836E+09f;
		foreach (Tile tile in targetTiles)
		{
			num = Mathf.Min(num, EstimateDistanceFromTarget(node.Tile.X, node.Tile.Y, tile.X, tile.Y, pathfindingStyle));
		}
		return num;
	}

	private static float EstimateDistanceFromTarget(int x, int y, int targetX, int targetY, PathfindingDefinition.E_PathfindingStyle pathfindingStyle = PathfindingDefinition.E_PathfindingStyle.Manhattan)
	{
		switch (pathfindingStyle)
		{
		case PathfindingDefinition.E_PathfindingStyle.Manhattan:
			return Mathf.Abs(targetX - x) + Mathf.Abs(targetY - y);
		case PathfindingDefinition.E_PathfindingStyle.Hypotenuse:
		case PathfindingDefinition.E_PathfindingStyle.Bresenham:
			return Mathf.Sqrt(Mathf.Pow(Mathf.Abs(targetX - x), 2f) + Mathf.Pow(Mathf.Abs(targetY - y), 2f));
		default:
			return -1f;
		}
	}

	private static bool MustBeInterrupted(PathfindingData pathfindingData, List<Tile> adjacentTiles)
	{
		GoalDefinition.E_InterruptionCondition interruptionCondition = pathfindingData.AIPathfindingData.InterruptionCondition;
		for (int num = adjacentTiles.Count - 1; num >= 0; num--)
		{
			Tile tile = adjacentTiles[num];
			if (tile != null)
			{
				if (tile.Unit != null && tile.Unit != pathfindingData.Unit)
				{
					if (interruptionCondition == GoalDefinition.E_InterruptionCondition.AdjacentToUnit)
					{
						return true;
					}
					if (tile.Unit is PlayableUnit && (interruptionCondition & GoalDefinition.E_InterruptionCondition.AdjacentToPlayableUnit) != GoalDefinition.E_InterruptionCondition.None)
					{
						return true;
					}
					if (tile.Unit is EnemyUnit && (interruptionCondition & GoalDefinition.E_InterruptionCondition.AdjacentToEnemyUnit) != GoalDefinition.E_InterruptionCondition.None)
					{
						return true;
					}
				}
				if (tile.Building != null)
				{
					if (interruptionCondition == GoalDefinition.E_InterruptionCondition.AdjacentToBuilding)
					{
						return true;
					}
					if (!tile.Building.IsWall && !tile.Building.IsBarricade && (interruptionCondition & GoalDefinition.E_InterruptionCondition.AdjacentToBuildingExceptWallAndBarricade) != GoalDefinition.E_InterruptionCondition.None)
					{
						return true;
					}
					if (tile.Building.IsWall && (interruptionCondition & GoalDefinition.E_InterruptionCondition.AdjacentToWall) != GoalDefinition.E_InterruptionCondition.None)
					{
						return true;
					}
					if (tile.Building.IsBarricade && (interruptionCondition & GoalDefinition.E_InterruptionCondition.AdjacentToBarricade) != GoalDefinition.E_InterruptionCondition.None)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void ComputeReachableTiles(TheLastStand.Model.Unit.Unit unit, Tile currentTile, int moveRange)
	{
		if (!CanCross(unit, currentTile))
		{
			return;
		}
		if (moveRange != -1)
		{
			if (currentTile != unit.OriginTile)
			{
				moveRange--;
			}
			if (moveRange < 0)
			{
				return;
			}
		}
		if (Pathfinding.ReachableTiles.TryGetValue(currentTile, out var value))
		{
			if (value >= moveRange)
			{
				return;
			}
			Pathfinding.ReachableTiles[currentTile] = moveRange;
		}
		else
		{
			Pathfinding.ReachableTiles.Add(currentTile, moveRange);
		}
		for (int i = currentTile.X - 1; i <= currentTile.X + 1; i++)
		{
			for (int j = currentTile.Y - 1; j <= currentTile.Y + 1; j++)
			{
				if ((i != currentTile.X || j != currentTile.Y) && (i == currentTile.X || j == currentTile.Y) && i >= 0 && i < TPSingleton<TileMapManager>.Instance.TileMap.Width && j >= 0 && j < TPSingleton<TileMapManager>.Instance.TileMap.Height)
				{
					Tile tile = TileMapManager.GetTile(i, j);
					if (tile != null)
					{
						ComputeReachableTiles(unit, tile, moveRange);
					}
				}
			}
		}
	}
}
