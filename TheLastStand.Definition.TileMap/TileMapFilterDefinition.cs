using UnityEngine;

namespace TheLastStand.Definition.TileMap;

[CreateAssetMenu(fileName = "TileMap Filter", menuName = "TLS/Tilemap/TileMapFilter", order = 1)]
public class TileMapFilterDefinition : ScriptableObject
{
	[SerializeField]
	private string id;

	public string Id => id;
}
