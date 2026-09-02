using TheLastStand.Definition.Item;
using TheLastStand.Model.Item;
using TheLastStand.View.Shop;

namespace TheLastStand.Controller.Item;

public class ShopInventorySlotController : ItemSlotController
{
	public ShopInventorySlot ShopInventorySlot => base.ItemSlot as ShopInventorySlot;

	public ShopInventorySlotController(ItemSlotDefinition itemSlotDefinition, ShopInventorySlotView shopSlotView)
		: base(itemSlotDefinition, shopSlotView)
	{
		base.ItemSlot = new ShopInventorySlot(itemSlotDefinition, this, shopSlotView);
	}
}
