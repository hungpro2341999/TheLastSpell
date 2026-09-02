using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedProductionReport : ISerializedData
{
	public List<SerializedProducedResource> ProductionGolds;

	public List<SerializedProductionItems> ProductionItems = new List<SerializedProductionItems>();

	public List<SerializedProducedResource> ProductionMaterials;

	public int BaseRewardReroll;

	public int RemainingRewardReroll;
}
