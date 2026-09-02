using System.Collections.Generic;
using TheLastStand.Controller.Unit.Pathfinding;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Unit.Pathfinding;

namespace TheLastStand.Model.Unit.Pathfinding;

public class MovePath
{
	public enum E_State
	{
		Undefined,
		Valid,
		Invalid,
		Max,
		Away,
		Hidden,
		Teleport
	}

	public static class Constants
	{
		public const string MovePathAngleDownLeftTilePath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Down Left";

		public const string MovePathAngleDownRightTilePath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Down Right";

		public const string MovePathAngleUpLeftTilePath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Up Left";

		public const string MovePathAngleUpRightTilePath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Up Right";

		public const string MovePathArrowDownPath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Down";

		public const string MovePathArrowLeftPath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Left";

		public const string MovePathArrowRightPath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Right";

		public const string MovePathArrowUpPath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Up";

		public const string MovePathHorizontalTilePath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Horizontal";

		public const string MovePathStartDownTilePath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Down";

		public const string MovePathStartLeftTilePath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Left";

		public const string MovePathStartRightTilePath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Right";

		public const string MovePathStartUpTilePath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Up";

		public const string MovePathVerticalTilePath = "View/Tiles/Feedbacks/Movement/MovePath/Move Path Vertical";
	}

	public MovePathController MovePathController { get; }

	public MovePathView MovePathView { get; }

	public int MoveRange { get; set; }

	public List<Tile> Path { get; set; } = new List<Tile>();

	public E_State State { get; set; }

	public MovePath(MovePathController movePathController, MovePathView movePathView)
	{
		MovePathController = movePathController;
		MovePathView = movePathView;
		State = E_State.Undefined;
		Path = new List<Tile>();
	}
}
