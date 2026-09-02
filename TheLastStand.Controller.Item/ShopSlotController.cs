using TheLastStand.Definition.Item;
using TheLastStand.Model.Item;
using TheLastStand.Serialization.Item;
using TheLastStand.View.Shop;

namespace TheLastStand.Controller.Item;

public class ShopSlotController : ItemSlotController
{
	public ShopSlot ShopSlot => base.ItemSlot as ShopSlot;

	public ShopSlotController(SerializedItemShopSlot container, ShopSlotView shopSlotView)
		: base(container)
	{
		base.ItemSlot = new ShopSlot(container, this, shopSlotView);
	}

	public ShopSlotController(ItemSlotDefinition itemSlotDefinition, ShopSlotView shopSlotView)
		: base(itemSlotDefinition, shopSlotView)
	{
		base.ItemSlot = new ShopSlot(itemSlotDefinition, this, shopSlotView);
	}
}
