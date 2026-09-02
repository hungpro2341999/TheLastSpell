using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.View.Item;

namespace TheLastStand.Controller.Item;

public abstract class ItemSlotController
{
	public ItemSlot ItemSlot { get; protected set; }

	public ItemSlotController(ISerializedData container)
	{
	}

	public ItemSlotController(ItemSlotDefinition itemSlotDefinition, ItemSlotView itemSlotView)
	{
	}

	public bool IsItemCompatible(TheLastStand.Model.Item.Item item)
	{
		if (ItemSlot.ItemSlotDefinition.Categories.HasFlag(item.ItemDefinition.Category))
		{
			return ItemSlot.ItemSlotDefinition.Hands.Contains(item.ItemDefinition.Hands);
		}
		return false;
	}

	public void RemoveItem()
	{
		SetItem(null);
	}

	public virtual void SetItem(TheLastStand.Model.Item.Item item, bool onLoad = false)
	{
		if (ItemSlot.Item == item)
		{
			return;
		}
		if (ItemSlot.Item != null)
		{
			ItemSlot.Item.ItemSlot = null;
			ItemSlot.Item.ItemController.PerkClearAllReplacementSkills();
		}
		ItemSlot.Item = item;
		if (item != null)
		{
			if (item.ItemSlot != null)
			{
				item.ItemSlot.ItemSlotController.RemoveItem();
			}
			item.ItemSlot = ItemSlot;
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
			{
				item.ItemController.RefillOverallUses();
			}
		}
		ItemSlot.ItemSlotView.Refresh();
	}

	public void SwapItems(ItemSlot otherItemSlot = null, bool onLoad = false)
	{
		if (otherItemSlot == null)
		{
			otherItemSlot = TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.GetFirstAvailableSlot();
		}
		if (otherItemSlot == null)
		{
			TPSingleton<ItemManager>.Instance.LogWarning("No inventory slot found to swap items (inventory may be full), aborting.");
			return;
		}
		TheLastStand.Model.Item.Item item = otherItemSlot.Item;
		if (item != null && item == ItemSlot.Item)
		{
			otherItemSlot.ItemSlotController.SetItem(ItemSlot.Item, onLoad);
			return;
		}
		otherItemSlot.ItemSlotController.SetItem(ItemSlot.Item, onLoad);
		SetItem(item, onLoad);
	}
}
