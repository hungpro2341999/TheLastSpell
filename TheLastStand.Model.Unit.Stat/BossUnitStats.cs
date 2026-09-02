using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Model.Unit.Stat;

public class BossUnitStats : EnemyUnitStats
{
	public BossUnit BossUnit => base.EnemyUnit as BossUnit;

	public BossUnitStatsController BossUnitStatsController => base.UnitStatsController as BossUnitStatsController;

	public override DamageableType UnitType => DamageableType.Boss;

	public BossUnitStats(EnemyUnitStatsController statsController, BossUnit bossUnit)
		: base(statsController, bossUnit)
	{
	}
}
