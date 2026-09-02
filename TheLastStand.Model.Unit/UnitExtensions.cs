using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Model.Unit;

public static class UnitExtensions
{
	public static bool IsAlly(this Unit unit, Unit otherUnit)
	{
		if (!(unit is PlayableUnit) || !(otherUnit is PlayableUnit))
		{
			if (unit is EnemyUnit)
			{
				return otherUnit is EnemyUnit;
			}
			return false;
		}
		return true;
	}
}
