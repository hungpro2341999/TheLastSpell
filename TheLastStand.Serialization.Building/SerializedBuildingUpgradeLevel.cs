using System;

namespace TheLastStand.Serialization.Building;

[Serializable]
public class SerializedBuildingUpgradeLevel : ISerializedData
{
	public int Level;

	public int UpgradeLevel;
}
