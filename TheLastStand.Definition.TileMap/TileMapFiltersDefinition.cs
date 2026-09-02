using UnityEngine;

namespace TheLastStand.Definition.TileMap;

[CreateAssetMenu(fileName = "TileMap Filters", menuName = "TLS/Tilemap/TileMapFilters", order = 1)]
public class TileMapFiltersDefinition : ScriptableObject
{
	[SerializeField]
	private TileMapFilterDefinition[] filterDefinitions;

	public TileMapFilterDefinition[] FiltersDefinitions => filterDefinitions;
}
