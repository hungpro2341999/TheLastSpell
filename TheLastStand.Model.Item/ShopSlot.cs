using TheLastStand.Controller.Item;
using TheLastStand.Definition.Item;
using TheLastStand.Serialization.Item;
using TheLastStand.View.Shop;

namespace TheLastStand.Model.Item;

public class ShopSlot : ItemSlot
{
	public bool IsSoldOut { get; set; }

	public ShopSlotView ShopSlotView => base.ItemSlotView as ShopSlotView;

	public ShopSlot(SerializedItemShopSlot container, ShopSlotController shopSlotController, ShopSlotView shopSlotView)
	{
		base.ItemSlotController = shopSlotController;
		base.ItemSlotView = shopSlotView;
		Deserialize(container);
	}

	public ShopSlot(ItemSlotDefinition itemSlotDefinition, ShopSlotController shopSlotController, ShopSlotView shopSlotView)
		: base(itemSlotDefinition, shopSlotController, shopSlotView)
	{
	}

	public override void Deserialize(ISerializedData container, int saveVersion = -1)
	{
		SerializedItemShopSlot serializedItemShopSlot = container as SerializedItemShopSlot;
		base.Item = ((serializedItemShopSlot.Item == null) ? null : new ItemController(serializedItemShopSlot.Item, this).Item);
		IsSoldOut = serializedItemShopSlot.IsSoldOut;
	}

	public override ISerializedData Serialize()
	{
		return new SerializedItemShopSlot
		{
			Item = (base.Item?.Serialize() as SerializedItem),
			IsSoldOut = IsSoldOut
		};
	}
}
