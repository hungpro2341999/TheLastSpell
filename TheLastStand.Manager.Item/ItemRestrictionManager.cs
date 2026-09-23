using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.Item.ItemRestriction;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Item.ItemRestriction;
using TheLastStand.Serialization.Item.ItemRestriction;
using UnityEngine;

namespace TheLastStand.Manager.Item;

/// <summary>
/// Manager Singleton quản lý hệ thống giới hạn vật phẩm (Item Restrictions).
/// Cho phép người chơi bật/tắt các "họ vũ khí" (weapon families) trước khi bắt đầu run,
/// để tùy chỉnh pool vật phẩm xuất hiện trong game.
/// 
/// Hệ thống phân cấp:
/// - ItemRestrictionCategoriesCollection ("WeaponsRestrictions"): nhóm các category lớn.
/// - ItemRestrictionFamily: một họ vật phẩm (ví dụ: "Swords", "Bows", "Shields").
///   Mỗi family link tới 1 ItemsListDefinition chứa danh sách item cụ thể.
/// 
/// Chức năng:
/// - Quản lý danh sách families (ItemRestrictionFamilies).
/// - Tra cứu nhanh theo ItemsListId hoặc ItemCategory.
/// - Cung cấp GetLockedItemsIds() cho ItemManager.GetAllLockedItemsIds().
/// - Serialize/Deserialize trạng thái bật/tắt của từng family.
/// - Debug commands để thao tác trực tiếp.
/// 
/// Implement ISerializable + IDeserializable (lưu trạng thái restriction qua các runs).
/// </summary>
public sealed class ItemRestrictionManager : Manager<ItemRestrictionManager>, ISerializable, IDeserializable
{
	#region Nested Types

	/// <summary>Các hằng số cho hệ thống Item Restriction.</summary>
	public static class Constants
	{
		/// <summary>ID của collection "WeaponsRestrictions" - nhóm giới hạn vũ khí chính.</summary>
		public const string WeaponsCategoriesCollectionId = "WeaponsRestrictions";
	}

	#endregion Nested Types

	#region Fields

	/// <summary>Cờ đánh dấu đã khởi tạo xong (tránh xử lý quá sớm).</summary>
	private bool initialized;

	#endregion Fields

	#region Properties

	/// <summary>
	/// Danh sách TẤT CẢ ItemRestrictionFamily (họ vật phẩm).
	/// Mỗi family có IsSelected (người chơi bật/tắt) và IsActive (tính cả HasUnlockedItems).
	/// </summary>
	public List<ItemRestrictionFamily> ItemRestrictionFamilies { get; private set; }

	/// <summary>
	/// Dictionary tra cứu nhanh family theo ItemsListId.
	/// Key = ItemsListDefinition.Id, Value = ItemRestrictionFamily.
	/// </summary>
	public Dictionary<string, ItemRestrictionFamily> ItemRestrictionFamiliesByItemsListId { get; private set; }

	/// <summary>
	/// Dictionary tra cứu nhanh families theo ItemCategory.
	/// Key = E_Category (Sword, Bow, Shield...), Value = danh sách families.
	/// </summary>
	public Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamily>> ItemRestrictionFamiliesByItemCategory { get; private set; }

	/// <summary>
	/// Collection quản lý các category vũ khí (WeaponsRestrictions).
	/// Cho phép bật/tắt theo nhóm category lớn (MeleeWeapon, RangedWeapon...).
	/// </summary>
	public ItemRestrictionCategoriesCollection WeaponsRestrictionsCategories { get; private set; }

	#endregion Properties

	#region Public Methods

