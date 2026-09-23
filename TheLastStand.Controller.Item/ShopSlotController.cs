using TheLastStand.Definition.Item;
using TheLastStand.Model.Item;
using TheLastStand.Serialization.Item;
using TheLastStand.View.Shop;

namespace TheLastStand.Controller.Item;

/// <summary>
/// Controller quản lý một ô trưng bày trong Cửa hàng (Shop).
/// Kế thừa từ ItemSlotController. Mỗi ShopSlot hiển thị 1 vật phẩm có thể mua.
/// Khác với ShopInventorySlot (kho đã mua), ShopSlot là nơi TRƯNG BÀY vật phẩm bán.
/// Khi người chơi mua, vật phẩm chuyển từ ShopSlot → ShopInventory → Inventory.
/// </summary>
public class ShopSlotController : ItemSlotController
{
	#region Properties

	/// <summary>
	/// Cast ItemSlot thành ShopSlot để truy cập thuộc tính đặc thù
	/// (như giá bán, trạng thái đã bán...).
	/// </summary>
	public ShopSlot ShopSlot => base.ItemSlot as ShopSlot;

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor khởi tạo ShopSlot từ dữ liệu save (deserialization).
	/// Phục hồi trạng thái cửa hàng khi load game.
	/// </summary>
	/// <param name="container">Dữ liệu serialized của ô shop.</param>
	/// <param name="shopSlotView">View component hiển thị ô trong UI.</param>
	public ShopSlotController(SerializedItemShopSlot container, ShopSlotView shopSlotView)
		: base(container)
	{
		base.ItemSlot = new ShopSlot(container, this, shopSlotView);
	}

	/// <summary>
	/// Constructor khởi tạo ShopSlot mới (khi tạo giao diện cửa hàng mới).
	/// </summary>
	/// <param name="itemSlotDefinition">Định nghĩa ô slot.</param>
	/// <param name="shopSlotView">View component hiển thị ô trong UI.</param>
	public ShopSlotController(ItemSlotDefinition itemSlotDefinition, ShopSlotView shopSlotView)
		: base(itemSlotDefinition, shopSlotView)
	{
		base.ItemSlot = new ShopSlot(itemSlotDefinition, this, shopSlotView);
	}

	#endregion Constructors
}

