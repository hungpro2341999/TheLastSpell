using TPLib;
using TheLastStand.Controller;
using TheLastStand.Definition;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.Unit.Movement;

public class MoveUnitCommand : UnitCommand
{
	private GameDefinition.E_Direction startDirection;

	private readonly bool forceInstant;

	private readonly int movePointsSpent;

	public Task MoveUnitTask { get; private set; }

	public Tile StartTile { get; private set; }

	public MoveUnitCommand(PlayableUnit playableUnit, int movePointsSpent, bool forceInstant)
		: base(playableUnit)
	{
		this.movePointsSpent = movePointsSpent;
		this.forceInstant = forceInstant;
	}

	public override void Compensate()
	{
		base.PlayableUnit.Path.Clear();
		base.PlayableUnit.Path.Add(base.PlayableUnit.OriginTile);
		base.PlayableUnit.Path.Add(StartTile);
		Task task = base.PlayableUnit.PlayableUnitController.PrepareForMovement(playWalkAnim: false, followPathOrientation: false, float.PositiveInfinity, 0f, isMovementInstant: true);
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
		{
			PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
		}
		PlayableUnitManager.MovePath.MovePathController.Clear();
		GameController.SetState(Game.E_State.Wait);
		TPSingleton<PlayableUnitManager>.Instance.MoveUnitsTaskGroup = new TaskGroup(delegate
		{
			GameController.SetState(Game.E_State.Management);
		});
		TPSingleton<PlayableUnitManager>.Instance.MoveUnitsTaskGroup.AddTask(task);
		TPSingleton<PlayableUnitManager>.Instance.MoveUnitsTaskGroup.Run();
		base.PlayableUnit.PlayableUnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.MovePoints, movePointsSpent, includeChildStat: false);
		base.PlayableUnit.PlayableUnitController.AddCrossedTiles(-movePointsSpent);
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			base.PlayableUnit.LifetimeStats.LifetimeStatsController.DecreaseTilesCrossed(movePointsSpent);
		}
		base.PlayableUnit.UnitController.LookAtDirection(startDirection);
	}

	public override bool Execute()
	{
		StartTile = base.PlayableUnit.OriginTile;
		startDirection = base.PlayableUnit.LookDirection;
		base.PlayableUnit.PlayableUnitController.SpendMovePoints(movePointsSpent);
		base.PlayableUnit.PlayableUnitController.AddCrossedTiles(movePointsSpent);
		MoveUnitTask = base.PlayableUnit.PlayableUnitController.PrepareForMovement(playWalkAnim: true, followPathOrientation: true, -1f, 0f, TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day || forceInstant);
		return true;
	}
}
