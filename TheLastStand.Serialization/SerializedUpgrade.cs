using System;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedUpgrade : ISerializedData
{
	public string Id;

	public int UpgradeLevel;
}
