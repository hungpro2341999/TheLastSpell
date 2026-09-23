using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.View.Item;

namespace TheLastStand.Controller.Item;

/// <summary>
/// Lớp cơ sở trừu tượng (Abstract Base Class) cho tất cả các controller quản lý ô chứa vật phẩm (ItemSlot).
/// Cung cấp logic chung cho việc đặt/gỡ/hoán đổi vật phẩm giữa các ô.
/// Các lớp con cụ thể: EquipmentSlotController, InventorySlotController, 
/// ShopSlotController, ShopInventorySlotController.
/// 
/// Luồng vật phẩm: Shop → ShopInventory → Inventory → EquipmentSlot (trang bị).
/// </summary>
public abstract class ItemSlotController
{
	#region Properties

	/// <summary>
	/// Model ItemSlot mà controller này quản lý.
	/// Chứa: Item hiện tại, ItemSlotDefinition, reference tới View.
	/// </summary>
	public ItemSlot ItemSlot { get; protected set; }

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor từ dữ liệu save (deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu serialized.</param>
	public ItemSlotController(ISerializedData container)
	{
	}

	/// <summary>
	/// Constructor tạo ô mới với định nghĩa và view.
	/// </summary>
	/// <param name="itemSlotDefinition">Định nghĩa ô slot (loại ô, category cho phép).</param>
	/// <param name="itemSlotView">View component hiển thị ô trong UI.</param>
	public ItemSlotController(ItemSlotDefinition itemSlotDefinition, ItemSlotView itemSlotView)
	{
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Kiểm tra xem vật phẩm có tương thích với ô slot này không.
	/// Kiểm tra 2 điều kiện:
	/// 1. Category của vật phẩm phải nằm trong Categories cho phép của slot.
	/// 2. Kiểu cầm (Hands) của vật phẩm phải được slot hỗ trợ.
	/// </summary>
	/// <param name="item">Vật phẩm cần kiểm tra.</param>
	/// <returns>True nếu vật phẩm tương thích với ô slot.</returns>
	public bool IsItemCompatible(TheLastStand.Model.Item.Item item)
	{
		if (ItemSlot.ItemSlotDefinition.Categories.HasFlag(item.ItemDefinition.Category))
		{
			return ItemSlot.ItemSlotDefinition.Hands.Contains(item.ItemDefinition.Hands);
		}
		return false;
	}

	/// <summary>
	/// Gỡ vật phẩm khỏi ô slot này (đặt Item = null).
	/// </summary>
	public void RemoveItem()
	{
		SetItem(null);
	}

	/// <summary>
	/// Đặt vật phẩm vào ô slot này. Virtual method - EquipmentSlotController override
	/// để thêm logic trang bị/gỡ trang bị (cập nhật stats, perks, body parts...).
	/// 
	/// Logic xử lý:
	/// 1. Nếu item trùng với item hiện tại → bỏ qua.
	/// 2. Nếu đang có item cũ → hủy liên kết item cũ với slot, clear replacement skills.
	/// 3. Gán item mới vào slot.
	/// 4. Nếu item mới đang nằm ở slot khác → gỡ item khỏi slot cũ.
	/// 5. Nếu đang trong pha Day → nạp lại OverallUses cho item.
	/// 6. Refresh View.
	/// </summary>
	/// <param name="item">Vật phẩm cần đặt (null = gỡ vật phẩm).</param>
	/// <param name="onLoad">True nếu đang load save game (bỏ qua một số side effect).</param>
	public virtual void SetItem(TheLastStand.Model.Item.Item item, bool onLoad = false)
	{
		if (ItemSlot.Item == item)
		{
			return;
		}
		// Hủy liên kết item cũ nếu có
		if (ItemSlot.Item != null)
		{
			ItemSlot.Item.ItemSlot = null;
			ItemSlot.Item.ItemController.PerkClearAllReplacementSkills();
		}
		// Gán item mới
		ItemSlot.Item = item;
		if (item != null)
		{
			// Nếu item đang ở slot khác → gỡ khỏi slot cũ
			if (item.ItemSlot != null)
			{
				item.ItemSlot.ItemSlotController.RemoveItem();
			}
			item.ItemSlot = ItemSlot;
			// Nạp lại OverallUses khi di chuyển item trong pha Day
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
			{
				item.ItemController.RefillOverallUses();
			}
		}
		// Cập nhật hiển thị UI
		ItemSlot.ItemSlotView.Refresh();
	}

	/// <summary>
	/// Hoán đổi (swap) vật phẩm giữa ô này với một ô khác.
	/// Nếu không chỉ định ô đích, tự động tìm ô trống đầu tiên trong Inventory.
	/// </summary>
	/// <param name="otherItemSlot">Ô đích để hoán đổi. Null = tự tìm ô trống trong Inventory.</param>
	/// <param name="onLoad">True nếu đang load save game.</param>
	public void SwapItems(ItemSlot otherItemSlot = null, bool onLoad = false)
	{
		if (otherItemSlot == null)
		{
			otherItemSlot = TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.GetFirstAvailableSlot();
		}
		if (otherItemSlot == null)
		{
			TPSingleton<ItemManager>.Instance.LogWarning("No inventory slot found to swap items (inventory may be full), aborting.");
			return;
		}
		TheLastStand.Model.Item.Item item = otherItemSlot.Item;
		// Nếu cả 2 slot cùng chứa cùng 1 item → chỉ cần di chuyển
		if (item != null && item == ItemSlot.Item)
		{
			otherItemSlot.ItemSlotController.SetItem(ItemSlot.Item, onLoad);
			return;
		}
		// Hoán đổi: di chuyển item hiện tại sang ô đích, đặt item đích vào ô này
		otherItemSlot.ItemSlotController.SetItem(ItemSlot.Item, onLoad);
		SetItem(item, onLoad);
	}

	#endregion Public Methods
}
