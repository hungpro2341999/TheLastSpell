using System;

namespace TheLastStand.Serialization.Building;

[Serializable]
public class SerializedBuildingAction : ISerializedData
{
	public string Id;

	public int TimesUsed;
}
