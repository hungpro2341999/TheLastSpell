using System.Collections.Generic;

namespace TheLastStand.Serialization.Unit;

public class SerializedEnemyUnits : ISerializedData
{
	public List<SerializedEnemyUnit> EnemyUnits = new List<SerializedEnemyUnit>();

	public List<SerializedEliteEnemyUnit> EliteEnemyUnits = new List<SerializedEliteEnemyUnit>();

	public List<SerializedBonePilesPercentages> BonePilesPercentages = new List<SerializedBonePilesPercentages>();
}
