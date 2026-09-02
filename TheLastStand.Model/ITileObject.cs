using System.Collections.Generic;
using TheLastStand.Controller;
using TheLastStand.Definition;
using TheLastStand.Model.TileMap;
using TheLastStand.View;

namespace TheLastStand.Model;

public interface ITileObject
{
	string Id { get; }

	bool IsInCity { get; }

	bool IsInWorld { get; }

	ITileObjectView TileObjectView { get; }

	ITileObjectController TileObjectController { get; }

	ITileObjectDefinition TileObjectDefinition { get; }

	List<Tile> OccupiedTiles { get; }

	Tile OriginTile { get; }

	bool IsTargetableByAI();
}
