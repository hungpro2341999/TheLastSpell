using TPLib;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Item;

namespace TheLastStand.Controller.Item;

/// <summary>
/// Ngữ cảnh phiên dịch công thức (Formula Interpreter Context) dành cho hệ thống vật phẩm.
/// Kế thừa từ FormulaInterpreterContext, cung cấp các biến động (dynamic variables)
/// cho Expression Interpreter khi tính toán công thức giá bán vật phẩm.
/// 
/// Các biến có thể dùng trong công thức giá:
/// - BasePrice: giá cơ sở theo level
/// - Rarity: độ hiếm (0-4)
/// - Constant1, Constant2, PowerConstant: hằng số cân bằng từ database
/// - [Category]Quantity: số lượng vật phẩm khả dụng theo từng loại trong shop
/// 
/// Ví dụ công thức: "BasePrice * (1 + Constant1 * Rarity^PowerConstant)"
/// </summary>
public class ItemInterpreterContext : FormulaInterpreterContext
{
	#region Fields

	/// <summary>
	/// Vật phẩm đang được tính giá. Null nếu context được dùng cho mục đích khác.
	/// </summary>
	private TheLastStand.Model.Item.Item item;

	#endregion Fields

	#region Properties - Thông số vật phẩm

	/// <summary>
	/// Giá trị đặc biệt biểu thị "tất cả" (-1). Dùng trong công thức khi cần tham chiếu toàn bộ.
	/// </summary>
	private int All => -1;

	/// <summary>
	/// Giá cơ sở (base price) của vật phẩm theo cấp độ hiện tại.
	/// Lấy từ ItemDefinition.BasePriceByLevel[level].
	/// </summary>
	private float BasePrice => item.ItemDefinition.BasePriceByLevel[item.Level];

	/// <summary>
	/// Độ hiếm của vật phẩm dưới dạng số nguyên (Common=0, Rare=1, Epic=2...).
	/// Dùng trong công thức để tính giá theo độ hiếm.
	/// </summary>
	private int Rarity => (int)item.Rarity;

	#endregion Properties - Thông số vật phẩm

	#region Properties - Hằng số cân bằng (Balancing Constants)

	/// <summary>
	/// Hằng số 1 trong phương trình giá vật phẩm. Đọc từ ItemDatabase.
	/// </summary>
	private float Constant1 => ItemDatabase.ItemPriceEquationConstant1;

	/// <summary>
	/// Hằng số 2 trong phương trình giá vật phẩm. Đọc từ ItemDatabase.
	/// </summary>
	private float Constant2 => ItemDatabase.ItemPriceEquationConstant2;

	/// <summary>
	/// Hằng số mũ (power) trong phương trình giá. Dùng cho lũy thừa theo Rarity.
	/// </summary>
	private float PowerConstant => ItemDatabase.ItemPriceEquationPowerConstant;

	#endregion Properties - Hằng số cân bằng (Balancing Constants)

	#region Properties - Số lượng vật phẩm trong Shop (Quantity Variables)

	/// <summary>Số lượng vũ khí cận chiến khả dụng trong cửa hàng.</summary>
	private int MeleeWeaponQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.MeleeWeapon);

	/// <summary>Số lượng vũ khí tầm xa khả dụng trong cửa hàng.</summary>
	private int RangedWeaponQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.RangeWeapon);

	/// <summary>Số lượng vũ khí phép thuật khả dụng trong cửa hàng.</summary>
	private int MagicWeaponQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.MagicWeapon);

	/// <summary>Tổng số vũ khí khả dụng (Melee + Ranged + Magic).</summary>
	private int WeaponQuantity => MeleeWeaponQuantity + RangedWeaponQuantity + MagicWeaponQuantity;

	/// <summary>Số lượng khiên khả dụng trong cửa hàng.</summary>
	private int ShieldQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Shield);

	/// <summary>Số lượng giáp thân khả dụng trong cửa hàng.</summary>
	private int BodyArmorQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.BodyArmor);

	/// <summary>Số lượng mũ/nón khả dụng trong cửa hàng.</summary>
	private int HelmQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Helm);

	/// <summary>Số lượng giày khả dụng trong cửa hàng.</summary>
	private int BootsQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Boots);

	/// <summary>Số lượng phụ kiện (trinket) khả dụng trong cửa hàng.</summary>
	private int TrinketQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Trinket);

	/// <summary>Số lượng vật phẩm tiện ích (utility) khả dụng trong cửa hàng.</summary>
	private int UtilityQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Utility);

	/// <summary>Số lượng thuốc (potion) khả dụng trong cửa hàng.</summary>
	private int PotionQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Potion);

	/// <summary>Số lượng cuộn giấy phép (scroll) khả dụng trong cửa hàng.</summary>
	private int ScrollQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Scroll);

	/// <summary>Tổng số vật phẩm sử dụng được (Potion + Scroll).</summary>
	private int UsableQuantity => PotionQuantity + ScrollQuantity;

	/// <summary>Tổng số giáp (BodyArmor + Helm + Boots).</summary>
	private int ArmorQuantity => BodyArmorQuantity + HelmQuantity + BootsQuantity;

	/// <summary>Tổng số trang bị (Shield + Armor + Trinket).</summary>
	private int EquipmentQuantity => ShieldQuantity + ArmorQuantity + TrinketQuantity;

	/// <summary>Tổng số vật phẩm tay phụ (Utility + Shield).</summary>
	private int OffHandQuantity => UtilityQuantity + ShieldQuantity;

	/// <summary>Tổng số tất cả vật phẩm khả dụng trong cửa hàng.</summary>
	private int AllQuantity => WeaponQuantity + EquipmentQuantity + UsableQuantity + OffHandQuantity;

	#endregion Properties - Số lượng vật phẩm trong Shop (Quantity Variables)

	#region Constructors

	/// <summary>
	/// Constructor mặc định - tạo context không gắn với vật phẩm cụ thể.
	/// Chỉ có các biến Quantity khả dụng (dùng cho công thức chung).
	/// </summary>
	public ItemInterpreterContext()
	{
	}

	/// <summary>
	/// Constructor tạo context gắn với vật phẩm cụ thể (dùng khi tính giá bán).
	/// Cho phép truy cập BasePrice, Rarity và các biến liên quan đến item.
	/// </summary>
	/// <param name="item">Vật phẩm đang được tính giá.</param>
	public ItemInterpreterContext(TheLastStand.Model.Item.Item item)
	{
		this.item = item;
	}

	#endregion Constructors
}
