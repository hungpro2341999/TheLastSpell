using TheLastStand.Definition.Item;
using TheLastStand.Model.Item;
using TheLastStand.View.Shop;

namespace TheLastStand.Controller.Item;

/// <summary>
/// Controller quản lý một ô trong ShopInventory (kho đồ của cửa hàng).
/// Kế thừa từ ItemSlotController. ShopInventory chứa các vật phẩm đã mua
/// nhưng chưa được nhận vào Inventory chính của người chơi.
/// Đây là vùng đệm giữa Shop (cửa hàng) và Inventory (túi đồ).
/// </summary>
public class ShopInventorySlotController : ItemSlotController
{
	#region Properties

	/// <summary>
	/// Cast ItemSlot thành ShopInventorySlot để truy cập thuộc tính đặc thù.
	/// </summary>
	public ShopInventorySlot ShopInventorySlot => base.ItemSlot as ShopInventorySlot;

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo controller cho ô ShopInventory mới.
	/// </summary>
	/// <param name="itemSlotDefinition">Định nghĩa ô slot.</param>
	/// <param name="shopSlotView">View component hiển thị ô trong UI cửa hàng.</param>
	public ShopInventorySlotController(ItemSlotDefinition itemSlotDefinition, ShopInventorySlotView shopSlotView)
		: base(itemSlotDefinition, shopSlotView)
	{
		base.ItemSlot = new ShopInventorySlot(itemSlotDefinition, this, shopSlotView);
	}

	#endregion Constructors
}

