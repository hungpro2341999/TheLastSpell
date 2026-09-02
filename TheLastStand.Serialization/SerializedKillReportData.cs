using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization;

[Serializable]
public struct SerializedKillReportData : ISerializedData
{
	public float BaseExperience;

	public string EnemyUnitSpecificId;

	public List<SerializedIntPair> KillAmountPerKillerId;
}
