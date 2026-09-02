using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedUnitStats : ISerializedData
{
	public List<SerializedUnitStat> Stats;

	public int InjuryStage;
}
