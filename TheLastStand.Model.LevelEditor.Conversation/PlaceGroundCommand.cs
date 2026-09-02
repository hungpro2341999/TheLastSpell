using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Definition;
using TheLastStand.Framework.Command;
using TheLastStand.Framework.Command.Conversation;
using TheLastStand.Manager;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.LevelEditor.Conversation;

public class PlaceGroundCommand : ICompensableCommand, ICommand
{
	private GroundDefinition groundDefinition;

	private Dictionary<Tile, GroundDefinition> previousGroundDefinitions = new Dictionary<Tile, GroundDefinition>();

	private List<Tile> tilesToPaint = new List<Tile>();

	public PlaceGroundCommand(GroundDefinition groundDefinition, Tile tile, bool useBucketTool = false)
	{
		this.groundDefinition = groundDefinition;
		if (this.groundDefinition != tile.GroundDefinition)
		{
			if (useBucketTool)
			{
				SetToPaintWithAdjacent(tile);
				return;
			}
			previousGroundDefinitions.Add(tile, tile.GroundDefinition);
			tilesToPaint.Add(tile);
		}
	}

	public PlaceGroundCommand(GroundDefinition groundDefinition, IEnumerable<Tile> tiles)
	{
		this.groundDefinition = groundDefinition;
		previousGroundDefinitions.Clear();
		foreach (Tile tile in tiles)
		{
			previousGroundDefinitions.Add(tile, tile.GroundDefinition);
		}
		tilesToPaint.AddRange(tiles);
	}

	public void Compensate()
	{
		foreach (Tile item in tilesToPaint)
		{
			item.GroundDefinition = previousGroundDefinitions[item];
			TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayGround(item, LevelEditorManager.CityToLoadId);
		}
	}

	public bool Execute()
	{
		if (previousGroundDefinitions.Where((KeyValuePair<Tile, GroundDefinition> o) => o.Value == groundDefinition).Count() == previousGroundDefinitions.Count)
		{
			return false;
		}
		foreach (Tile item in tilesToPaint)
		{
			item.GroundDefinition = groundDefinition;
			TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayGround(item, LevelEditorManager.CityToLoadId);
		}
		return true;
	}

	private void SetToPaintWithAdjacent(Tile tile)
	{
		tilesToPaint.Add(tile);
		previousGroundDefinitions.Add(tile, tile.GroundDefinition);
		foreach (Tile adjacentTile in tile.GetAdjacentTiles())
		{
			if (!tilesToPaint.Contains(adjacentTile) && adjacentTile.GroundDefinition == tile.GroundDefinition)
			{
				SetToPaintWithAdjacent(adjacentTile);
			}
		}
	}

	public override string ToString()
	{
		return "Place ground " + groundDefinition.Id;
	}
}
