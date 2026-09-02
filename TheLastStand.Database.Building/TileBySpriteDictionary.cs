using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheLastStand.Database.Building;

[CreateAssetMenu(fileName = "NewTileBySpriteDictionary", menuName = "TLS/Tile by Sprite Dictionary", order = -800)]
public class TileBySpriteDictionary : SerializedScriptableObject
{
	[SerializeField]
	private string[] spritesFolderFilters;

	[Space(10f)]
	[SerializeField]
	private Dictionary<Sprite, TileBase> manuallySetTilesBySprites = new Dictionary<Sprite, TileBase>();

	[Space(10f)]
	[SerializeField]
	private Dictionary<Sprite, TileBase> tilesBySprites = new Dictionary<Sprite, TileBase>();

	public TileBase GetTileBySprite(Sprite sprite)
	{
		if (sprite != null && tilesBySprites.TryGetValue(sprite, out var value))
		{
			return value;
		}
		return null;
	}
}
