using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Building;

[Serializable]
public class SerializedBuildings : ISerializedData
{
	public List<SerializedBuilding> Buildings = new List<SerializedBuilding>();

	public SerializedShop Shop;

	public SerializedProductionReport ProductionReport;

	public SerializedBuildingUpgradeLevel BuildingUpgradeLevel;

	public SerializedConstruction SerializedConstruction;

	public List<SerializedBuildingToRestore> BuildingsToRestore = new List<SerializedBuildingToRestore>();

	public List<SerializedRandomBuilding> RandomBuildings = new List<SerializedRandomBuilding>();
}
