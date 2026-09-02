using TPLib;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;

namespace TheLastStand.Model.LevelEditor.Conversation;

public class IncreaseLevelSizeCommand : ChangeLevelSizeCommand
{
	public override void Compensate()
	{
		new DecreaseLevelSizeCommand().Execute();
	}

	public override bool Execute()
	{
		ClearTilemaps();
		TheLastStand.Model.TileMap.TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
		TheLastStand.Model.TileMap.TileMap tileMap2 = new TheLastStand.Model.TileMap.TileMap(null, tileMap.TileMapView)
		{
			Width = tileMap.Width + 2,
			Height = tileMap.Height + 2
		};
		tileMap2.Tiles = new Tile[tileMap2.Width * tileMap2.Height];
		for (int i = -1; i < tileMap.Width + 1; i++)
		{
			for (int j = -1; j < tileMap.Height + 1; j++)
			{
				Tile tile = ((i == -1 || j == -1 || i == tileMap.Width || j == tileMap.Height) ? new TileController(i, j, TileDatabase.GroundDefinitions["Dirt"]).Tile : tileMap.Tiles[i * tileMap.Height + j]);
				tile.X++;
				tile.Y++;
				tileMap2.Tiles[(i + 1) * tileMap2.Height + j + 1] = tile;
				tileMap2.TileMapView.DisplayGround(tile, LevelEditorManager.CityToLoadId);
				TileMapView.SetTile(TileMapView.GridTilemap, tile, "View/Tiles/Feedbacks/Grid Cell");
				if (tile.Building == null || tile.Building.OriginTile != tile)
				{
					tile.Building = null;
				}
			}
		}
		TPSingleton<TileMapManager>.Instance.TileMap = tileMap2;
		for (int k = 0; k < tileMap2.Width; k++)
		{
			for (int l = 0; l < tileMap2.Height; l++)
			{
				Tile tile2 = tileMap2.Tiles[k * tileMap2.Height + l];
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
		return "Increase level size";
	}
}
