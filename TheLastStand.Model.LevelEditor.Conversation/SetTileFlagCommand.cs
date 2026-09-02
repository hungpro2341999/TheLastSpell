using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.TileMap;
using TheLastStand.Framework.Command;
using TheLastStand.Framework.Command.Conversation;
using TheLastStand.Manager;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;

namespace TheLastStand.Model.LevelEditor.Conversation;

public class SetTileFlagCommand : ICompensableCommand, ICommand
{
	private readonly TileFlagDefinition flagDefinition;

	private readonly List<Tile> tiles = new List<Tile>();

	private readonly Dictionary<Tile, bool> previousTilesFlagBuffer = new Dictionary<Tile, bool>();

	public bool Erase { get; }

	private SetTileFlagCommand(TileFlagDefinition flagDefinition, bool erase)
	{
		this.flagDefinition = flagDefinition;
		Erase = erase;
	}

	public SetTileFlagCommand(TileFlagDefinition flagDefinition, Tile tile, bool erase)
		: this(flagDefinition, erase)
	{
		RegisterTile(tile);
	}

	public SetTileFlagCommand(TileFlagDefinition flagDefinition, IEnumerable<Tile> tiles, bool erase)
		: this(flagDefinition, erase)
	{
		foreach (Tile tile in tiles)
		{
			RegisterTile(tile);
		}
	}

	public void Compensate()
	{
		if (!Erase)
		{
			foreach (Tile tile in tiles)
			{
				if (!previousTilesFlagBuffer[tile])
				{
					TPSingleton<TileMapView>.Instance.ClearFlagTile(flagDefinition, tile);
					TileMapManager.RemoveTileFlag(tile, flagDefinition.TileFlagTag);
				}
			}
			LevelEditorManager.ClearLastFlagTile();
			return;
		}
		foreach (Tile tile2 in tiles)
		{
			if (previousTilesFlagBuffer[tile2])
			{
				TileMapManager.SetTileFlag(tile2, flagDefinition.TileFlagTag);
				TPSingleton<TileMapView>.Instance.DisplayFlagTile(flagDefinition, tile2);
			}
		}
	}

	public bool Execute()
	{
		if (flagDefinition == null)
		{
			return false;
		}
		if (Erase)
		{
			foreach (Tile tile in tiles)
			{
				TileMapManager.RemoveTileFlag(tile, flagDefinition.TileFlagTag);
				TPSingleton<TileMapView>.Instance.ClearFlagTile(flagDefinition, tile);
			}
		}
		else
		{
			foreach (Tile tile2 in tiles)
			{
				TileMapManager.SetTileFlag(tile2, flagDefinition.TileFlagTag);
				TPSingleton<TileMapView>.Instance.DisplayFlagTile(flagDefinition, tile2);
			}
		}
		return true;
	}

	public override string ToString()
	{
		return $"Setting flag {flagDefinition.TileFlagTag}.";
	}

	private void RegisterTile(Tile tile)
	{
		tiles.Add(tile);
		List<Tile> value2;
		bool value = TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag.TryGetValue(flagDefinition.TileFlagTag, out value2) && value2.Contains(tile);
		previousTilesFlagBuffer.Add(tile, value);
	}
}
