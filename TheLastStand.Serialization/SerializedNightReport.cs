using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedNightReport : ISerializedData
{
	public List<SerializedKillReportData> SerializedKillReportDataContainer;
}
