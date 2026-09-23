using TheLastStand.Controller.Item;
using TheLastStand.Definition.Item;
using TheLastStand.View.Shop;

namespace TheLastStand.Model.Item;

/// <summary>
/// Model đại diện cho một ô trong ShopInventory (kho đồ đã mua của cửa hàng).
/// Kế thừa từ ItemSlot. ShopInventory chứa vật phẩm đã mua nhưng chưa nhận vào Inventory chính.
/// Đây là vùng đệm giữa Shop (mua) và Inventory (sử dụng).
/// </summary>
public class ShopInventorySlot : ItemSlot
{
	#region Properties

	/// <summary>Cast ItemSlotView thành ShopInventorySlotView.</summary>
	public ShopInventorySlotView ShopInventorySlotView => base.ItemSlotView as ShopInventorySlotView;

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo ShopInventorySlot mới.
	/// </summary>
	/// <param name="itemSlotDefinition">Định nghĩa ô slot.</param>
	/// <param name="shopSlotController">Controller quản lý ô này.</param>
	/// <param name="shopSlotView">View component hiển thị ô trong UI cửa hàng.</param>
	public ShopInventorySlot(ItemSlotDefinition itemSlotDefinition, ShopInventorySlotController shopSlotController, ShopInventorySlotView shopSlotView)
		: base(itemSlotDefinition, shopSlotController, shopSlotView)
	{
	}

	#endregion Constructors
}
