using TheLastStand.Controller.Item;
using TheLastStand.Definition.Item;
using TheLastStand.View.Shop;

namespace TheLastStand.Model.Item;

public class ShopInventorySlot : ItemSlot
{
	public ShopInventorySlotView ShopInventorySlotView => base.ItemSlotView as ShopInventorySlotView;

	public ShopInventorySlot(ItemSlotDefinition itemSlotDefinition, ShopInventorySlotController shopSlotController, ShopInventorySlotView shopSlotView)
		: base(itemSlotDefinition, shopSlotController, shopSlotView)
	{
	}
}
