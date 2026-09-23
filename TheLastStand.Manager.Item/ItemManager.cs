using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.Meta;
using TheLastStand.Database;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Helpers;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Item;
using UnityEngine;

namespace TheLastStand.Manager.Item;

/// <summary>
/// Manager Singleton trung tâm quản lý toàn bộ hệ thống sinh vật phẩm (Item Generation).
/// Đây là "nhà máy" tạo ra vật phẩm trong game, xử lý:
/// 
/// 1. SINH VẬT PHẨM (GenerateItem):
///    - Tạo Item từ ItemDefinition + Level + Rarity.
///    - Sinh Affix bonus ngẫu nhiên (theo probability, weight, max occurrences).
///    - Đánh dấu 1 Affix ngẫu nhiên là Epic nếu Rarity = Epic.
///    - Áp dụng AffixMalus (phạt) theo Apocalypse level.
///    - Đặt item vào đích (Inventory hoặc Shop).
/// 
/// 2. QUẢN LÝ DANH SÁCH VẬT PHẨM:
///    - TakeRandomItemInList: chọn ngẫu nhiên item từ danh sách (có weight, locked items, priority).
///    - GetAllItemsInList: flatten danh sách lồng nhau thành HashSet ID.
///    - GetAllLockedItemsIds: tổng hợp item bị khóa (MetaUpgrades + ItemRestrictions).
/// 
/// 3. REWARDS:
///    - NightRewardsCount / ProdRewardsCount: số lượng item thưởng sau đêm/sản xuất.
///    - Init(): sinh StartStockItems cho đầu game.
/// 
/// 4. SO SÁNH: GetStatsDiffBetweenItems - tính khác biệt stats giữa 2 item.
/// 
/// 5. DEBUG COMMANDS: sinh item theo ID/category/list, sinh tất cả potions...
/// </summary>
public class ItemManager : Manager<ItemManager>
{
	#region Nested Types

	/// <summary>
	/// Struct chứa thông tin cần thiết để sinh 1 vật phẩm.
	/// Được truyền vào GenerateItem() như parameter object.
	/// </summary>
	public struct ItemGenerationInfo
	{
		/// <summary>Đích đến: Inventory hoặc Shop.</summary>
		public ItemSlotDefinition.E_ItemSlotId Destination;

		/// <summary>Định nghĩa vật phẩm cần tạo.</summary>
		public ItemDefinition ItemDefinition;

		/// <summary>Cấp độ vật phẩm (0-10).</summary>
		public int Level;

		/// <summary>Độ hiếm (Common, Uncommon, Rare, Epic).</summary>
		public ItemDefinition.E_Rarity Rarity;

		/// <summary>True = bỏ qua bước sinh AffixMalus (dùng cho debug/special items).</summary>
		public bool SkipMalusAffixes;
	}

	#endregion Nested Types

	#region Fields

	/// <summary>Số lượng item thưởng sau mỗi đêm (base value, trước modifier).</summary>
	[SerializeField]
	private int nightRewardsCount = 3;

	/// <summary>Số lượng item thưởng trong pha sản xuất (base value, trước modifier).</summary>
	[SerializeField]
	private int prodRewardsCount = 3;

	/// <summary>
	/// Item đang được trang bị mà người chơi đang so sánh (tay chính).
	/// Dùng trong UI so sánh stats khi hover item trong Inventory.
	/// </summary>
	public TheLastStand.Model.Item.Item EquippedItemBeingCompared;

	/// <summary>
	/// Item đang được trang bị mà người chơi đang so sánh (tay phụ/off-hand).
	/// Dùng khi so sánh vũ khí 2 tay với item ở tay trái.
	/// </summary>
	public TheLastStand.Model.Item.Item EquippedItemBeingComparedOffHand;

	#endregion Fields

	#region Properties

	/// <summary>
	/// Số lượng item thưởng sau đêm = base + GlyphModifier.
	/// Glyph (biểu tượng meta) có thể tăng/giảm số lượng reward.
	/// </summary>
	public int NightRewardsCount => nightRewardsCount + TPSingleton<GlyphManager>.Instance.NightRewardsCountModifier;

	/// <summary>
	/// Số lượng item thưởng trong pha sản xuất = base + GlyphModifier.
	/// </summary>
	public int ProdRewardsCount => prodRewardsCount + TPSingleton<GlyphManager>.Instance.ProdRewardsCountModifier;

	#endregion Properties

	#region Public Methods - Sinh vật phẩm (Item Generation)

