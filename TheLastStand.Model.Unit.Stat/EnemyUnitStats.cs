using System.Collections.Generic;
using System.Linq;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Model.Unit.Stat;

public class EnemyUnitStats : UnitStats
{
	public EnemyUnit EnemyUnit => base.Unit as EnemyUnit;

	public EnemyUnitStatsController EnemyUnitStatsController => base.UnitStatsController as EnemyUnitStatsController;

	public override DamageableType UnitType => DamageableType.Enemy;

	public EnemyUnitStats(UnitStatsController statsController, EnemyUnit enemyUnit)
		: base(statsController, enemyUnit)
	{
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedEnemyUnitStats serializedEnemyUnitStats = container as SerializedEnemyUnitStats;
		base.InjuryStage = serializedEnemyUnitStats.InjuryStage;
		base.Unit.StatusList.ForEach(delegate(TheLastStand.Model.Status.Status o)
		{
			base.UnitStatsController.OnStatusAdded(o);
		});
		EnemyUnitStatsController.InitStatsModifiersFromApocalypse();
		serializedEnemyUnitStats.Stats.ForEach(delegate(SerializedEnemyUnitStat o)
		{
			EnemyUnitStatsController.InitStat(o);
		});
	}

	public override ISerializedData Serialize()
	{
		return new SerializedEnemyUnitStats
		{
			Stats = (from o in base.Stats
				where o.Value.IsChildStat
				select o.Value.Serialize()).Cast<SerializedEnemyUnitStat>().ToList(),
			InjuryStage = base.InjuryStage
		};
	}
}
