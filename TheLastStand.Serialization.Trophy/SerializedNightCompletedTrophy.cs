using System;

namespace TheLastStand.Serialization.Trophy;

[Serializable]
public class SerializedNightCompletedTrophy : ASerializedTrophy
{
	public int ValueProgression;

	public string CityId;
}
