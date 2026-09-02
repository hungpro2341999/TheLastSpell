using TheLastStand.Controller;
using TheLastStand.Definition.Building;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.Model;

public class Cursor
{
	public enum E_CursorState
	{
		Undefined,
		Standard,
		Valid,
		Invalid
	}

	private E_CursorState cursorState;

	public BuildingDefinition BuildingToFit { get; set; }

	public CursorController CursorController { get; set; }

	public E_CursorState CursorState
	{
		get
		{
			return cursorState;
		}
		set
		{
			if (cursorState != value)
			{
				cursorState = value;
			}
		}
	}

	public Tile PreviousTile { get; set; }

	public Vector3Int PreviousTilePosition { get; set; }

	public Tile Tile { get; set; }

	public bool TileHasChanged => Tile != PreviousTile;

	public Vector3Int TilePosition { get; set; }

	public Cursor(CursorController cursorController)
	{
		CursorController = cursorController;
		CursorState = E_CursorState.Standard;
		TilePosition = Vector3Int.zero;
	}
}
