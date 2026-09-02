using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Item;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Item;
using TheLastStand.Serialization.Item;
using TheLastStand.View.CharacterSheet.Inventory;

namespace TheLastStand.Model.Item;

public class Inventory : ISerializable, IDeserializable
{
	public InventoryController InventoryController { get; private set; }

	public List<InventorySlot> InventorySlots { get; set; } = new List<InventorySlot>();

	public InventoryView InventoryView { get; private set; }

	public int ItemCount => InventorySlots.FindAll((InventorySlot slot) => slot.Item != null).Count;

	public Inventory(ISerializedData container, InventoryController inventoryController, InventoryView inventoryView)
	{
		InventoryController = inventoryController;
		InventoryView = inventoryView;
	}

	public Inventory(InventoryController inventoryController, InventoryView inventoryView)
	{
		InventoryController = inventoryController;
		InventoryView = inventoryView;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		List<SerializedItem> list = container as List<SerializedItem>;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null)
			{
				try
				{
					InventorySlots[i].Item = new ItemController(list[i], InventorySlots[i]).Item;
					InventorySlots[i].IsNewItem = true;
				}
				catch (Database<TheLastStand.Database.ItemDatabase>.MissingAssetException arg)
				{
					TPSingleton<InventoryManager>.Instance.LogError($"{arg}\nTried to load non existing item definition, this item will be skipped.", CLogLevel.DETAILED);
				}
			}
		}
	}

	public ISerializedData Serialize()
	{
		SerializedItems serializedItems = new SerializedItems();
		for (int i = 0; i < InventorySlots.Count; i++)
		{
			if (InventorySlots[i].Item != null)
			{
				SerializedItem item = InventorySlots[i].Item.Serialize() as SerializedItem;
				serializedItems.Add(item);
			}
		}
		return serializedItems;
	}
}
