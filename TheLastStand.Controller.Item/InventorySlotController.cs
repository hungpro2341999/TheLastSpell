using TheLastStand.Definition.Item;
using TheLastStand.Model.Item;
using TheLastStand.View.CharacterSheet.Inventory;

namespace TheLastStand.Controller.Item;

/// <summary>
/// Controller quản lý một ô trong Inventory (túi đồ) của người chơi.
/// Kế thừa từ ItemSlotController. Mỗi ô Inventory có thể chứa 1 vật phẩm bất kỳ.
/// Inventory là nơi lưu trữ tạm thời vật phẩm chưa trang bị cho tướng.
/// </summary>
public class InventorySlotController : ItemSlotController
{
	#region Properties

	/// <summary>
	/// Cast ItemSlot thành InventorySlot để truy cập thuộc tính đặc thù của ô Inventory
	/// (như IsNewItem, InventoryView reference...).
	/// </summary>
	public InventorySlot InventorySlot => base.ItemSlot as InventorySlot;

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo controller cho ô Inventory mới (khi tạo giao diện Inventory).
	/// Tạo Model InventorySlot tương ứng và liên kết với View.
	/// </summary>
	/// <param name="itemSlotDefinition">Định nghĩa ô slot (loại Inventory).</param>
	/// <param name="inventorySlotView">View component hiển thị ô trong UI.</param>
	/// <param name="inventoryView">View component của toàn bộ Inventory panel.</param>
	public InventorySlotController(ItemSlotDefinition itemSlotDefinition, InventorySlotView inventorySlotView, InventoryView inventoryView)
		: base(itemSlotDefinition, inventorySlotView)
	{
		base.ItemSlot = new InventorySlot(itemSlotDefinition, this, inventorySlotView, inventoryView);
	}

	#endregion Constructors
}

