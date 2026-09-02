using TheLastStand.Definition.Item;
using TheLastStand.Model.Item;
using TheLastStand.View.CharacterSheet.Inventory;

namespace TheLastStand.Controller.Item;

public class InventorySlotController : ItemSlotController
{
	public InventorySlot InventorySlot => base.ItemSlot as InventorySlot;

	public InventorySlotController(ItemSlotDefinition itemSlotDefinition, InventorySlotView inventorySlotView, InventoryView inventoryView)
		: base(itemSlotDefinition, inventorySlotView)
	{
		base.ItemSlot = new InventorySlot(itemSlotDefinition, this, inventorySlotView, inventoryView);
	}
}
