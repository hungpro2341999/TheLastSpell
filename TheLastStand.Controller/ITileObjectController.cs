using System.Collections.Generic;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller;

public interface ITileObjectController
{
	List<Tile> GetAdjacentTiles();

	List<Tile> GetAdjacentTilesWithDiagonals();

	List<Tile> GetTilesInRange(int maxRange, int minRange = 0, bool cardinalOnly = false);

	Dictionary<Tile, Tile> GetTilesInRangeWithClosestOccupiedTile(int maxRange, int minRange = 0, bool cardinalOnly = false);

	void FreeOccupiedTiles();
}
