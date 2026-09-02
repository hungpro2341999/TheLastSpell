using TheLastStand.Controller.Item;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization.Item;
using TheLastStand.View.Item;

namespace TheLastStand.Model.Item;

public abstract class ItemSlot : ISerializable, IDeserializable
{
	public Item Item { get; set; }

	public ItemSlotController ItemSlotController { get; protected set; }

	public ItemSlotDefinition ItemSlotDefinition { get; protected set; }

	public ItemSlotView ItemSlotView { get; set; }

	internal ItemSlot()
	{
	}

	public ItemSlot(SerializedItemSlot container, ItemSlotController itemSlotController, ItemSlotView itemSlotView)
	{
		ItemSlotController = itemSlotController;
		ItemSlotView = itemSlotView;
		Deserialize(container);
	}

	public ItemSlot(ItemSlotDefinition itemSlotDefinition, ItemSlotController itemSlotController, ItemSlotView itemSlotView)
	{
		ItemSlotDefinition = itemSlotDefinition;
		ItemSlotController = itemSlotController;
		ItemSlotView = itemSlotView;
	}

	public virtual void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedItemSlot serializedItemSlot = container as SerializedItemSlot;
		ItemSlotDefinition = ItemDatabase.ItemSlotDefinitions[serializedItemSlot.Id];
		if (serializedItemSlot.Item != null && ItemDatabase.ItemDefinitions.ContainsKey(serializedItemSlot.Item.Id))
		{
			Item = new ItemController(serializedItemSlot.Item, this).Item;
		}
	}

	public virtual ISerializedData Serialize()
	{
		return new SerializedItemSlot
		{
			Id = ItemSlotDefinition.Id,
			Item = (Item?.Serialize() as SerializedItem)
		};
	}
}
