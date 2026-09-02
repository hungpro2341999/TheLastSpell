using System;
using TheLastStand.Definition.Unit;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedUnitStat : ISerializedData
{
	public UnitStatDefinition.E_Stat StatId;

	public float Base;
}
