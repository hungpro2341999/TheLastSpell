using TheLastStand.Controller.Item;
using TheLastStand.Definition.Item;
using TheLastStand.View.CharacterSheet.Inventory;

namespace TheLastStand.Model.Item;

public class InventorySlot : ItemSlot
{
	public InventoryView InventoryView { get; private set; }

	public InventorySlotView InventorySlotView => base.ItemSlotView as InventorySlotView;

	public bool IsNewItem { get; set; }

	public InventorySlot(ItemSlotDefinition itemSlotDefinition, InventorySlotController inventorySlotController, InventorySlotView inventorySlotView, InventoryView inventoryView)
		: base(itemSlotDefinition, inventorySlotController, inventorySlotView)
	{
		InventoryView = inventoryView;
	}
}
