using System.Collections.Generic;
using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization.Building;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class BuildingUpgradeLevel : ILevelOwner, ISerializable, IDeserializable
{
	public HashSet<BuildingGlobalUpgrade> BuildingGlobalUpgrades = new HashSet<BuildingGlobalUpgrade>();

	public int Level { get; set; }

	public string Name => "BuildingUpgradeLevel";

	public int UpgradeLevel { get; set; }

	public BuildingUpgradeLevel(SerializedBuildingUpgradeLevel container)
	{
		Deserialize(container);
	}

	public BuildingUpgradeLevel()
	{
		Level = 1;
		UpgradeLevel = -1;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedBuildingUpgradeLevel serializedBuildingUpgradeLevel)
		{
			Level = serializedBuildingUpgradeLevel.Level;
			UpgradeLevel = serializedBuildingUpgradeLevel.UpgradeLevel;
		}
	}

	public ISerializedData Serialize()
	{
		return new SerializedBuildingUpgradeLevel
		{
			Level = Level,
			UpgradeLevel = UpgradeLevel
		};
	}
}
