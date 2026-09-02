using TPLib;
using TheLastStand.Database.Unit;
using TheLastStand.Manager;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.Unit.Pathfinding;

public class Node
{
	public float DistanceFromStart { get; set; }

	public float DistanceFromTarget { get; set; }

	public Node Parent { get; set; }

	public float Score => (DistanceFromStart + DistanceFromTarget) * Weight;

	public Tile Tile { get; private set; }

	public float Weight { get; private set; } = 1f;

	public Node(Tile tile, float spreadFactor = 1f)
	{
		Tile = tile;
		if (tile.WillBeReached)
		{
			Weight = spreadFactor;
		}
		if (tile.HasFog && TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			Weight *= UnitDatabase.PathfindingDefinition.NodeWeightFogMultiplier;
		}
	}
}
