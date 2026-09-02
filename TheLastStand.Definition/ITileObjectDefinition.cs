using System.Collections.Generic;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Definition;

public interface ITileObjectDefinition
{
	int OriginX { get; }

	int OriginY { get; }

	List<List<Tile.E_UnitAccess>> Tiles { get; }
}
