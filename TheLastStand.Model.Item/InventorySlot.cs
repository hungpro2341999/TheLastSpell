using TheLastStand.Controller.Item;
using TheLastStand.Definition.Item;
using TheLastStand.View.CharacterSheet.Inventory;

namespace TheLastStand.Model.Item;

/// <summary>
/// Model đại diện cho một ô trong Inventory (túi đồ) của người chơi.
/// Kế thừa từ ItemSlot, thêm thuộc tính đặc thù cho ô Inventory:
/// - InventoryView: reference tới View của toàn bộ Inventory panel.
/// - IsNewItem: đánh dấu vật phẩm mới chưa xem (hiển thị indicator "New!").
/// </summary>
public class InventorySlot : ItemSlot
{
	#region Properties

	/// <summary>Reference tới View của toàn bộ Inventory panel.</summary>
	public InventoryView InventoryView { get; private set; }

	/// <summary>Cast ItemSlotView thành InventorySlotView.</summary>
	public InventorySlotView InventorySlotView => base.ItemSlotView as InventorySlotView;

	/// <summary>
	/// True nếu vật phẩm trong ô này là mới (vừa nhận, chưa xem).
	/// Dùng để hiển thị indicator "New!" trên UI.
	/// Được đặt false khi người chơi mở Inventory (MarkAllItemsAsSeen).
	/// </summary>
	public bool IsNewItem { get; set; }

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo InventorySlot mới.
	/// </summary>
	/// <param name="itemSlotDefinition">Định nghĩa ô slot (loại Inventory).</param>
	/// <param name="inventorySlotController">Controller quản lý ô này.</param>
	/// <param name="inventorySlotView">View component hiển thị ô.</param>
	/// <param name="inventoryView">View component của toàn bộ Inventory panel.</param>
	public InventorySlot(ItemSlotDefinition itemSlotDefinition, InventorySlotController inventorySlotController, InventorySlotView inventorySlotView, InventoryView inventoryView)
		: base(itemSlotDefinition, inventorySlotController, inventorySlotView)
	{
		InventoryView = inventoryView;
	}

	#endregion Constructors
}
