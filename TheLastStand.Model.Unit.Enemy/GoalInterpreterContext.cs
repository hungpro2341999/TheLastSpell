using TPLib;
using TheLastStand.Controller.TileMap;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.Unit.Enemy;

public class GoalInterpreterContext
{
	private Goal goal;

	public Tile TargetCandidateTile { get; set; }

	public float TargetHealth
	{
		get
		{
			if (TargetCandidateTile == null)
			{
				TPSingleton<EnemyUnitManager>.Instance.LogError("The goal has no target");
				return 0f;
			}
			if (TargetCandidateTile.Unit != null)
			{
				return TargetCandidateTile.Unit.Health;
			}
			if (TargetCandidateTile.Building != null && !TargetCandidateTile.Building.BlueprintModule.IsIndestructible)
			{
				return TargetCandidateTile.Building.DamageableModule.Health;
			}
			TPSingleton<EnemyUnitManager>.Instance.LogError("The target has no health");
			return 0f;
		}
	}

	public GoalInterpreterContext(Goal goal)
	{
		this.goal = goal;
	}

	public float Distance(ITileObject other)
	{
		return TileMapController.DistanceBetweenTiles(goal.Owner.OriginTile, other.OriginTile);
	}

	public double Random(double a, double b)
	{
		return RandomManager.GetRandomRange(this, (float)a, (float)b);
	}

	public float Step(double valueToTest, double threshold)
	{
		return (valueToTest >= threshold) ? 1 : 0;
	}
}
