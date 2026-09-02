using System;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedEnemyUnit : ASerializedEnemyUnit
{
	public SerializedEnemyUnitStats EnemyUnitStats;

	public SerializedEnemyUnit()
	{
	}

	public SerializedEnemyUnit(SerializedUnit baseUnit)
	{
		Unit = baseUnit;
	}
}
