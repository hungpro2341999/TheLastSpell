using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;

namespace TheLastStand.Model.LevelEditor.Conversation;

public class DecreaseLevelSizeCommand : ChangeLevelSizeCommand
{
	private List<Tile> firstColumnTiles = new List<Tile>();

	private List<Tile> lastColumnTiles = new List<Tile>();

	private List<Tile> firstRowTiles = new List<Tile>();

	private List<Tile> lastRowTiles = new List<Tile>();

	public override void Compensate()
	{
		new IncreaseLevelSizeCommand().Execute();
		for (int i = 0; i < TPSingleton<TileMapManager>.Instance.TileMap.Width; i++)
		{
			for (int j = 0; j < TPSingleton<TileMapManager>.Instance.TileMap.Height; j++)
			{
				Tile tile = null;
				if (i == 0)
				{
					tile = firstColumnTiles[j];
				}
				else if (i == TPSingleton<TileMapManager>.Instance.TileMap.Width - 1)
				{
					tile = lastColumnTiles[j];
				}
				else if (j == 0)
				{
					tile = firstRowTiles[i - 1];
				}
				else
				{
					if (j != TPSingleton<TileMapManager>.Instance.TileMap.Height - 1)
					{
						continue;
					}
					tile = lastRowTiles[i - 1];
				}
				TileMapManager.GetTile(i, j).GroundDefinition = tile.GroundDefinition;
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayGround(TileMapManager.GetTile(i, j), LevelEditorManager.CityToLoadId);
				TileMapManager.GetTile(i, j).Building = tile.Building;
				if (tile.Building != null && tile.Building.OriginTile == tile)
				{
					BuildingManager.CreateBuilding(tile.Building.BuildingDefinition, TileMapManager.GetTile(i, j), updateView: true, playSound: true, instantly: true, triggerEvent: false);
				}
			}
		}
		firstColumnTiles.Clear();
		lastColumnTiles.Clear();
		firstRowTiles.Clear();
		lastRowTiles.Clear();
	}

	public override bool Execute()
	{
		if (TPSingleton<TileMapManager>.Instance.TileMap.Width < 5 || TPSingleton<TileMapManager>.Instance.TileMap.Height < 5)
		{
			TPSingleton<LevelEditorManager>.Instance.LogWarning("The level is too small to be resized!");
			return false;
		}
		for (int i = 0; i < TPSingleton<TileMapManager>.Instance.TileMap.Width; i++)
		{
			for (int j = 0; j < TPSingleton<TileMapManager>.Instance.TileMap.Height; j++)
			{
				if (i == 0)
				{
					firstColumnTiles.Add(TileMapManager.GetTile(i, j));
				}
				else if (i == TPSingleton<TileMapManager>.Instance.TileMap.Width - 1)
				{
					lastColumnTiles.Add(TileMapManager.GetTile(i, j));
				}
				else if (j == 0)
				{
					firstRowTiles.Add(TileMapManager.GetTile(i, j));
				}
				else if (j == TPSingleton<TileMapManager>.Instance.TileMap.Height - 1)
				{
					lastRowTiles.Add(TileMapManager.GetTile(i, j));
				}
			}
		}
		ClearTilemaps();
		TheLastStand.Model.TileMap.TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
		TheLastStand.Model.TileMap.TileMap tileMap2 = new TheLastStand.Model.TileMap.TileMap(null, tileMap.TileMapView)
		{
			Width = tileMap.Width - 2,
			Height = tileMap.Height - 2
		};
		tileMap2.Tiles = new Tile[tileMap2.Width * tileMap2.Height];
		for (int k = 1; k < tileMap.Width - 1; k++)
		{
			for (int l = 1; l < tileMap.Height - 1; l++)
			{
				Tile tile = tileMap.Tiles[k * tileMap.Height + l];
				tile.X--;
				tile.Y--;
				tileMap2.Tiles[(k - 1) * tileMap2.Height + (l - 1)] = tile;
				tileMap2.TileMapView.DisplayGround(tile, LevelEditorManager.CityToLoadId);
				if (tile.Building == null || tile.Building.OriginTile != tile)
				{
					tile.Building = null;
				}
			}
		}
		TPSingleton<TileMapManager>.Instance.TileMap = tileMap2;
		for (int m = 0; m < tileMap2.Width; m++)
		{
			for (int n = 0; n < tileMap2.Height; n++)
			{
				Tile tile2 = tileMap2.Tiles[m * tileMap2.Height + n];
				TileMapView.SetTile(TileMapView.GridTilemap, tile2, "View/Tiles/Feedbacks/Grid Cell");
				if (tile2.Building != null && tile2.Building.OriginTile == tile2)
				{
					BuildingManager.CreateBuilding(tile2.Building.BuildingDefinition, tile2, updateView: true, playSound: true, instantly: true, triggerEvent: false);
				}
			}
		}
		OnLevelSizeChanged();
		return true;
	}

	public override string ToString()
	{
		return "Decrease level size";
	}
}
