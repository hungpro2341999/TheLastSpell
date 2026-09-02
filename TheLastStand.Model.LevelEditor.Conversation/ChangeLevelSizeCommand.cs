using TPLib;
using TheLastStand.Framework.Command;
using TheLastStand.Framework.Command.Conversation;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Model.LevelEditor.Conversation;

public abstract class ChangeLevelSizeCommand : ICompensableCommand, ICommand
{
	public ChangeLevelSizeCommand()
	{
	}

	public abstract void Compensate();

	public abstract bool Execute();

	protected void ClearTilemaps()
	{
		BuildingManager.ClearBuildings();
		TileMapView.GridTilemap.ClearAllTiles();
		TileMapView.BuildingTilemap.ClearAllTiles();
		TileMapView.BuildingMasksTilemap.ClearAllTiles();
		TileMapView.BuildingShadowsTilemap.ClearAllTiles();
		TileMapView.GroundCityTilemap.ClearAllTiles();
		TileMapView.GroundCraterTilemap.ClearAllTiles();
		TileMapView.SideWalksTilemap.ClearAllTiles();
		TileMapView.SideWalkShadowsTilemap.ClearAllTiles();
		TileMapView.WorldLimitsTilemap.ClearAllTiles();
	}

	protected void OnLevelSizeChanged()
	{
		for (int i = -1; i <= TPSingleton<TileMapManager>.Instance.TileMap.Width; i++)
		{
			for (int j = -1; j <= TPSingleton<TileMapManager>.Instance.TileMap.Height; j++)
			{
				TPSingleton<TileMapView>.Instance.SetWorldLimitTile(new Vector3Int(i, j, 0));
			}
		}
		LevelEditorManager.CenterLevelFeedback();
		LevelEditorManager.RefreshFogPreview(LevelEditorManager.FogDisplayed);
	}
}