	/// <summary>
	/// Cập nhật trạng thái HasUnlockedItems cho tất cả families thuộc category chỉ định.
	/// Gọi khi MetaUpgrade mở khóa/khóa item mới → cần recompute.
	/// </summary>
	/// <param name="itemCategory">Category cần cập nhật.</param>
	/// <param name="isUnlocked">Trạng thái mới (đã mở khóa hay chưa).</param>
	public static void RefreshItemFamiliesLockedItemsFromCategory(ItemDefinition.E_Category itemCategory, bool isUnlocked)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null || !TPSingleton<ItemRestrictionManager>.Instance.initialized)
		{
			return;
		}
		foreach (ItemRestrictionFamily itemRestrictionFamily in TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamilies)
		{
			if (itemRestrictionFamily.ItemFamilyDefinition.ItemCategory.HasFlag(itemCategory) && itemRestrictionFamily.HasUnlockedItems != isUnlocked)
			{
				itemRestrictionFamily.ItemFamilyController.ComputeHasUnlockedItems();
			}
		}
	}

	/// <summary>
	/// Lấy tất cả Item IDs bị khóa bởi hệ thống restriction.
	/// Family có IsActive = false → tất cả item trong family đều bị khóa.
	/// Kết quả được dùng bởi ItemManager.GetAllLockedItemsIds().
	/// </summary>
	/// <returns>HashSet chứa IDs item bị khóa.</returns>
	public HashSet<string> GetLockedItemsIds()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (ItemRestrictionFamily itemRestrictionFamily in TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamilies)
		{
			if (!itemRestrictionFamily.IsActive)
			{
				hashSet.UnionWith(itemRestrictionFamily.ItemsIds);
			}
		}
		return hashSet;
	}

	/// <summary>
	/// Thay đổi trạng thái bật/tắt (IsSelected) của family theo ID.
	/// </summary>
	/// <param name="isSelected">True = bật family (item xuất hiện), False = tắt (item bị khóa).</param>
	/// <param name="itemFamilyId">ItemsListId của family cần thay đổi.</param>
	/// <returns>True nếu tìm thấy và thay đổi thành công.</returns>
	public bool TryChangeItemFamilySelected(bool isSelected, string itemFamilyId)
	{
		if (ItemRestrictionFamiliesByItemsListId.TryGetValue(itemFamilyId, out var value))
		{
			value.ItemFamilyController.SetSelected(isSelected);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Bật TẤT CẢ families thuộc category chỉ định.
	/// </summary>
	/// <param name="itemCategory">Category cần bật.</param>
	public void SelectAllItemFamiliesFromCategory(ItemDefinition.E_Category itemCategory)
	{
		if (!ItemRestrictionFamiliesByItemCategory.TryGetValue(itemCategory, out var value))
		{
			return;
		}
		foreach (ItemRestrictionFamily item in value)
		{
			item.ItemFamilyController.SetSelected(isSelected: true);
		}
	}

	/// <summary>
	/// Lấy danh sách family vũ khí đang active (cho analytics tracking).
	/// Format: "Category_ShortId" (ví dụ: "Weapon_Swords").
	/// </summary>
	/// <returns>Mảng tên family active.</returns>
	public string[] GetActiveWeaponFamiliesForAnalytics()
	{
		List<string> list = new List<string>();
		foreach (ItemRestrictionFamily itemRestrictionFamily in ItemRestrictionFamilies)
		{
			if (itemRestrictionFamily.IsActive && (ItemDefinition.E_Category.Weapon & itemRestrictionFamily.ItemFamilyDefinition.ItemCategory) != ItemDefinition.E_Category.None)
			{
				string item = itemRestrictionFamily.ItemFamilyDefinition.ItemCategory.ToString() + "_" + itemRestrictionFamily.ItemFamilyDefinition.ShortId;
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	/// <summary>
	/// Lấy danh sách family vũ khí bị loại bỏ bởi người chơi (cho analytics tracking).
	/// Chỉ tính family đã mở khóa nhưng người chơi chủ động tắt.
	/// </summary>
	/// <returns>Mảng tên family bị loại bỏ.</returns>
	public string[] GetExcludedWeaponFamiliesForAnalytics()
	{
		List<string> list = new List<string>();
		foreach (ItemRestrictionFamily itemRestrictionFamily in ItemRestrictionFamilies)
		{
			if (itemRestrictionFamily.HasUnlockedItems && !itemRestrictionFamily.IsSelected && (ItemDefinition.E_Category.Weapon & itemRestrictionFamily.ItemFamilyDefinition.ItemCategory) != ItemDefinition.E_Category.None)
			{
				string item = itemRestrictionFamily.ItemFamilyDefinition.ItemCategory.ToString() + "_" + itemRestrictionFamily.ItemFamilyDefinition.ShortId;
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	#endregion Public Methods

	#region Serialization / Deserialization

	/// <summary>
	/// Deserialize hệ thống ItemRestriction từ save data.
	/// 
	/// Luồng:
	/// 1. Build dictionary serialized families (nếu có save data).
	/// 2. Tạo ItemRestrictionFamily cho mỗi definition:
	///    - Có save → khôi phục IsSelected từ save.
	///    - Không có save → tạo mới (mặc định IsSelected = true).
	/// 3. Kiểm tra trùng lặp ItemsListId.
	/// 4. ComputeHasUnlockedItems cho mỗi family.
	/// 5. Build dictionaries tra cứu nhanh (ByItemsListId, ByItemCategory).
	/// 6. Tạo WeaponsRestrictionsCategories từ collection definition.
	/// </summary>
	/// <param name="container">Dữ liệu serialized (null = game mới).</param>
	/// <param name="saveVersion">Phiên bản save.</param>
	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		ItemRestrictionFamiliesByItemsListId = new Dictionary<string, ItemRestrictionFamily>();
		ItemRestrictionFamiliesByItemCategory = new Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamily>>();
		// Build dictionary serialized families
		Dictionary<string, SerializedItemRestrictionFamily> dictionary = new Dictionary<string, SerializedItemRestrictionFamily>();
		SerializedItemRestrictions serializedItemRestrictions = null;
		if (container is SerializedItemRestrictions serializedItemRestrictions2)
		{
			serializedItemRestrictions = serializedItemRestrictions2;
			if (serializedItemRestrictions2.ItemFamilies != null && serializedItemRestrictions2.ItemFamilies.Count > 0)
			{
				foreach (SerializedItemRestrictionFamily itemFamily in serializedItemRestrictions2.ItemFamilies)
				{
					dictionary.Add(itemFamily.Id, itemFamily);
				}
			}
		}
		// Tạo families từ definitions
		ItemRestrictionFamilies = new List<ItemRestrictionFamily>();
		foreach (ItemRestrictionFamilyDefinition itemRestrictionFamilyDefinition in ItemDatabase.ItemRestrictionFamiliesDefinitions.Values)
		{
			ItemRestrictionFamily itemRestrictionFamily = null;
			// Có save data → khôi phục, không có → tạo mới
			itemRestrictionFamily = ((!dictionary.TryGetValue(itemRestrictionFamilyDefinition.ItemsListId, out var value)) ? new ItemRestrictionFamilyController(itemRestrictionFamilyDefinition).ItemFamily : new ItemRestrictionFamilyController(value, itemRestrictionFamilyDefinition).ItemFamily);
			// Kiểm tra trùng lặp
			if (ItemRestrictionFamilies.Any((ItemRestrictionFamily itemFamily) => itemFamily.ItemFamilyDefinition.ItemsListId == itemRestrictionFamilyDefinition.ItemsListId))
			{
				LogError("Tried to add an ItemRestrictionFamily with an ItemListsIds already present !", CLogLevel.DETAILED);
				continue;
			}
			// Tính toán HasUnlockedItems và thêm vào danh sách
			itemRestrictionFamily.ItemFamilyController.ComputeHasUnlockedItems();
			ItemRestrictionFamilies.Add(itemRestrictionFamily);
			// Build dictionaries tra cứu nhanh
			if (!ItemRestrictionFamiliesByItemsListId.ContainsKey(itemRestrictionFamilyDefinition.ItemsListId))
			{
				ItemRestrictionFamiliesByItemsListId.Add(itemRestrictionFamilyDefinition.ItemsListId, itemRestrictionFamily);
			}
			if (!ItemRestrictionFamiliesByItemCategory.ContainsKey(itemRestrictionFamilyDefinition.ItemCategory))
			{
				ItemRestrictionFamiliesByItemCategory.Add(itemRestrictionFamilyDefinition.ItemCategory, new List<ItemRestrictionFamily> { itemRestrictionFamily });
			}
			else
			{
				ItemRestrictionFamiliesByItemCategory[itemRestrictionFamilyDefinition.ItemCategory].Add(itemRestrictionFamily);
			}
		}
		// Tạo WeaponsRestrictionsCategories
		foreach (ItemRestrictionCategoriesCollectionDefinition value2 in ItemDatabase.ItemRestrictionCategoriesCollectionDefinitions.Values)
		{
			string id = value2.Id;
			if (id != null && id == "WeaponsRestrictions")
			{
				if (serializedItemRestrictions != null && serializedItemRestrictions.WeaponsCategoriesCollection != null)
				{
					WeaponsRestrictionsCategories = new ItemRestrictionCategoriesCollectionController(serializedItemRestrictions.WeaponsCategoriesCollection, value2).ItemRestrictionCategoriesCollection;
				}
				else
				{
					WeaponsRestrictionsCategories = new ItemRestrictionCategoriesCollectionController(value2).ItemRestrictionCategoriesCollection;
				}
				WeaponsRestrictionsCategories.ItemCategoriesCollectionController.SelectAllItemsIfCollectionNotAvailable();
			}
		}
		initialized = true;
	}

	/// <summary>
	/// Serialize trạng thái restriction hiện tại thành save data.
	/// Lưu: danh sách families (với IsSelected) + WeaponsCategoriesCollection.
	/// </summary>
	/// <returns>SerializedItemRestrictions chứa dữ liệu cần lưu.</returns>
	public ISerializedData Serialize()
	{
		return new SerializedItemRestrictions
		{
			ItemFamilies = ItemRestrictionFamilies.Select((ItemRestrictionFamily family) => family.Serialize() as SerializedItemRestrictionFamily).ToList(),
			WeaponsCategoriesCollection = (WeaponsRestrictionsCategories.Serialize() as SerializedItemRestrictionCategoriesCollection)
		};
	}

	#endregion Serialization / Deserialization

	#region Debug Commands

	/// <summary>
	/// [Debug Console] Hiển thị thông tin chi tiết của family theo ShortId.
	/// Lệnh: ItemRestrictionDisplayFamily [shortId]
	/// </summary>
	[DevConsoleCommand("ItemRestrictionDisplayFamily")]
	public static void DebugItemRestrictionDisplayFamily([StringConverter(typeof(ItemRestrictionFamily.StringToItemRestrictionFamilyShortIdConverter))] string itemRestrictionFamilyShortId)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null)
		{
			TPDebug.LogError("No ItemRestrictionManager instantiated !");
			return;
		}
		ItemRestrictionFamily itemRestrictionFamily = TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamilies.Find((ItemRestrictionFamily anItemFamily) => anItemFamily.ItemFamilyDefinition.ShortId == itemRestrictionFamilyShortId);
		if (itemRestrictionFamily == null)
		{
			TPDebug.LogError("Couldn't find an ItemRestrictionFamily with short id: " + itemRestrictionFamilyShortId);
		}
		else
		{
			TPSingleton<DebugManager>.Instance.LogDevConsole(itemRestrictionFamily);
		}
	}

	/// <summary>
	/// [Debug Console] Bật/tắt family theo ShortId.
	/// Lệnh: ItemRestrictionSetFamilySelected [shortId] [selected]
	/// </summary>
	[DevConsoleCommand("ItemRestrictionSetFamilySelected")]
	public static void DebugItemRestrictionSetFamilySelected([StringConverter(typeof(ItemRestrictionFamily.StringToItemRestrictionFamilyShortIdConverter))] string itemRestrictionFamilyShortId, bool selected = true)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null)
		{
			TPDebug.LogError("No ItemRestrictionManager instantiated !");
			return;
		}
		ItemRestrictionFamily itemRestrictionFamily = TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamilies.Find((ItemRestrictionFamily anItemFamily) => anItemFamily.ItemFamilyDefinition.ShortId == itemRestrictionFamilyShortId);
		if (itemRestrictionFamily == null)
		{
			TPDebug.LogError("Couldn't find an ItemRestrictionFamily with short id: " + itemRestrictionFamilyShortId);
		}
		else
		{
			itemRestrictionFamily.ItemFamilyController.SetSelected(selected);
		}
	}

	/// <summary>
	/// [Debug Console] Bật/tắt TẤT CẢ families.
	/// Lệnh: ItemRestrictionSetAllFamiliesSelected [selected]
	/// </summary>
	[DevConsoleCommand("ItemRestrictionSetAllFamiliesSelected")]
	public static void DebugItemRestrictionSetAllFamiliesSelected(bool selected)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null)
		{
			TPDebug.LogError("No ItemRestrictionManager instantiated !");
			return;
		}
		foreach (ItemRestrictionFamily itemRestrictionFamily in TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamilies)
		{
			itemRestrictionFamily.ItemFamilyController.SetSelected(selected);
		}
	}

	/// <summary>
	/// [Debug Console] Hiển thị tất cả families active/inactive.
	/// Lệnh: ItemRestrictionGetAllActiveFamilies [areActive] [onlyUnlocked]
	/// </summary>
	[DevConsoleCommand("ItemRestrictionGetAllActiveFamilies")]
	public static void DebugItemRestrictionGetAllActiveFamilies(bool areActive = true, bool getOnlyUnlockedFamilies = false)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null)
		{
			TPDebug.LogError("No ItemRestrictionManager instantiated !");
			return;
		}
		Dictionary<ItemDefinition.E_Category, List<string>> dictionary = new Dictionary<ItemDefinition.E_Category, List<string>>();
		foreach (ItemRestrictionFamily itemRestrictionFamily in TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamilies)
		{
			if (itemRestrictionFamily.IsActive == areActive && (!getOnlyUnlockedFamilies || itemRestrictionFamily.HasUnlockedItems))
			{
				if (dictionary.ContainsKey(itemRestrictionFamily.ItemFamilyDefinition.ItemCategory))
				{
					dictionary[itemRestrictionFamily.ItemFamilyDefinition.ItemCategory].Add(itemRestrictionFamily.ItemFamilyDefinition.ShortId);
					continue;
				}
				dictionary.Add(itemRestrictionFamily.ItemFamilyDefinition.ItemCategory, new List<string> { itemRestrictionFamily.ItemFamilyDefinition.ShortId });
			}
		}
		TPSingleton<DebugManager>.Instance.LogDevConsole("<b>" + (areActive ? "Active" : "Inactive") + "</b> item families :");
		foreach (ItemDefinition.E_Category key in dictionary.Keys)
		{
			TPSingleton<DebugManager>.Instance.LogDevConsole(string.Format("{0} : {1}", key, string.Join(", ", dictionary[key])));
		}
	}

	/// <summary>
	/// [Debug Console] Hiển thị chi tiết WeaponsRestrictionsCategories.
	/// Lệnh: ItemRestrictionWeaponsDetails
	/// </summary>
	[DevConsoleCommand("ItemRestrictionWeaponsDetails")]
	public static void DebugItemRestrictionWeaponsDetails()
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null)
		{
			TPDebug.LogError("No ItemRestrictionManager instantiated !");
		}
		else
		{
			Debug.Log(TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories);
		}
	}

	/// <summary>
	/// [Debug Console] Bật/tắt chế độ Boundless (không giới hạn) cho weapons.
	/// Lệnh: ItemRestrictionWeaponsSetBoundless [isBoundless]
	/// </summary>
	[DevConsoleCommand("ItemRestrictionWeaponsSetBoundless")]
	public static void DebugItemRestrictionWeaponsSetBoundless(bool isBoundless)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null)
		{
			TPDebug.LogError("No ItemRestrictionManager instantiated !");
		}
		else
		{
			TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.ItemCategoriesCollectionController.SetBoundlessModeActive(isBoundless);
		}
	}

	#endregion Debug Commands
}
