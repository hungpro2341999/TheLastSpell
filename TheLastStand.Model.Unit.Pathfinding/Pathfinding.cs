using System.Collections.Generic;
using TheLastStand.Controller.Unit.Pathfinding;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.Unit.Pathfinding;

public class Pathfinding
{
	public PathfindingController PathfindingController { get; private set; }

	public Dictionary<Tile, int> ReachableTiles { get; set; } = new Dictionary<Tile, int>();

	public Pathfinding(PathfindingController pathfindingController)
	{
		PathfindingController = pathfindingController;
	}
}
