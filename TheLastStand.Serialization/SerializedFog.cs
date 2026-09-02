using System;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedFog : ISerializedData
{
	public int FogDensityIndex;

	public int FogDailyUpdateFrequency;

	public bool HasBeenRepelled;
}
