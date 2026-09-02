using System;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedSoundSettings : ISerializedData
{
	public float MasterVolume;

	public float MusicVolume;

	public float UIVolume;

	public float AmbientVolume;
}
