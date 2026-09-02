using System;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedGaugeEffect : ISerializedData
{
	public string Id;

	public int Units;
}
