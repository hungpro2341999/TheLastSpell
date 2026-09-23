using TheLastStand.Controller.Item;
using TheLastStand.Definition.Item;
using TheLastStand.Serialization.Item;
using TheLastStand.View.Shop;

namespace TheLastStand.Model.Item;

/// <summary>
/// Model đại diện cho một ô trưng bày vật phẩm trong Cửa hàng (Shop).
/// Kế thừa từ ItemSlot, thêm thuộc tính đặc thù:
/// - IsSoldOut: đánh dấu ô đã bán hết (người chơi đã mua vật phẩm này).
/// 
/// Override Deserialize/Serialize để xử lý SerializedItemShopSlot
/// (chứa thêm IsSoldOut so với SerializedItemSlot thường).
/// </summary>
public class ShopSlot : ItemSlot
{
	#region Properties

	/// <summary>
	/// True nếu vật phẩm trong ô đã được bán (người chơi đã mua).
	/// Khi IsSoldOut = true, ô hiển thị "SOLD OUT" và không thể mua lại.
	/// </summary>
	public bool IsSoldOut { get; set; }

	/// <summary>Cast ItemSlotView thành ShopSlotView.</summary>
	public ShopSlotView ShopSlotView => base.ItemSlotView as ShopSlotView;

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor khởi tạo ShopSlot từ dữ liệu save (deserialization).
	/// Dùng internal constructor của base (ItemSlot()) để tránh gọi base Deserialize,
	/// sau đó tự gọi Deserialize riêng để xử lý SerializedItemShopSlot.
	/// </summary>
	/// <param name="container">Dữ liệu serialized của ô shop.</param>
	/// <param name="shopSlotController">Controller quản lý ô.</param>
	/// <param name="shopSlotView">View component hiển thị ô.</param>
	public ShopSlot(SerializedItemShopSlot container, ShopSlotController shopSlotController, ShopSlotView shopSlotView)
	{
		base.ItemSlotController = shopSlotController;
		base.ItemSlotView = shopSlotView;
		Deserialize(container);
	}

	/// <summary>
	/// Constructor tạo ShopSlot mới.
	/// </summary>
	/// <param name="itemSlotDefinition">Định nghĩa ô slot.</param>
	/// <param name="shopSlotController">Controller quản lý ô.</param>
	/// <param name="shopSlotView">View component hiển thị ô.</param>
	public ShopSlot(ItemSlotDefinition itemSlotDefinition, ShopSlotController shopSlotController, ShopSlotView shopSlotView)
		: base(itemSlotDefinition, shopSlotController, shopSlotView)
	{
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Override Deserialize - đọc dữ liệu riêng của ShopSlot từ SerializedItemShopSlot.
	/// Không gọi base.Deserialize vì ShopSlot có format serialization khác.
	/// Đọc: Item (nếu có) và IsSoldOut.
	/// </summary>
	/// <param name="container">Dữ liệu serialized.</param>
	/// <param name="saveVersion">Phiên bản save.</param>
	public override void Deserialize(ISerializedData container, int saveVersion = -1)
	{
		SerializedItemShopSlot serializedItemShopSlot = container as SerializedItemShopSlot;
		// Tạo item từ save data (null nếu ô trống)
		base.Item = ((serializedItemShopSlot.Item == null) ? null : new ItemController(serializedItemShopSlot.Item, this).Item);
		IsSoldOut = serializedItemShopSlot.IsSoldOut;
	}

	/// <summary>
	/// Override Serialize - lưu dữ liệu riêng của ShopSlot.
	/// Lưu: Item serialized, IsSoldOut.
	/// </summary>
	/// <returns>SerializedItemShopSlot chứa dữ liệu cần lưu.</returns>
	public override ISerializedData Serialize()
	{
		return new SerializedItemShopSlot
		{
			Item = (base.Item?.Serialize() as SerializedItem),
			IsSoldOut = IsSoldOut
		};
	}

	#endregion Public Methods
}
