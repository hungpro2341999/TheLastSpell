using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Item;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Item;
using TheLastStand.Serialization.Item;
using TheLastStand.View.CharacterSheet.Inventory;

namespace TheLastStand.Model.Item;

/// <summary>
/// Model đại diện cho toàn bộ Inventory (túi đồ/kho đồ) của người chơi.
/// Implement ISerializable + IDeserializable để hỗ trợ save/load game.
/// 
/// Inventory chứa danh sách InventorySlots (ô chứa) - mỗi ô có thể chứa 1 vật phẩm.
/// Số lượng ô được quyết định bởi số child trong ItemsPanelTransform của InventoryView (UI).
/// Inventory là CHUNG cho toàn đội (không phải mỗi tướng 1 inventory).
/// </summary>
public class Inventory : ISerializable, IDeserializable
{
	#region Properties

	/// <summary>Controller xử lý logic cho Inventory.</summary>
	public InventoryController InventoryController { get; private set; }

	/// <summary>
	/// Danh sách tất cả ô chứa (slots) trong Inventory.
	/// Mỗi slot có thể chứa hoặc không chứa 1 Item.
	/// </summary>
	public List<InventorySlot> InventorySlots { get; set; } = new List<InventorySlot>();

	/// <summary>View component (Unity UI) hiển thị Inventory panel.</summary>
	public InventoryView InventoryView { get; private set; }

	/// <summary>
	/// Số lượng vật phẩm hiện có trong Inventory (đếm slot có Item != null).
	/// Dùng để kiểm tra Inventory đầy hay chưa.
	/// </summary>
	public int ItemCount => InventorySlots.FindAll((InventorySlot slot) => slot.Item != null).Count;

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor khởi tạo Inventory từ dữ liệu save.
	/// Lưu ý: Deserialize() sẽ được gọi SAU khi GenerateInventorySlots() 
	/// trong InventoryController để đảm bảo có slot sẵn trước khi đặt item.
	/// </summary>
	/// <param name="container">Dữ liệu serialized.</param>
	/// <param name="inventoryController">Controller quản lý Inventory.</param>
	/// <param name="inventoryView">View component.</param>
	public Inventory(ISerializedData container, InventoryController inventoryController, InventoryView inventoryView)
	{
		InventoryController = inventoryController;
		InventoryView = inventoryView;
	}

	/// <summary>
	/// Constructor tạo Inventory mới (trống).
	/// </summary>
	/// <param name="inventoryController">Controller quản lý Inventory.</param>
	/// <param name="inventoryView">View component.</param>
	public Inventory(InventoryController inventoryController, InventoryView inventoryView)
	{
		InventoryController = inventoryController;
		InventoryView = inventoryView;
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Deserialize danh sách item từ save data vào các InventorySlot.
	/// Duyệt qua danh sách SerializedItem theo thứ tự index,
	/// tạo ItemController cho mỗi item và gán vào slot tương ứng.
	/// Item mới load từ save được đánh dấu IsNewItem = true.
	/// Bắt MissingAssetException nếu ItemDefinition không tồn tại (skip item lỗi).
	/// </summary>
	/// <param name="container">Danh sách SerializedItem.</param>
	/// <param name="saveVersion">Phiên bản save.</param>
	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		List<SerializedItem> list = container as List<SerializedItem>;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null)
			{
				try
				{
					InventorySlots[i].Item = new ItemController(list[i], InventorySlots[i]).Item;
					InventorySlots[i].IsNewItem = true;
				}
				catch (Database<TheLastStand.Database.ItemDatabase>.MissingAssetException arg)
				{
					TPSingleton<InventoryManager>.Instance.LogError($"{arg}\nTried to load non existing item definition, this item will be skipped.", CLogLevel.DETAILED);
				}
			}
		}
	}

	/// <summary>
	/// Serialize toàn bộ Inventory thành danh sách SerializedItem.
	/// Chỉ lưu các slot CÓ item (slot trống bị bỏ qua).
	/// </summary>
	/// <returns>SerializedItems chứa danh sách item cần lưu.</returns>
	public ISerializedData Serialize()
	{
		SerializedItems serializedItems = new SerializedItems();
		for (int i = 0; i < InventorySlots.Count; i++)
		{
			if (InventorySlots[i].Item != null)
			{
				SerializedItem item = InventorySlots[i].Item.Serialize() as SerializedItem;
				serializedItems.Add(item);
			}
		}
		return serializedItems;
	}

	#endregion Public Methods
}
