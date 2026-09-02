using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedRandoms : ISerializedData
{
	public int BaseSeed;

	public List<SerializedRandom> RandomLibrary = new List<SerializedRandom>();
}
