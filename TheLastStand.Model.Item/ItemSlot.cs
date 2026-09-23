using TheLastStand.Controller.Item;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization.Item;
using TheLastStand.View.Item;

namespace TheLastStand.Model.Item;

/// <summary>
/// Lớp cơ sở trừu tượng (Abstract Base Class) cho tất cả các loại ô chứa vật phẩm.
/// Implement ISerializable + IDeserializable để hỗ trợ save/load game.
/// 
/// Chứa thông tin chung:
/// - Item: vật phẩm hiện tại trong ô (null nếu ô trống).
/// - ItemSlotDefinition: định nghĩa ô (loại, category cho phép, hands cho phép).
/// - ItemSlotController: controller xử lý logic (SetItem, SwapItems...).
/// - ItemSlotView: view component hiển thị ô trong UI.
/// 
/// Các lớp con: EquipmentSlot, InventorySlot, ShopSlot, ShopInventorySlot.
/// </summary>
public abstract class ItemSlot : ISerializable, IDeserializable
{
	#region Properties

	/// <summary>
	/// Vật phẩm hiện tại trong ô slot. Null nếu ô trống.
	/// </summary>
	public Item Item { get; set; }

	/// <summary>
	/// Controller xử lý logic cho ô slot (SetItem, SwapItems, RemoveItem...).
	/// </summary>
	public ItemSlotController ItemSlotController { get; protected set; }

	/// <summary>
	/// Định nghĩa ô slot: xác định loại slot (Inventory, RightHand, Head...),
	/// danh sách category và hands type được phép chứa.
	/// </summary>
	public ItemSlotDefinition ItemSlotDefinition { get; protected set; }

	/// <summary>
	/// View component (Unity UI) hiển thị ô slot trên giao diện.
	/// </summary>
	public ItemSlotView ItemSlotView { get; set; }

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor nội bộ mặc định (dùng bởi lớp con khi cần khởi tạo đặc biệt).
	/// </summary>
	internal ItemSlot()
	{
	}

	/// <summary>
	/// Constructor khởi tạo từ dữ liệu save (deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu serialized của ô slot.</param>
	/// <param name="itemSlotController">Controller quản lý ô.</param>
	/// <param name="itemSlotView">View component hiển thị ô.</param>
	public ItemSlot(SerializedItemSlot container, ItemSlotController itemSlotController, ItemSlotView itemSlotView)
	{
		ItemSlotController = itemSlotController;
		ItemSlotView = itemSlotView;
		Deserialize(container);
	}

	/// <summary>
	/// Constructor tạo ô mới (chưa có item).
	/// </summary>
	/// <param name="itemSlotDefinition">Định nghĩa ô slot.</param>
	/// <param name="itemSlotController">Controller quản lý ô.</param>
	/// <param name="itemSlotView">View component hiển thị ô.</param>
	public ItemSlot(ItemSlotDefinition itemSlotDefinition, ItemSlotController itemSlotController, ItemSlotView itemSlotView)
	{
		ItemSlotDefinition = itemSlotDefinition;
		ItemSlotController = itemSlotController;
		ItemSlotView = itemSlotView;
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Deserialize ô slot từ save data.
	/// 1. Tra cứu ItemSlotDefinition từ ItemDatabase theo Id.
	/// 2. Nếu save data có item → tạo ItemController + Model Item tương ứng.
	/// </summary>
	/// <param name="container">Dữ liệu serialized.</param>
	/// <param name="saveVersion">Phiên bản save (để tương thích ngược).</param>
	public virtual void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedItemSlot serializedItemSlot = container as SerializedItemSlot;
		ItemSlotDefinition = ItemDatabase.ItemSlotDefinitions[serializedItemSlot.Id];
		if (serializedItemSlot.Item != null && ItemDatabase.ItemDefinitions.ContainsKey(serializedItemSlot.Item.Id))
		{
			Item = new ItemController(serializedItemSlot.Item, this).Item;
		}
	}

	/// <summary>
	/// Serialize ô slot thành dữ liệu lưu game.
	/// Lưu: Id slot, Item serialized (nếu có).
	/// </summary>
	/// <returns>SerializedItemSlot chứa dữ liệu cần lưu.</returns>
	public virtual ISerializedData Serialize()
	{
		return new SerializedItemSlot
		{
			Id = ItemSlotDefinition.Id,
			Item = (Item?.Serialize() as SerializedItem)
		};
	}

	#endregion Public Methods
}