	/// <summary>
	/// Sinh 1 vật phẩm hoàn chỉnh từ ItemGenerationInfo.
	/// 
	/// Luồng xử lý chi tiết:
	/// 1. Tạo Item cơ bản (ItemController) với Definition, Level, Rarity.
	/// 2. Tính danh sách Affix khả dụng (ComputeAvailableAffixDefinitions).
	/// 3. Lặp sinh Affix ngẫu nhiên đến khi đủ số lượng theo Rarity:
	///    a. Random AffixLevel theo probability.
	///    b. Lọc Affix có LevelDefinition tương ứng (ComputePotentialAffixDefinitions).
	///    c. Random Affix theo weight → tạo AffixController → gán level → thêm vào item.
	///    d. Kiểm tra MaxOccurrences - loại bỏ Affix đã đạt tối đa.
	/// 4. Nếu Rarity = Epic → đánh dấu 1 Affix ngẫu nhiên là IsEpic = true.
	/// 5. Áp dụng AffixMalus (ApplyMaluses).
	/// 6. Đặt item vào đích (Inventory hoặc Shop).
	/// </summary>
	/// <param name="generationInfo">Thông tin sinh vật phẩm.</param>
	/// <returns>Item đã được sinh hoàn chỉnh.</returns>
	public static TheLastStand.Model.Item.Item GenerateItem(ItemGenerationInfo generationInfo)
	{
		TPSingleton<ItemManager>.Instance.Log($"Generating item {generationInfo.ItemDefinition.Id} (level {generationInfo.Level}, {generationInfo.Rarity.ToString()} rarity, headed to {generationInfo.Destination}.", CLogLevel.DETAILED);
		// Bước 1: Tạo Item cơ bản
		TheLastStand.Model.Item.Item item = new ItemController(generationInfo.ItemDefinition, generationInfo.Level, generationInfo.Rarity).Item;
		// Bước 2: Tính Affix khả dụng (lọc theo category, level, droppable, locked)
		Dictionary<AffixDefinition, float> dictionary = ComputeAvailableAffixDefinitions(generationInfo, item);
		// Probability bảng AffixLevel theo item level
		Dictionary<int, float> dictionary2 = new Dictionary<int, float>(ItemDatabase.AffixLevelsDefinition.AffixLevelsProbas[generationInfo.Level]);
		// Đếm số lần mỗi Affix xuất hiện (để check MaxOccurrences)
		Dictionary<AffixDefinition, int> dictionary3 = new Dictionary<AffixDefinition, int>();
		// Số Affix cần sinh = AffixesCountPerRarity[Rarity]
		int num = ItemDatabase.AffixesCountPerRarity[generationInfo.Rarity];
		// Bước 3: Lặp sinh Affix
		while (item.AdditionalAffixes.Count < num && dictionary.Count > 0 && dictionary2.Count > 0)
		{
			// 3a. Random AffixLevel
			int randomItemFromWeights = DictionaryHelpers.GetRandomItemFromWeights(dictionary2, TPSingleton<ItemManager>.Instance);
			// 3b. Lọc Affix có LevelDefinition cho level này
			Dictionary<AffixDefinition, float> dictionary4 = ComputePotentialAffixDefinitions(dictionary, randomItemFromWeights);
			if (dictionary4.Count == 0)
			{
				// Không có Affix nào cho level này → loại bỏ level khỏi pool
				dictionary2.Remove(randomItemFromWeights);
				continue;
			}
			// 3c. Random Affix theo weight → tạo và thêm
			AffixDefinition randomItemFromWeights2 = DictionaryHelpers.GetRandomItemFromWeights(dictionary4, TPSingleton<ItemManager>.Instance);
			Affix affix = new AffixController(randomItemFromWeights2).Affix;
			affix.Level = randomItemFromWeights;
			item.AdditionalAffixes.Add(affix);
			// 3d. Đếm occurrences và loại bỏ nếu đạt tối đa
			dictionary3.AddValueOrCreateKey(randomItemFromWeights2, 1, (int a, int b) => a + b);
			if (randomItemFromWeights2.MaxOccurrences != -1 && dictionary3[randomItemFromWeights2] >= randomItemFromWeights2.MaxOccurrences)
			{
				dictionary.Remove(randomItemFromWeights2);
			}
		}
		// Bước 4: Đánh dấu 1 Affix ngẫu nhiên là Epic nếu Rarity = Epic
		if (item.AdditionalAffixes.Count > 0 && item.Rarity == ItemDefinition.E_Rarity.Epic)
		{
			item.AdditionalAffixes[RandomManager.GetRandomRange(TPSingleton<ItemManager>.Instance, 0, item.AdditionalAffixes.Count)].IsEpic = true;
		}
		// Bước 5: Áp dụng AffixMalus
		ApplyMaluses(generationInfo, item);
		// Bước 6: Đặt item vào đích
		switch (generationInfo.Destination)
		{
		case ItemSlotDefinition.E_ItemSlotId.Inventory:
			if (TPSingleton<InventoryManager>.Instance.Inventory.ItemCount < TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots.Count)
			{
				TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.AddItem(item, null, isNewItem: true);
			}
			break;
		case ItemSlotDefinition.E_ItemSlotId.Shop:
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.AddItem(item);
			break;
		}
		return item;
	}

	/// <summary>
	/// Sinh 1 vật phẩm từ CreateItemDefinition (cấu hình sinh item từ XML).
	/// 1. Chọn ngẫu nhiên ItemDefinition từ danh sách (TakeRandomItemInList).
	/// 2. Điều chỉnh level theo ItemMinLevel (nếu có).
	/// 3. Tìm level tồn tại gần nhất (GetHigherExistingLevelFromInitValue).
	/// 4. Retry tối đa 1000 lần nếu không tìm được level phù hợp.
	/// 5. Sinh Rarity ngẫu nhiên theo probability tree.
	/// 6. Gọi GenerateItem(ItemGenerationInfo).
	/// </summary>
	/// <param name="itemDestination">Đích đến (Inventory/Shop).</param>
	/// <param name="createItemDefinition">Cấu hình sinh item từ XML.</param>
	/// <param name="level">Level mong muốn.</param>
	/// <returns>Item đã sinh.</returns>
	public static TheLastStand.Model.Item.Item GenerateItem(ItemSlotDefinition.E_ItemSlotId itemDestination, CreateItemDefinition createItemDefinition, int level)
	{
		ItemDefinition itemDefinition = TakeRandomItemInList(createItemDefinition.ItemsListDefinition);
		// Điều chỉnh level theo ItemMinLevel
		if (createItemDefinition.ItemMinLevel >= 0)
		{
			int num = createItemDefinition.ItemMinLevel;
			int minLevelInItemList = GetMinLevelInItemList(createItemDefinition.ItemsListDefinition);
			if (num < minLevelInItemList)
			{
				num = minLevelInItemList;
			}
			if (level < num)
			{
				level = num;
			}
		}
		// Tìm level tồn tại gần nhất, retry nếu không tìm được
		int higherExistingLevelFromInitValue = itemDefinition.GetHigherExistingLevelFromInitValue(level);
		int num2 = 1000;
		while (higherExistingLevelFromInitValue == -1 && --num2 > 0)
		{
			itemDefinition = TakeRandomItemInList(createItemDefinition.ItemsListDefinition);
			higherExistingLevelFromInitValue = itemDefinition.GetHigherExistingLevelFromInitValue(level);
		}
		// Sinh Rarity và tạo item
		int minRarityIndexFromItemDefinition = RarityProbabilitiesTreeController.GetMinRarityIndexFromItemDefinition(itemDefinition);
		return GenerateItem(new ItemGenerationInfo
		{
			Destination = itemDestination,
			ItemDefinition = itemDefinition,
			Level = higherExistingLevelFromInitValue,
			Rarity = RarityProbabilitiesTreeController.GenerateRarity(createItemDefinition.ItemRaritiesListDefinition, minRarityIndexFromItemDefinition)
		});
	}

