using System;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedAffix : ISerializedData
{
	public string Id;

	public bool IsEpic;

	public int Level;
}
