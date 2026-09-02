using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Stat;

namespace TheLastStand.Controller.Unit.Stat;

public class BossUnitStatsController : EnemyUnitStatsController
{
	public BossUnitStats BossUnitStats => base.UnitStats as BossUnitStats;

	public BossUnitStatsController(BossUnit bossUnit)
		: base(bossUnit)
	{
	}

	public override void Init()
	{
		InitStats();
		ComputeApocalypseStatBaseValueModifier();
		LinkParentStats();
		LinkChildStats();
	}

	protected override void CreateModel(TheLastStand.Model.Unit.Unit unit)
	{
		base.UnitStats = new BossUnitStats(this, unit as BossUnit);
	}
}