	/// <summary>
	/// Sinh NHIỀU vật phẩm từ CreateItemDefinition.
	/// Số lượng = Count (có thể bị MetaUpgrade modifier thay đổi).
	/// Nếu Count = -1 (All) → sinh TẤT CẢ item trong danh sách.
	/// </summary>
	/// <param name="itemDestination">Đích đến.</param>
	/// <param name="createItemDefinition">Cấu hình sinh item.</param>
	/// <param name="level">Level mong muốn.</param>
	public static void GenerateItems(ItemSlotDefinition.E_ItemSlotId itemDestination, CreateItemDefinition createItemDefinition, int level)
	{
		Node count = createItemDefinition.Count;
		// Kiểm tra MetaUpgrade modifier có thay đổi số lượng không
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<CreateItemModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int i = 0; i < effects.Length; i++)
			{
				if (createItemDefinition.HasID && effects[i].CreateItemId == createItemDefinition.Id)
				{
					count = effects[i].Count;
					break;
				}
			}
		}
		int num = count.EvalToInt(new ItemInterpreterContext());
		if (num == CreateItemDefinition.All)
		{
			// Sinh TẤT CẢ item trong danh sách
			GenerateAllItemsInList(itemDestination, createItemDefinition.ItemsListDefinition, level, createItemDefinition.ItemRaritiesListDefinition);
			return;
		}
		// Sinh N item ngẫu nhiên
		for (int j = 0; j < num; j++)
		{
			GenerateItem(itemDestination, createItemDefinition, level);
		}
	}

	/// <summary>
	/// Sinh TẤT CẢ item trong một ItemsListDefinition.
	/// Duyệt đệ quy: nếu entry là ItemDefinition → sinh item, nếu là ItemsListDefinition → đệ quy.
	/// </summary>
	/// <param name="itemDestination">Đích đến.</param>
	/// <param name="itemsListDefinition">Danh sách item cần sinh.</param>
	/// <param name="level">Level mong muốn.</param>
	/// <param name="rarityProbability">Bảng probability rarity.</param>
	public static void GenerateAllItemsInList(ItemSlotDefinition.E_ItemSlotId itemDestination, ItemsListDefinition itemsListDefinition, int level, ProbabilityTreeEntriesDefinition rarityProbability)
	{
		foreach (KeyValuePair<string, int> item in itemsListDefinition.ItemsWithOdd)
		{
			ItemsListDefinition value2;
			if (ItemDatabase.ItemDefinitions.TryGetValue(item.Key, out var value))
			{
				int minRarityIndexFromItemDefinition = RarityProbabilitiesTreeController.GetMinRarityIndexFromItemDefinition(value);
				ItemGenerationInfo generationInfo = new ItemGenerationInfo
				{
					Destination = itemDestination,
					ItemDefinition = value,
					Level = value.GetHigherExistingLevelFromInitValue(level),
					Rarity = RarityProbabilitiesTreeController.GenerateRarity(rarityProbability, minRarityIndexFromItemDefinition)
				};
				if (generationInfo.Level != -1)
				{
					GenerateItem(generationInfo);
				}
			}
			else if (ItemDatabase.ItemsListDefinitions.TryGetValue(item.Key, out value2))
			{
				// Đệ quy cho danh sách lồng nhau
				GenerateAllItemsInList(itemDestination, value2, level, rarityProbability);
			}
			else
			{
				TPSingleton<ItemManager>.Instance.LogError("Trying to generate all items in list " + itemsListDefinition.Id + " -> Id " + item.Key + " does not refer to an item nor an items list.");
			}
		}
	}

	#endregion Public Methods - Sinh vật phẩm (Item Generation)

	#region Public Methods - Quản lý danh sách item (List Management)

	/// <summary>
	/// Lấy weight (tỉ lệ xuất hiện) của item trong danh sách.
	/// Nhân thêm GlyphManager weight multiplier nếu có.
	/// </summary>
	/// <param name="itemsListDefinition">Danh sách chứa item.</param>
	/// <param name="itemId">ID item cần lấy weight.</param>
	/// <returns>Weight cuối cùng (base × multiplier).</returns>
	public static float GetItemOddFromItemList(ItemsListDefinition itemsListDefinition, string itemId)
	{
		float num = 1f;
		if (TPSingleton<GlyphManager>.Instance.ItemWeightMultipliers.TryGetValue(itemsListDefinition.Id, out var value) && value.TryGetValue(itemId, out var value2))
		{
			num = value2;
		}
		return (float)itemsListDefinition.ItemsWithOdd[itemId] * num;
	}

	/// <summary>
	/// Kiểm tra xem TẤT CẢ nội dung của danh sách item có bị khóa không.
	/// Đệ quy kiểm tra: nếu có ít nhất 1 item chưa bị khóa → false.
	/// </summary>
	/// <param name="itemsListDefinition">Danh sách cần kiểm tra.</param>
	/// <param name="unavailableIds">Mảng ID item bị khóa.</param>
	/// <returns>True nếu TẤT CẢ item đều bị khóa.</returns>
	public static bool IsItemsListContentLocked(ItemsListDefinition itemsListDefinition, string[] unavailableIds)
	{
		foreach (KeyValuePair<string, int> item in itemsListDefinition.ItemsWithOdd)
		{
			if (ItemDatabase.ItemDefinitions.TryGetValue(item.Key, out var _) && !unavailableIds.Contains(item.Key))
			{
				return false;
			}
			if (ItemDatabase.ItemsListDefinitions.TryGetValue(item.Key, out var value2) && !IsItemsListContentLocked(value2, unavailableIds))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Kiểm tra xem có BẤT KỲ item nào trong danh sách thỏa mãn điều kiện (predicate).
	/// Đệ quy kiểm tra qua các danh sách lồng nhau. Tránh vòng lặp vô hạn bằng exploredIds.
	/// </summary>
	/// <param name="itemsListDefinition">Danh sách cần kiểm tra.</param>
	/// <param name="predicate">Điều kiện cần thỏa mãn.</param>
	/// <param name="exploredIds">Danh sách ID đã duyệt (tránh đệ quy vô hạn).</param>
	/// <returns>True nếu có ít nhất 1 item thỏa mãn.</returns>
	public static bool AnyItemMatchingCondition(ItemsListDefinition itemsListDefinition, Func<ItemDefinition, bool> predicate, List<string> exploredIds = null)
	{
		foreach (KeyValuePair<string, int> item in itemsListDefinition.ItemsWithOdd)
		{
			ItemsListDefinition value2;
			if (ItemDatabase.ItemDefinitions.TryGetValue(item.Key, out var value))
			{
				if (predicate(value))
				{
					return true;
				}
			}
			else if (ItemDatabase.ItemsListDefinitions.TryGetValue(item.Key, out value2))
			{
				if (exploredIds == null)
				{
					exploredIds = new List<string> { value2.Id };
				}
				else
				{
					exploredIds.Add(value2.Id);
				}
				if (AnyItemMatchingCondition(value2, predicate, exploredIds))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Chọn ngẫu nhiên 1 ItemDefinition từ danh sách (theo weight/tỉ lệ).
	/// 
	/// Logic:
	/// 1. Lọc bỏ item bị khóa (MetaUpgrades + ItemRestrictions).
	/// 2. Áp dụng predicate (nếu có) để lọc thêm.
	/// 3. Áp dụng priority items (nếu có) - chỉ chọn item trong danh sách ưu tiên.
	/// 4. Random theo weight.
	/// 5. Nếu kết quả là ItemsListDefinition (danh sách lồng) → đệ quy.
	/// </summary>
	/// <param name="itemsListDefinition">Danh sách item nguồn.</param>
	/// <param name="predicate">Bộ lọc bổ sung (null = không lọc).</param>
	/// <param name="exploredIds">Tránh đệ quy vô hạn.</param>
	/// <param name="priorityItemsIds">Danh sách ID item ưu tiên (null = không ưu tiên).</param>
	/// <returns>ItemDefinition được chọn, hoặc null nếu không có item nào.</returns>
	public static ItemDefinition TakeRandomItemInList(ItemsListDefinition itemsListDefinition, Func<ItemDefinition, bool> predicate = null, List<string> exploredIds = null, List<string> priorityItemsIds = null)
	{
		string[] array = GetAllLockedItemsIds().ToArray();
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		bool flag = priorityItemsIds != null && priorityItemsIds.Count > 0;
		foreach (string item in itemsListDefinition.ItemsWithOdd.Select((KeyValuePair<string, int> o) => o.Key))
		{
			bool num = array.Contains(item);
			ItemDefinition value;
			bool flag2 = ItemDatabase.ItemDefinitions.TryGetValue(item, out value);
			ItemsListDefinition value2;
			bool flag3 = ItemDatabase.ItemsListDefinitions.TryGetValue(item, out value2);
			// Kiểm tra: không bị khóa, thỏa predicate, thỏa priority
			if (!num && (!flag2 || ((predicate == null || predicate(value)) && (!flag || priorityItemsIds.Contains(item)))) && (!flag3 || (!IsItemsListContentLocked(value2, array) && (predicate == null || AnyItemMatchingCondition(value2, predicate)) && (!flag || AnyItemMatchingCondition(value2, (ItemDefinition itemDef) => priorityItemsIds.Contains(itemDef.Id))))))
			{
				dictionary.Add(item, GetItemOddFromItemList(itemsListDefinition, item));
			}
		}
		// Random theo weight
		string randomItemFromWeights = DictionaryHelpers.GetRandomItemFromWeights(dictionary, TPSingleton<ItemManager>.Instance);
		if (randomItemFromWeights == null)
		{
			return null;
		}
		// Nếu kết quả là ItemDefinition → trả về trực tiếp
		if (ItemDatabase.ItemDefinitions.TryGetValue(randomItemFromWeights, out var value3))
		{
			return value3;
		}
		// Nếu kết quả là danh sách lồng → đệ quy
		if (exploredIds == null)
		{
			exploredIds = new List<string> { randomItemFromWeights };
		}
		else
		{
			exploredIds.Add(randomItemFromWeights);
		}
		return TakeRandomItemInList(ItemDatabase.ItemsListDefinitions[randomItemFromWeights], predicate, exploredIds, priorityItemsIds);
	}

	/// <summary>
	/// Lấy tất cả Item IDs từ danh sách hỗn hợp (chứa cả ItemId lẫn ItemsListId).
	/// Flatten đệ quy các danh sách lồng nhau.
	/// </summary>
	/// <param name="itemsListIdsAndItemsIds">Danh sách hỗn hợp IDs.</param>
	/// <returns>HashSet chứa tất cả Item IDs (không trùng lặp).</returns>
	public static HashSet<string> GetAllItemsIds(List<string> itemsListIdsAndItemsIds)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string itemsListIdsAndItemsId in itemsListIdsAndItemsIds)
		{
			ItemDefinition value;
			bool num = ItemDatabase.ItemDefinitions.TryGetValue(itemsListIdsAndItemsId, out value);
			ItemsListDefinition value2;
			bool flag = ItemDatabase.ItemsListDefinitions.TryGetValue(itemsListIdsAndItemsId, out value2);
			if (num)
			{
				hashSet.Add(value.Id);
			}
			else if (flag)
			{
				hashSet.AddRange(GetAllItemsInList(value2));
			}
		}
		return hashSet;
	}

	/// <summary>
	/// Flatten một ItemsListDefinition thành HashSet chứa tất cả Item IDs.
	/// Đệ quy xử lý danh sách lồng nhau.
	/// </summary>
	/// <param name="itemsListDefinition">Danh sách cần flatten.</param>
	/// <returns>HashSet chứa tất cả Item IDs.</returns>
	public static HashSet<string> GetAllItemsInList(ItemsListDefinition itemsListDefinition)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (KeyValuePair<string, int> item in itemsListDefinition.ItemsWithOdd)
		{
			string key = item.Key;
			if (ItemDatabase.ItemDefinitions.ContainsKey(key))
			{
				hashSet.Add(key);
			}
			else if (ItemDatabase.ItemsListDefinitions.ContainsKey(key))
			{
				hashSet.UnionWith(GetAllItemsInList(ItemDatabase.ItemsListDefinitions[key]));
			}
		}
		return hashSet;
	}

	/// <summary>
	/// Tổng hợp tất cả Item IDs bị khóa (locked) từ 2 nguồn:
	/// 1. MetaUpgradesManager: item bị khóa do chưa mở MetaUpgrade.
	/// 2. ItemRestrictionManager: item bị khóa do người chơi loại bỏ (item restriction).
	/// </summary>
	/// <returns>HashSet chứa tất cả locked Item IDs.</returns>
	public static HashSet<string> GetAllLockedItemsIds()
	{
		HashSet<string> hashSet = new HashSet<string>(TPSingleton<MetaUpgradesManager>.Instance.GetLockedItemsIds());
		hashSet.UnionWith(TPSingleton<ItemRestrictionManager>.Instance.GetLockedItemsIds());
		return hashSet;
	}

	/// <summary>
	/// Tìm level thấp nhất tồn tại trong danh sách item (bỏ qua item bị khóa).
	/// Dùng để đảm bảo level sinh item không thấp hơn level tối thiểu khả dụng.
	/// </summary>
	/// <param name="itemsListDefinition">Danh sách item.</param>
	/// <returns>Level thấp nhất tìm được.</returns>
	public static int GetMinLevelInItemList(ItemsListDefinition itemsListDefinition)
	{
		string[] source = GetAllLockedItemsIds().ToArray();
		HashSet<string> allItemsInList = GetAllItemsInList(itemsListDefinition);
		int num = 999;
		foreach (string item in allItemsInList)
		{
			if (!source.Contains(item) && ItemDatabase.ItemDefinitions.TryGetValue(item, out var value))
			{
				int lowerExistingLevelFromInitValue = value.GetLowerExistingLevelFromInitValue(0);
				if (lowerExistingLevelFromInitValue < num)
				{
					num = lowerExistingLevelFromInitValue;
				}
			}
		}
		return num;
	}

	#endregion Public Methods - Quản lý danh sách item (List Management)

	#region Public Methods - So sánh và khởi tạo (Compare & Init)

	/// <summary>
	/// Tính khác biệt stats giữa baseItem và các otherItems.
	/// Kết quả = stats của baseItem - tổng stats của otherItems.
	/// Dùng trong UI tooltip để hiển thị "+5 Damage" hoặc "-3 Dodge" khi so sánh.
	/// </summary>
	/// <param name="baseItem">Item đang xem xét (item mới).</param>
	/// <param name="otherItems">Các item đang trang bị (item cũ).</param>
	/// <returns>Dictionary: E_Stat → chênh lệch (dương = tốt hơn, âm = tệ hơn).</returns>
	public Dictionary<UnitStatDefinition.E_Stat, float> GetStatsDiffBetweenItems(TheLastStand.Model.Item.Item baseItem, params TheLastStand.Model.Item.Item[] otherItems)
	{
		Dictionary<UnitStatDefinition.E_Stat, float> allStatBonusesMerged = baseItem.GetAllStatBonusesMerged();
		foreach (TheLastStand.Model.Item.Item item in otherItems)
		{
			if (item == null)
			{
				continue;
			}
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item2 in item.GetAllStatBonusesMerged())
			{
				allStatBonusesMerged.AddValueOrCreateKey(item2.Key, 0f - item2.Value, (float a, float b) => a + b);
			}
		}
		return allStatBonusesMerged;
	}

	/// <summary>
	/// Khởi tạo Inventory ban đầu (đầu game mới).
	/// Nếu Inventory trống → sinh các StartStockItems được cấu hình trong ItemDatabase.
	/// </summary>
	public void Init()
	{
		if (TPSingleton<InventoryManager>.Instance.Inventory.ItemCount != 0)
		{
			return;
		}
		foreach (CreateItemDefinition startStockItemDefinition in ItemDatabase.StartStockItemDefinitions)
		{
			GenerateItems(ItemSlotDefinition.E_ItemSlotId.Inventory, startStockItemDefinition, 0);
		}
	}

	#endregion Public Methods - So sánh và khởi tạo (Compare & Init)

	#region Private Methods - Affix Generation

	/// <summary>
	/// Áp dụng AffixMalus (phạt) cho item vừa sinh.
	/// Bỏ qua nếu: SkipMalusAffixes, Apocalypse không bật malus, hoặc item là Usable (potion/scroll).
	/// 
	/// Luồng:
	/// 1. Random MalusLevel theo probability (None/Low/Medium/High).
	/// 2. Lọc AffixMalusDefinitions có MalusLevel đó → random theo Weight.
	/// 3. Tạo AffixMalus, set level, thêm vào item.
	/// </summary>
	/// <param name="generationInfo">Thông tin sinh item.</param>
	/// <param name="item">Item cần áp dụng malus.</param>
	private static void ApplyMaluses(ItemGenerationInfo generationInfo, TheLastStand.Model.Item.Item item)
	{
		// Bỏ qua nếu skip, không bật malus, hoặc item là Usable
		if (generationInfo.SkipMalusAffixes || !ApocalypseManager.CurrentApocalypse.GenerateMalusAffixes || (ItemDefinition.E_Category.Usable & item.ItemDefinition.Category) != ItemDefinition.E_Category.None)
		{
			return;
		}
		AffixMalusDefinition.E_MalusLevel malusLevel = AffixMalusDefinition.E_MalusLevel.Undefined;
		UnitStatDefinition.E_Stat key = UnitStatDefinition.E_Stat.Undefined;
		// Bước 1: Random MalusLevel theo probability
		Dictionary<AffixMalusDefinition.E_MalusLevel, float> dictionary = ItemDatabase.AffixLevelsDefinition.AffixMalusLevelsProbas[item.Level];
		float num = RandomManager.GetRandomRange(max: dictionary.Values.Sum(), caller: TPSingleton<ItemManager>.Instance, min: 0f);
		float num2 = 0f;
		foreach (KeyValuePair<AffixMalusDefinition.E_MalusLevel, float> item2 in dictionary)
		{
			num2 += item2.Value;
			if (num <= num2)
			{
				malusLevel = item2.Key;
				break;
			}
		}
		// Bước 2: Lọc AffixMalus có MalusLevel đó, random theo Weight
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary2 = ItemDatabase.AffixMalusDefinitions.Where((KeyValuePair<UnitStatDefinition.E_Stat, AffixMalusDefinition> o) => o.Value.IsMalusLevelDefined(malusLevel)).ToDictionary((Func<KeyValuePair<UnitStatDefinition.E_Stat, AffixMalusDefinition>, UnitStatDefinition.E_Stat>)((KeyValuePair<UnitStatDefinition.E_Stat, AffixMalusDefinition> k) => k.Value.Stat), (Func<KeyValuePair<UnitStatDefinition.E_Stat, AffixMalusDefinition>, float>)((KeyValuePair<UnitStatDefinition.E_Stat, AffixMalusDefinition> v) => v.Value.Weight));
		num = RandomManager.GetRandomRange(max: dictionary2.Values.Sum(), caller: TPSingleton<ItemManager>.Instance, min: 0f);
		num2 = 0f;
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item3 in dictionary2)
		{
			num2 += item3.Value;
			if (num <= num2)
			{
				key = item3.Key;
				break;
			}
		}
		// Bước 3: Tạo AffixMalus và thêm vào item
		AffixMalus affixMalus = new AffixMalusController(ItemDatabase.AffixMalusDefinitions[key]).AffixMalus;
		affixMalus.AffixMalusController.SetLevel(malusLevel);
		item.AdditionalAffixesMalus.Add(affixMalus);
	}

	/// <summary>
	/// Lọc Affix definitions có LevelDefinition tương ứng với rarity level.
	/// Dùng để thu hẹp pool Affix trước khi random.
	/// </summary>
	/// <param name="availableAffixDefinitions">Pool Affix khả dụng.</param>
	/// <param name="rarity">Rarity level cần kiểm tra.</param>
	/// <returns>Dictionary Affix có LevelDefinition cho rarity đó.</returns>
	private static Dictionary<AffixDefinition, float> ComputePotentialAffixDefinitions(Dictionary<AffixDefinition, float> availableAffixDefinitions, int rarity)
	{
		Dictionary<AffixDefinition, float> dictionary = new Dictionary<AffixDefinition, float>();
		foreach (KeyValuePair<AffixDefinition, float> availableAffixDefinition in availableAffixDefinitions)
		{
			if (availableAffixDefinition.Key.LevelDefinitions.ContainsKey(rarity))
			{
				dictionary.Add(availableAffixDefinition.Key, availableAffixDefinition.Value);
			}
		}
		return dictionary;
	}

	/// <summary>
	/// Tính toàn bộ Affix definitions khả dụng cho item đang sinh.
	/// Lọc theo nhiều điều kiện:
	/// - Droppable = true (Affix có thể rơi ngẫu nhiên).
	/// - Level nằm trong [LevelMin, LevelMax].
	/// - Category của Affix phải match với category của item.
	/// - Affix không bị khóa bởi MetaUpgrade.
	/// </summary>
	/// <param name="generationInfo">Thông tin sinh item.</param>
	/// <param name="item">Item đang sinh.</param>
	/// <returns>Dictionary: AffixDefinition → weight (tỉ lệ xuất hiện).</returns>
	private static Dictionary<AffixDefinition, float> ComputeAvailableAffixDefinitions(ItemGenerationInfo generationInfo, TheLastStand.Model.Item.Item item)
	{
		Dictionary<AffixDefinition, float> dictionary = new Dictionary<AffixDefinition, float>();
		string[] lockedAffixesIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedAffixesIds();
		foreach (KeyValuePair<string, AffixDefinition> affixDefinition in ItemDatabase.AffixDefinitions)
		{
			AffixDefinition value = affixDefinition.Value;
			ItemDefinition.E_Category e_Category = ItemDefinition.E_Category.None;
			// Tìm category match giữa Affix và item
			foreach (KeyValuePair<ItemDefinition.E_Category, float> item2 in value.ItemCategoriesWithWeight)
			{
				if ((item2.Key & item.ItemDefinition.Category) != ItemDefinition.E_Category.None)
				{
					e_Category = item2.Key;
					break;
				}
			}
			// Kiểm tra tất cả điều kiện
			if (value.Droppable && generationInfo.Level >= value.LevelMin && generationInfo.Level <= value.LevelMax && e_Category != ItemDefinition.E_Category.None && !lockedAffixesIds.Contains(value.Id))
			{
				dictionary.Add(affixDefinition.Value, value.ItemCategoriesWithWeight[e_Category]);
			}
		}
		return dictionary;
	}

	#endregion Private Methods - Affix Generation

	#region Debug Commands

	/// <summary>
	/// [Debug Console] Sinh vật phẩm theo ID.
	/// Lệnh: GenerateItem [itemId] [level] [rarity] [amount] [skipMalus]
	/// </summary>
	[DevConsoleCommand(Name = "GenerateItem")]
	public static void DebugGenerateItem([StringConverter(typeof(TheLastStand.Model.Item.Item.StringToItemIdConverter))] string itemId, int level = 0, ItemDefinition.E_Rarity rarity = ItemDefinition.E_Rarity.None, int amountToGenerate = 1, bool skipMalusAffixes = false)
	{
		amountToGenerate = Mathf.Max(amountToGenerate, 1);
		if (!ItemDatabase.ItemDefinitions.TryGetValue(itemId, out var value))
		{
			TPDebug.LogError("No item found with the Id " + itemId + "!");
			return;
		}
		bool flag = rarity == ItemDefinition.E_Rarity.None;
		for (int i = 0; i < amountToGenerate; i++)
		{
			if (flag)
			{
				rarity = (ItemDefinition.E_Rarity)RandomManager.GetRandomRange(TPSingleton<ItemManager>.Instance, 1, 5);
			}
			ItemGenerationInfo generationInfo = new ItemGenerationInfo
			{
				Destination = ItemSlotDefinition.E_ItemSlotId.Inventory,
				ItemDefinition = value,
				Level = value.GetHigherExistingLevelFromInitValue(level),
				Rarity = rarity,
				SkipMalusAffixes = skipMalusAffixes
			};
			if (generationInfo.Level != -1)
			{
				GenerateItem(generationInfo);
			}
			else
			{
				TPDebug.LogError("Couldn't generate item " + itemId + " due to level not being found !");
			}
		}
	}

	/// <summary>
	/// [Debug Console] Sinh tất cả potions (7 loại × 6 levels).
	/// Lệnh: GenerateAllPotions
	/// </summary>
	[DevConsoleCommand(Name = "GenerateAllPotions")]
	public static void DebugGenerateAllPotions()
	{
		string[] array = new string[7] { "HealthPotion", "ManaPotion", "EnergyPotion", "SpeedPotion", "InvisibilityPotion", "StonePotion", "StrengthPotion" };
		for (int i = 0; i < array.Length; i++)
		{
			if (!ItemDatabase.ItemDefinitions.TryGetValue(array[i], out var value))
			{
				continue;
			}
			for (int j = 0; j <= 5; j++)
			{
				ItemGenerationInfo generationInfo = new ItemGenerationInfo
				{
					Destination = ItemSlotDefinition.E_ItemSlotId.Inventory,
					ItemDefinition = value,
					Level = value.GetHigherExistingLevelFromInitValue(j),
					Rarity = ItemDefinition.E_Rarity.Common
				};
				if (generationInfo.Level != -1)
				{
					GenerateItem(generationInfo);
				}
			}
		}
	}

	/// <summary>
	/// [Debug Console] Sinh vật phẩm theo category.
	/// Lệnh: GenerateItemByCategory [category] [level] [rarity] [amount] [skipMalus]
	/// </summary>
	[DevConsoleCommand(Name = "GenerateItemByCategory")]
	public static void DebugGenerateItemByCategory(ItemDefinition.E_Category category, int level, ItemDefinition.E_Rarity rarity, int amountToGenerate = 1, bool skipMalusAffixes = false)
	{
		amountToGenerate = Mathf.Max(amountToGenerate, 1);
		List<ItemDefinition> list = new List<ItemDefinition>();
		foreach (KeyValuePair<string, ItemDefinition> itemDefinition2 in ItemDatabase.ItemDefinitions)
		{
			if (category == ItemDefinition.E_Category.None || (itemDefinition2.Value.Category & category) != ItemDefinition.E_Category.None)
			{
				list.Add(itemDefinition2.Value);
			}
		}
		if (list.Count == 0)
		{
			TPDebug.LogError($"No item found with the category {category}!");
			return;
		}
		bool flag = rarity == ItemDefinition.E_Rarity.None;
		for (int i = 0; i < amountToGenerate; i++)
		{
			if (flag)
			{
				rarity = (ItemDefinition.E_Rarity)RandomManager.GetRandomRange(TPSingleton<ItemManager>.Instance, 1, 5);
			}
			list = RandomManager.Shuffle(TPSingleton<ItemManager>.Instance, list).ToList();
			ItemDefinition itemDefinition = list[0];
			level = itemDefinition.GetLowerExistingLevelFromInitValue(level);
			GenerateItem(new ItemGenerationInfo
			{
				Destination = ItemSlotDefinition.E_ItemSlotId.Inventory,
				ItemDefinition = itemDefinition,
				Level = list[0].GetLowerExistingLevelFromInitValue(level),
				Rarity = rarity,
				SkipMalusAffixes = skipMalusAffixes
			});
		}
	}

	/// <summary>
	/// [Debug Console] Hiển thị tất cả Item IDs trong danh sách.
	/// Lệnh: ShowAllItemsInList [listId]
	/// </summary>
	[DevConsoleCommand(Name = "ShowAllItemsInList")]
	public static void DebugShowAllItemsInList([StringConverter(typeof(TheLastStand.Model.Item.Item.StringToItemsListIdConverter))] string listId)
	{
		TPSingleton<ItemManager>.Instance.Log(string.Join(", ", GetAllItemsInList(ItemDatabase.ItemsListDefinitions[listId])), CLogLevel.NORMAL, forcePrintInUnity: true);
	}

	/// <summary>
	/// [Debug Console] Kiểm tra xem có item 1 tay (OneHand) chưa bị khóa trong danh sách.
	/// Lệnh: AnyUnlockedOneArmedItemInList [listId]
	/// </summary>
	[DevConsoleCommand("AnyUnlockedOneArmedItemInList")]
	public static void AnyUnlockedOneArmedItemInList([StringConverter(typeof(TheLastStand.Model.Item.Item.StringToItemsListIdConverter))] string itemsListDefinitionId)
	{
		List<string> unlockedItemIds = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockEquipmentGenerationMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int i = 0; i < effects.Length; i++)
			{
				unlockedItemIds.Add(effects[i].Id);
			}
		}
		string[] lockedItemsIds = GetAllLockedItemsIds().ToArray();
		if (AnyItemMatchingCondition(ItemDatabase.ItemsListDefinitions[itemsListDefinitionId], (ItemDefinition item) => item.Hands == ItemDefinition.E_Hands.OneHand && (!lockedItemsIds.Contains(item.Id) || unlockedItemIds.Contains(item.Id))))
		{
			Debug.LogError("At least one available OneHand item has been found in list " + itemsListDefinitionId);
		}
		else
		{
			Debug.LogError("No available OneHand item has been found in list " + itemsListDefinitionId);
		}
	}

	/// <summary>
	/// [Debug Console] Sinh N item ngẫu nhiên từ danh sách.
	/// Lệnh: GenerateItemsInList [listId] [level] [rarityList] [amount] [skipMalus]
	/// </summary>
	[DevConsoleCommand(Name = "GenerateItemsInList")]
	public static void DebugGenerateItemsInList([StringConverter(typeof(TheLastStand.Model.Item.Item.StringToItemsListIdConverter))] string itemsListDefinitionId, int level = 0, [StringConverter(typeof(TheLastStand.Model.Item.Item.StringToRarityProbabilityListIdConverter))] string rarityProbabilityList = "AlwaysCommon", int amountToGenerate = 1, bool skipMalusAffixes = false)
	{
		if (!ItemDatabase.ItemsListDefinitions.TryGetValue(itemsListDefinitionId, out var value))
		{
			return;
		}
		ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition = ItemDatabase.ItemRaritiesListDefinitions[rarityProbabilityList];
		for (int i = 0; i < amountToGenerate; i++)
		{
			ItemDefinition itemDefinition = TakeRandomItemInList(value);
			if (itemDefinition.GetHigherExistingLevelFromInitValue(level) != -1)
			{
				int minRarityIndexFromItemDefinition = RarityProbabilitiesTreeController.GetMinRarityIndexFromItemDefinition(itemDefinition);
				GenerateItem(new ItemGenerationInfo
				{
					ItemDefinition = itemDefinition,
					Destination = ItemSlotDefinition.E_ItemSlotId.Inventory,
					Level = level,
					Rarity = RarityProbabilitiesTreeController.GenerateRarity(probabilityTreeEntriesDefinition, minRarityIndexFromItemDefinition),
					SkipMalusAffixes = skipMalusAffixes
				});
			}
		}
	}

	/// <summary>
	/// [Debug Console] Sinh TẤT CẢ item trong danh sách.
	/// Lệnh: GenerateAllItemsInList [listId] [level] [rarityList]
	/// </summary>
	[DevConsoleCommand(Name = "GenerateAllItemsInList")]
	public static void DebugGenerateAllItemsInList([StringConverter(typeof(TheLastStand.Model.Item.Item.StringToItemsListIdConverter))] string itemsListDefinitionId, int level = 0, [StringConverter(typeof(TheLastStand.Model.Item.Item.StringToRarityProbabilityListIdConverter))] string rarityProbabilityList = "AlwaysCommon")
	{
		if (ItemDatabase.ItemsListDefinitions.TryGetValue(itemsListDefinitionId, out var value))
		{
			GenerateAllItemsInList(ItemSlotDefinition.E_ItemSlotId.Inventory, value, level, ItemDatabase.ItemRaritiesListDefinitions[rarityProbabilityList]);
		}
	}

	#endregion Debug Commands
}
