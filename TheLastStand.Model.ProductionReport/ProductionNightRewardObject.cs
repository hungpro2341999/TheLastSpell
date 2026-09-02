using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Definition.Building;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Item;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Item;

namespace TheLastStand.Model.ProductionReport;

public class ProductionNightRewardObject : ProductionObject, ISerializable, IDeserializable
{
	public const string SerializationElementName = "ProductionNightRewardObject";

	public const string SerializedId = "NightRewardItems";

	public List<TheLastStand.Model.Item.Item> Items = new List<TheLastStand.Model.Item.Item>();

	public ProductionNightRewardObject(ISerializedData container, ProductionNightRewardObjectController productionObjectController)
		: base(productionObjectController)
	{
		Deserialize(container);
	}

	public ProductionNightRewardObject(ProductionNightRewardObjectController productionObjectController, TheLastStand.Model.Item.Item[] items, BuildingDefinition productionBuilding)
		: base(productionObjectController, productionBuilding)
	{
		Items = items.ToList();
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedProductionItems serializedProductionItems = container as SerializedProductionItems;
		if (string.IsNullOrEmpty(serializedProductionItems.Id))
		{
			TPSingleton<ItemManager>.Instance.LogWarning("SerializedProductionItems ID is null or empty: could not deserialize production item. This should be a save version compatibility issue.", CLogLevel.DETAILED);
			return;
		}
		foreach (SerializedItem item2 in serializedProductionItems.Items)
		{
			TheLastStand.Model.Item.Item item = new ItemController(item2, null).Item;
			Items.Add(item);
		}
	}

	public ISerializedData Serialize()
	{
		SerializedProductionItems serializedProductionItems = new SerializedProductionItems
		{
			Id = "NightRewardItems"
		};
		for (int i = 0; i < Items.Count; i++)
		{
			serializedProductionItems.Items.Add(Items[i].Serialize() as SerializedItem);
		}
		return serializedProductionItems;
	}
}
