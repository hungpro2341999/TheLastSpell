using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Definition.Unit;
using UnityEngine;

namespace TheLastStand.Model.Unit.Stat;

public class EliteEnemyUnitStat : EnemyUnitStat
{
	public EliteEnemyUnitStat(UnitStatsController statsController, UnitStatDefinition.E_Stat id, Vector2 boundaries)
		: base(statsController, id, boundaries)
	{
	}
}
