using System;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedEliteEnemyUnit : ASerializedEnemyUnit
{
	public string AffixId;

	public SerializedEliteEnemyUnitStats EliteEnemyUnitStats;

	public SerializedEliteEnemyUnit()
	{
	}

	public SerializedEliteEnemyUnit(SerializedUnit baseUnit)
	{
		Unit = baseUnit;
	}
}
