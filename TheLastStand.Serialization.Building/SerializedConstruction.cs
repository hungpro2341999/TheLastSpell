using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Building;

[Serializable]
public class SerializedConstruction : ISerializedData
{
	public struct SerializedInstantProduction
	{
		public string BuildingId;

		public int ProductionBonus;
	}

	public List<SerializedInstantProduction> SerializedInstantProductionBonus = new List<SerializedInstantProduction>();
}
