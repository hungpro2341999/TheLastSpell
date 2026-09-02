using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Model.Unit.Stat;

public class EliteEnemyUnitStats : EnemyUnitStats
{
	public EliteEnemyUnit EliteEnemyUnit => base.Unit as EliteEnemyUnit;

	public EliteEnemyUnitStatsController EliteEnemyUnitStatsController => base.UnitStatsController as EliteEnemyUnitStatsController;

	public override DamageableType UnitType => DamageableType.Enemy;

	public EliteEnemyUnitStats(UnitStatsController statsController, EliteEnemyUnit enemyUnit)
		: base(statsController, enemyUnit)
	{
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedEliteEnemyUnitStats serializedEliteEnemyUnitStats = container as SerializedEliteEnemyUnitStats;
		base.Deserialize((ISerializedData)serializedEliteEnemyUnitStats.Base, -1);
	}

	public override ISerializedData Serialize()
	{
		return new SerializedEliteEnemyUnitStats
		{
			Base = (base.Serialize() as SerializedEnemyUnitStats)
		};
	}
}
