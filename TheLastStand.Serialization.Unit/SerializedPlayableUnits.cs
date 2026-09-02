using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedPlayableUnits : ISerializedData
{
	public List<SerializedPlayableUnit> OwnedUnits = new List<SerializedPlayableUnit>();

	public List<SerializedDeadUnit> DeadUnits = new List<SerializedDeadUnit>();

	public SerializedRecruitment Recruitment;
}
