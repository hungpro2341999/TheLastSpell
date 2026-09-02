using System;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedProducedResource : ISerializedData
{
	public string Id;

	public int Value;
}
