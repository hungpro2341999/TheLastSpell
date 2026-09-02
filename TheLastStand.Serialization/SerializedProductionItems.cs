using System;
using TheLastStand.Serialization.Item;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedProductionItems : ISerializedData
{
	public SerializedItems Items = new SerializedItems();

	public string Id;

	public bool IsNightProduction;

	public int ProductionLevel;

	public string BuildingActionId;

	public int CreateItemIndex;
}
