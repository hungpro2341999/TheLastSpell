using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition;
using TheLastStand.Manager;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.Model.Extensions;

public static class TileExtensions
{
	public static List<Tile> GetOccupiedTiles(this Tile originTile, ITileObjectDefinition tileObjectDefinition)
	{
		if (tileObjectDefinition == null)
		{
			return new List<Tile>(1) { originTile };
		}
		List<Tile> list = new List<Tile>(tileObjectDefinition.Tiles.Count * tileObjectDefinition.Tiles[0].Count);
		Vector2Int vector2Int = new Vector2Int(originTile.X - tileObjectDefinition.OriginX, originTile.Y - tileObjectDefinition.OriginY);
		for (int num = tileObjectDefinition.Tiles.Count - 1; num >= 0; num--)
		{
			for (int i = 0; i < tileObjectDefinition.Tiles[num].Count; i++)
			{
				if (tileObjectDefinition.Tiles[num][i] != Tile.E_UnitAccess.None)
				{
					int x = vector2Int.x + i;
					int y = vector2Int.y + num;
					Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(x, y);
					if (tile != null)
					{
						list.Add(tile);
					}
				}
			}
		}
		return list;
	}

	public static List<Tile> GetTilesInRange(this Tile originTile, ITileObjectDefinition tileObjectDefinition, int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return originTile.GetOccupiedTiles(tileObjectDefinition).GetTilesInRange(maxRange, minRange, cardinalOnly);
	}

	public static List<Tile> GetTilesInRange(this List<Tile> occupiedTiles, int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		HashSet<Tile> tiles = new HashSet<Tile>();
		foreach (Tile occupiedTile in occupiedTiles)
		{
			Action<Tile, Vector2Int> actionOnTile = delegate(Tile tile, Vector2Int distance)
			{
				if (minRange == 0 || !occupiedTiles.Contains(tile))
				{
					tiles.Add(tile);
				}
			};
			int minRange2 = minRange;
			TileMapManager.GoThroughTilesInRange(maxRange, occupiedTile, actionOnTile, cardinalOnly, minRange2);
		}
		return tiles.ToList();
	}

	public static Dictionary<Tile, Tile> GetTilesInRangeWithClosestOccupiedTile(this Tile originTile, ITileObjectDefinition tileObjectDefinition, int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return originTile.GetOccupiedTiles(tileObjectDefinition).GetTilesInRangeWithClosestOccupiedTile(maxRange, minRange, cardinalOnly);
	}

	public static Dictionary<Tile, Tile> GetTilesInRangeWithClosestOccupiedTile(this List<Tile> occupiedTiles, int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		Dictionary<Tile, Tile> tiles = new Dictionary<Tile, Tile>();
		foreach (Tile occupied in occupiedTiles)
		{
			Tile sourceTile = occupied;
			Action<Tile, Vector2Int> actionOnTile = delegate(Tile tile, Vector2Int distance)
			{
				if ((minRange == 0 || !occupiedTiles.Contains(tile)) && (!tiles.ContainsKey(tile) || (tile.Position - tiles[tile].Position).magnitude > (tile.Position - occupied.Position).magnitude))
				{
					tiles[tile] = occupied;
				}
			};
			int minRange2 = minRange;
			TileMapManager.GoThroughTilesInRange(maxRange, sourceTile, actionOnTile, cardinalOnly, minRange2);
		}
		return tiles;
	}

	public static (int distance, Tile tile) GetFirstClosestTile(this Tile tile, List<Tile> tiles)
	{
		(int, Tile) result = (int.MaxValue, null);
		foreach (Tile tile2 in tiles)
		{
			int num = TileMapController.DistanceBetweenTiles(tile2, tile);
			if (num < result.Item1)
			{
				result = (num, tile2);
			}
			if (num == 0)
			{
				return result;
			}
		}
		return result;
	}

	public static (int distance, Tile tile) GetFirstClosestTile(this List<Tile> tiles, Tile tile)
	{
		return tile.GetFirstClosestTile(tiles);
	}

	public static (int distance, Tile tile) GetFirstFarthestTile(this Tile tile, List<Tile> tiles)
	{
		(int, Tile) result = (int.MinValue, null);
		foreach (Tile tile2 in tiles)
		{
			int num = TileMapController.DistanceBetweenTiles(tile2, tile);
			if (num > result.Item1)
			{
				result = (num, tile2);
			}
		}
		return result;
	}

	public static (int distance, Tile tile) GetFirstFarthestTile(this List<Tile> tiles, Tile tile)
	{
		return tile.GetFirstFarthestTile(tiles);
	}

	public static List<Tile> GetAdjacentTiles(this Tile tile)
	{
		List<Tile> list = new List<Tile>();
		Tile tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.X + 1, tile.Y);
		if (tile2 != null)
		{
			list.Add(tile2);
		}
		tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.X - 1, tile.Y);
		if (tile2 != null)
		{
			list.Add(tile2);
		}
		tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.X, tile.Y + 1);
		if (tile2 != null)
		{
			list.Add(tile2);
		}
		tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.X, tile.Y - 1);
		if (tile2 != null)
		{
			list.Add(tile2);
		}
		return list;
	}

	public static List<Tile> GetAdjacentTilesWithDiagonals(this Tile tile)
	{
		List<Tile> list = new List<Tile>();
		Tile tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.X + 1, tile.Y);
		if (tile2 != null)
		{
			list.Add(tile2);
		}
		tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.X - 1, tile.Y);
		if (tile2 != null)
		{
			list.Add(tile2);
		}
		tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.X, tile.Y + 1);
		if (tile2 != null)
		{
			list.Add(tile2);
		}
		tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.X, tile.Y - 1);
		if (tile2 != null)
		{
			list.Add(tile2);
		}
		tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.X + 1, tile.Y + 1);
		if (tile2 != null)
		{
			list.Add(tile2);
		}
		tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.X - 1, tile.Y + 1);
		if (tile2 != null)
		{
			list.Add(tile2);
		}
		tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.X + 1, tile.Y - 1);
		if (tile2 != null)
		{
			list.Add(tile2);
		}
		tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.X - 1, tile.Y - 1);
		if (tile2 != null)
		{
			list.Add(tile2);
		}
		return list;
	}
}
