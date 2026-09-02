using System;
using TheLastStand.Definition.TileMap;
using UnityEngine;

namespace TheLastStand.Model.TileMap;

[Serializable]
public class TileFlag
{
	[SerializeField]
	private TileFlagDefinition tileFlagDefinition;

	[SerializeField]
	private Vector2Int position = Vector2Int.zero;

	public Vector2Int Position => position;

	public TileFlagDefinition TileFlagDefinition => tileFlagDefinition;
}
