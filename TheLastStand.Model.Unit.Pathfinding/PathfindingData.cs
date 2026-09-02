using TheLastStand.Definition.Unit;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.Unit.Pathfinding;

public struct PathfindingData
{
	public AIPathfindingData AIPathfindingData { get; set; }

	public int DistanceFromTargetMax { get; set; }

	public int DistanceFromTargetMin { get; set; }

	public bool IgnoreCanStopOnConstraints { get; set; }

	public int MoveRange { get; set; }

	public UnitTemplateDefinition.E_MoveMethod OverrideUnitMoveMethod { get; set; }

	public PathfindingDefinition.E_PathfindingStyle PathfindingStyle { get; set; }

	public Tile[] TargetTiles { get; set; }

	public Unit Unit { get; set; }
}
