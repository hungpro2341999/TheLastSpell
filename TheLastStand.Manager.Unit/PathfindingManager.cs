using TPLib;
using TheLastStand.Controller.Unit.Pathfinding;
using TheLastStand.Model.Unit.Pathfinding;

namespace TheLastStand.Manager.Unit;

public class PathfindingManager : Manager<PathfindingManager>
{
	private Pathfinding pathfinding;

	public static Pathfinding Pathfinding => TPSingleton<PathfindingManager>.Instance.pathfinding;

	public void Init()
	{
		pathfinding = new PathfindingController().Pathfinding;
	}
}
