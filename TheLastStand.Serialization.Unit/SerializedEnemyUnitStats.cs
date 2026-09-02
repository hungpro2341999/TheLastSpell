using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedEnemyUnitStats : ISerializedData
{
	public List<SerializedEnemyUnitStat> Stats;

	public int InjuryStage;
}
