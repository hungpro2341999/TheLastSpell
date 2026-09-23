using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Controller.Item;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Item;
using TheLastStand.View;
using TheLastStand.View.CharacterSheet.Inventory;
using UnityEngine;

namespace TheLastStand.Manager.Item;

/// <summary>
/// Manager Singleton quản lý Inventory (túi đồ) chung của toàn bộ đội tướng.
/// Implement ISerializable + IDeserializable để hỗ trợ save/load game.
/// 
/// Chức năng chính:
/// - Sở hữu và khởi tạo Model Inventory (qua InventoryController).
/// - Cung cấp Inventory cho toàn bộ hệ thống qua TPSingleton pattern.
/// - Delegate StartTurn() cho InventoryController (nạp lại skill uses mỗi lượt).
/// - Chứa debug commands: xóa inventory, force truy cập.
/// 
/// Lưu ý: Inventory là CHUNG cho cả đội, không phải mỗi tướng 1 inventory riêng.
/// </summary>
public class InventoryManager : Manager<InventoryManager>, ISerializable, IDeserializable
{
	#region Fields

	/// <summary>Reference tới InventoryView (UI panel) - gán từ Unity Inspector.</summary>
	[SerializeField]
	private InventoryView inventoryView;

	/// <summary>Cờ debug cho phép mở Inventory bất kỳ lúc nào (bỏ qua kiểm tra trạng thái game).</summary>
	[SerializeField]
	private bool debugForceInventoryAccess;

	#endregion Fields

	#region Properties

	/// <summary>Truy cập static tới InventoryView component.</summary>
	public static InventoryView InventoryView => TPSingleton<InventoryManager>.Instance.inventoryView;

	/// <summary>
	/// Model Inventory - chứa danh sách InventorySlots, ItemCount.
	/// Được tạo trong Deserialize() hoặc khi bắt đầu game mới.
	/// </summary>
	public Inventory Inventory { get; private set; }

	/// <summary>Truy cập static tới cờ debug force inventory access.</summary>
	public static bool DebugForceInventoryAccess => TPSingleton<InventoryManager>.Instance.debugForceInventoryAccess;

	#endregion Properties

	#region Public Methods

	/// <summary>
	/// Xử lý logic đầu lượt cho Inventory.
	/// Delegate cho InventoryController.StartTurn() - nạp lại skill uses cho tất cả item.
	/// </summary>
	public static void StartTurn()
	{
		TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.StartTurn();
	}

	/// <summary>
	/// Deserialize Inventory từ save data hoặc tạo mới.
	/// - Nếu có container → khôi phục Inventory từ save (gọi InventoryController với container).
	/// - Nếu container null → tạo Inventory trống (game mới).
	/// </summary>
	/// <param name="container">Dữ liệu serialized (null = game mới).</param>
	/// <param name="saveVersion">Phiên bản save.</param>
	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container != null)
		{
			Inventory = new InventoryController(container, inventoryView).Inventory;
		}
		else
		{
			Inventory = new InventoryController(inventoryView).Inventory;
		}
	}

	/// <summary>
	/// Serialize Inventory thành dữ liệu lưu game.
	/// Delegate cho Inventory.Serialize().
	/// </summary>
	/// <returns>SerializedItems chứa danh sách item trong Inventory.</returns>
	public ISerializedData Serialize()
	{
		return Inventory.Serialize();
	}

	#endregion Public Methods

	#region Debug Commands

	/// <summary>
	/// [Debug Console] Bật/tắt force truy cập Inventory (bỏ qua kiểm tra trạng thái game).
	/// Lệnh: InventoryForceAccess [true/false]
	/// </summary>
	/// <param name="forceInventoryAccess">True = cho phép mở Inventory bất kỳ lúc nào.</param>
	[DevConsoleCommand(Name = "InventoryForceAccess")]
	public static void Debug_ForceInventoryAccess(bool forceInventoryAccess = true)
	{
		TPSingleton<InventoryManager>.Instance.debugForceInventoryAccess = forceInventoryAccess;
		GameView.BottomScreenPanel.Refresh();
	}

	/// <summary>
	/// [Debug Console] Xóa tất cả vật phẩm trong Inventory.
	/// Lệnh: InventoryClear
	/// </summary>
	[DevConsoleCommand(Name = "InventoryClear")]
	public static void Debug_ClearInventory()
	{
		foreach (InventorySlot inventorySlot in TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots)
		{
			inventorySlot.ItemSlotController.RemoveItem();
		}
	}

	#endregion Debug Commands
}
