using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.Skill;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.Database;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Item;
using UnityEngine;

namespace TheLastStand.Model.Item;

/// <summary>
/// Model chính đại diện cho một vật phẩm (Item) trong game.
/// Implement ISkillContainer (chứa skills), IPerkUnlocker (mở khóa perks),
/// ISerializable + IDeserializable (save/load).
/// 
/// Đây là lớp dữ liệu runtime trung tâm của hệ thống vật phẩm, chứa:
/// - Thông tin cơ bản: ItemDefinition, Level, Rarity, Resistance, Name
/// - Affix bonus/malus: AdditionalAffixes, AdditionalAffixesMalus
/// - Skills: danh sách kỹ năng gắn với item (theo level)
/// - Perks: danh sách perks gắn với item (theo level)
/// - Giá: FinalPrice, SellingPrice, DefaultSellingPrice
/// - Vị trí: ItemSlot (ô đang chứa item), Holder (tướng đang trang bị)
/// 
/// Item ≠ ItemDefinition:
/// - ItemDefinition = blueprint (cấu hình chung, dùng chung cho mọi item cùng loại).
/// - Item = instance cụ thể (có Level, Rarity, Affixes riêng).
/// </summary>
public class Item : ISkillContainer, IPerkUnlocker, ISerializable, IDeserializable
{
	#region Nested Types - Constants & Converters

	/// <summary>
	/// Các hằng số toàn cục liên quan đến hệ thống vật phẩm.
	/// </summary>
	public static class Constants
	{
		/// <summary>Các ID vật phẩm đặc biệt được hardcode.</summary>
		public static class Ids
		{
			/// <summary>ID vật phẩm Bia (Beer) - item tiêu thụ đặc biệt.</summary>
			public const string Beer = "Beer";
		}

		/// <summary>Đường dẫn prefix cho sprite category của item.</summary>
		public const string ItemCategoryPathPrefix = "View/Sprites/UI/Items/Categories/Icon_ItemCategory_";

		/// <summary>Đường dẫn prefix cho sprite hands type của item.</summary>
		public const string ItemHandsPathPrefix = "View/Sprites/UI/Items/Hands/Icon_ItemCategory_";

		/// <summary>Đường dẫn prefix cho sprite rarity frame của item.</summary>
		public const string LevelRarityPathPrefix = "View/Sprites/UI/Items/Rarity/ItemBox_0";

		/// <summary>Level tối đa của vật phẩm (0-10).</summary>
		public const int LevelMax = 10;
	}

	/// <summary>
	/// Converter chuỗi thành Item ID - dùng cho Debug Console.
	/// Cung cấp autocomplete danh sách tất cả ItemDefinition IDs.
	/// </summary>
	public class StringToItemIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(ItemDatabase.ItemDefinitions.Keys);
	}

	/// <summary>
	/// Converter chuỗi thành Items List ID - dùng cho Debug Console.
	/// Cung cấp autocomplete danh sách tất cả ItemsListDefinition IDs.
	/// </summary>
	public class StringToItemsListIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(ItemDatabase.ItemsListDefinitions.Keys);
	}

	/// <summary>
	/// Converter chuỗi thành Rarity Probability List ID - dùng cho Debug Console.
	/// Cung cấp autocomplete danh sách tất cả ItemRaritiesListDefinition IDs.
	/// </summary>
	public class StringToRarityProbabilityListIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(ItemDatabase.ItemRaritiesListDefinitions.Keys);
	}

	#endregion Nested Types - Constants & Converters

	#region Fields

	/// <summary>
	/// Ngữ cảnh phiên dịch công thức giá - cung cấp biến cho Expression Interpreter
	/// khi tính FinalPrice/SellingPrice.
	/// </summary>
	private ItemInterpreterContext itemInterpreterContext;

	#endregion Fields

	#region Properties - Affix (Bonus & Malus)

	/// <summary>
	/// Danh sách Affix bonus gắn trên item. Mỗi Affix cộng thêm stat modifier.
	/// Được sinh ngẫu nhiên khi tạo item dựa trên Rarity.
	/// </summary>
	public List<Affix> AdditionalAffixes { get; set; } = new List<Affix>();

	/// <summary>
	/// Danh sách Affix malus (phạt) gắn trên item. Mỗi AffixMalus TRỪ stat modifier.
	/// Số lượng/mức phạt tỉ lệ với chất lượng Affix bonus (trade-off).
	/// </summary>
	public List<AffixMalus> AdditionalAffixesMalus { get; set; } = new List<AffixMalus>();

	#endregion Properties - Affix (Bonus & Malus)

	#region Properties - Thông tin cơ bản (Core Info)

	/// <summary>
	/// Tướng (PlayableUnit) đang trang bị item này.
	/// Null nếu item nằm trong Inventory hoặc Shop (chưa trang bị).
	/// Truy cập qua ItemSlot → EquipmentSlot → PlayableUnit.
	/// </summary>
	public ISkillCaster Holder
	{
		get
		{
			if (!(ItemSlot is EquipmentSlot equipmentSlot))
			{
				return null;
			}
			return equipmentSlot.PlayableUnit;
		}
	}

	/// <summary>
	/// Controller xử lý logic cho item (merge affixes, refill uses, start turn...).
	/// </summary>
	public ItemController ItemController { get; }

	/// <summary>
	/// Định nghĩa (Definition) của item - blueprint chứa cấu hình chung.
	/// </summary>
	public ItemDefinition ItemDefinition { get; private set; }

	/// <summary>
	/// Ô slot đang chứa item này (EquipmentSlot, InventorySlot, ShopSlot...).
	/// Null nếu item không nằm trong slot nào.
	/// </summary>
	public ItemSlot ItemSlot { get; set; }

	/// <summary>
	/// Cấp độ hiện tại của item (0-10). Level cao → stats mạnh hơn.
	/// </summary>
	public int Level { get; set; }

	/// <summary>
	/// Độ hiếm của item (Common, Uncommon, Rare, Epic, Legendary).
	/// Ảnh hưởng đến số lượng Affix và giá bán.
	/// </summary>
	public ItemDefinition.E_Rarity Rarity { get; set; }

	/// <summary>
	/// Chỉ số kháng cự (Resistance) của item. Random trong khoảng [min, max] khi tạo.
	/// Cộng vào stat Resistance của tướng khi trang bị.
	/// </summary>
	public int Resistance { get; set; }

	/// <summary>
	/// True nếu item đã từng được bán trước đó.
	/// Dùng để tracking lịch sử giao dịch.
	/// </summary>
	public bool HasBeenSoldBefore { get; set; }

	/// <summary>True nếu item là vũ khí 2 tay (chiếm cả RightHand + LeftHand).</summary>
	public bool IsTwoHandedWeapon => ItemDefinition.Hands == ItemDefinition.E_Hands.TwoHands;

	/// <summary>True nếu item là Bia (Beer) - item đặc biệt.</summary>
	public bool IsBeer => ItemDefinition.Id.Contains("Beer");

	#endregion Properties - Thông tin cơ bản (Core Info)

	#region Properties - Stats & Damage

	/// <summary>
	/// Damage cơ sở theo level hiện tại. Vector2: x=min, y=max damage.
	/// </summary>
	public Vector2 BaseDamages => ItemDefinition.BaseDamageByLevel[Level];

	/// <summary>
	/// Stat bonuses cơ sở theo level (ngoài Affix). Dictionary: E_Stat → giá trị.
	/// </summary>
	public Dictionary<UnitStatDefinition.E_Stat, float> BaseStatBonuses => ItemDefinition.BaseStatBonusesByLevel[Level];

	/// <summary>
	/// Stat bonus chính (main stat) theo level. Tuple: (E_Stat, giá trị).
	/// Đây là stat nổi bật nhất của item (hiển thị đầu tiên trong UI).
	/// </summary>
	public Tuple<UnitStatDefinition.E_Stat, float> MainStatBonusByLevel => ItemDefinition.MainStatBonusByLevel[Level];

	#endregion Properties - Stats & Damage

	#region Properties - Giá (Price)

	/// <summary>
	/// Giá mua cuối cùng (sau khi tính modifier extra percentage).
	/// FinalPrice = BasePrice (từ công thức) + extra % từ ResourceManager.
	/// </summary>
	public int FinalPrice
	{
		get
		{
			int num = Mathf.RoundToInt(ItemDatabase.ItemPriceEquation.EvalToFloat(itemInterpreterContext));
			int num2 = 0;
			num2 += ResourceManager.ComputeExtraPercentageForCost(ResourceManager.E_PriceModifierType.Items, ResourceManager.E_ResourceType.Gold);
			return num + Mathf.RoundToInt((float)(num * num2) / 100f);
		}
	}

	/// <summary>
	/// Giá bán mặc định (chưa nhân SellingMultiplier của Shop).
	/// Tính bằng ItemPriceEquation với ItemInterpreterContext.
	/// </summary>
	public int DefaultSellingPrice => Mathf.RoundToInt(ItemDatabase.ItemPriceEquation.EvalToFloat(itemInterpreterContext));

	/// <summary>
	/// Giá bán thực tế = DefaultSellingPrice × SellingMultiplier / 100.
	/// SellingMultiplier có thể thay đổi nhờ nâng cấp Shop.
	/// </summary>
	public int SellingPrice => Mathf.FloorToInt((float)DefaultSellingPrice * TPSingleton<BuildingManager>.Instance.Shop.SellingMultiplier / 100f);

	#endregion Properties - Giá (Price)

	#region Properties - Hiển thị (Display)

	/// <summary>
	/// Tên hiển thị của item: BaseName + "+Level" (ví dụ: "Iron Sword +3").
	/// </summary>
	public string Name => ItemDefinition.BaseName + ((Level > 0) ? $" +{Level}" : "");

	/// <summary>
	/// Tên độ hiếm đã localize (dịch theo ngôn ngữ).
	/// Ví dụ: "Common", "Rare", "Epic" (hoặc bản dịch tương ứng).
	/// </summary>
	public string RarityName => Localizer.Get(string.Format("{0}{1}", "RarityName_", Rarity));

	#endregion Properties - Hiển thị (Display)

	#region Properties - Skills & Perks

	/// <summary>
	/// Danh sách Perk gắn với item theo level.
	/// Dictionary: PerkId → Perk instance.
	/// </summary>
	public Dictionary<string, Perk> Perks { get; } = new Dictionary<string, Perk>();

	/// <summary>
	/// HashSet chứa ID của tất cả Perks (dùng để kiểm tra nhanh).
	/// </summary>
	public HashSet<string> PerksId { get; private set; } = new HashSet<string>();

	/// <summary>
	/// Danh sách kỹ năng gốc của item (gắn theo ItemDefinition + Level).
	/// </summary>
	public List<TheLastStand.Model.Skill.Skill> Skills { get; } = new List<TheLastStand.Model.Skill.Skill>();

	/// <summary>
	/// Danh sách kỹ năng thay thế do Perk cung cấp.
	/// Perk có thể thay thế skill gốc bằng phiên bản nâng cao.
	/// </summary>
	public List<TheLastStand.Model.Skill.Skill> PerkReplacementSkills { get; } = new List<TheLastStand.Model.Skill.Skill>();

	/// <summary>
	/// Dictionary: SkillId → OverallUses (số lần dùng tổng thể) theo level hiện tại.
	/// -1 = không giới hạn. Lấy từ ItemDefinition.SkillsByLevel[Level].
	/// </summary>
	public Dictionary<string, int> SkillsOverallUses => ItemDefinition.SkillsByLevel[Level];

	#endregion Properties - Skills & Perks

	#region Constructors

	/// <summary>
	/// Constructor khởi tạo Item từ dữ liệu save (deserialization).
	/// Tạo ItemInterpreterContext để tính giá, sau đó gọi Deserialize().
	/// </summary>
	/// <param name="container">Dữ liệu serialized.</param>
	/// <param name="itemController">Controller quản lý item.</param>
	/// <param name="itemSlot">Ô slot đang chứa item.</param>
	public Item(SerializedItem container, ItemController itemController, ItemSlot itemSlot)
	{
		ItemController = itemController;
		ItemSlot = itemSlot;
		itemInterpreterContext = new ItemInterpreterContext(this);
		Deserialize(container);
	}

	/// <summary>
	/// Constructor tạo Item mới từ ItemDefinition (khi sinh vật phẩm).
	/// Level, Rarity, Resistance sẽ được set sau bởi ItemController constructor.
	/// </summary>
	/// <param name="itemDefinition">Định nghĩa vật phẩm.</param>
	/// <param name="itemController">Controller quản lý item.</param>
	public Item(ItemDefinition itemDefinition, ItemController itemController)
	{
		ItemDefinition = itemDefinition;
		ItemController = itemController;
		itemInterpreterContext = new ItemInterpreterContext(this);
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Tổng hợp TẤT CẢ stat bonuses của item thành 1 dictionary duy nhất.
	/// Bao gồm: Resistance + MainStatBonus + BaseStatBonuses + tất cả Affix (bonus - malus).
	/// Dùng khi cần biết tổng cộng item cung cấp bao nhiêu stats.
	/// </summary>
	/// <returns>Dictionary: E_Stat → tổng giá trị bonus từ mọi nguồn.</returns>
	public Dictionary<UnitStatDefinition.E_Stat, float> GetAllStatBonusesMerged()
	{
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary = new Dictionary<UnitStatDefinition.E_Stat, float>();
		// 1. Thêm Resistance
		dictionary.Add(UnitStatDefinition.E_Stat.Resistance, Resistance);
		// 2. Thêm Main Stat Bonus
		if (MainStatBonusByLevel != null)
		{
			dictionary.AddValueOrCreateKey(MainStatBonusByLevel.Item1, MainStatBonusByLevel.Item2, (float a, float b) => a + b);
		}
		// 3. Thêm Base Stat Bonuses
		if (BaseStatBonuses != null)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> baseStatBonuse in BaseStatBonuses)
			{
				dictionary.AddValueOrCreateKey(baseStatBonuse.Key, baseStatBonuse.Value, (float a, float b) => a + b);
			}
		}
		// 4. Thêm Affix bonuses - Affix maluses (ròng)
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item in ItemController.MergeAllAffixes())
		{
			dictionary.AddValueOrCreateKey(item.Key, item.Value, (float a, float b) => a + b);
		}
		return dictionary;
	}

	/// <summary>
	/// Tìm kỹ năng thay thế (Replacement Skill) theo ID.
	/// </summary>
	/// <param name="skillId">ID kỹ năng cần tìm.</param>
	/// <returns>Skill thay thế tương ứng.</returns>
	/// <exception cref="InvalidOperationException">Nếu không tìm thấy.</exception>
	public TheLastStand.Model.Skill.Skill GetReplacementSkill(string skillId)
	{
		return PerkReplacementSkills.First((TheLastStand.Model.Skill.Skill replacementSkill) => replacementSkill.Id == skillId);
	}

	/// <summary>
	/// Tìm kỹ năng theo ID - ưu tiên tìm trong Skills gốc, fallback sang PerkReplacementSkills.
	/// </summary>
	/// <param name="skillId">ID kỹ năng cần tìm.</param>
	/// <returns>Skill tìm được (gốc hoặc thay thế).</returns>
	public TheLastStand.Model.Skill.Skill GetSkillOrReplacementSkillFromId(string skillId)
	{
		return Skills.Find((TheLastStand.Model.Skill.Skill aSkill) => aSkill.Id == skillId) ?? GetReplacementSkill(skillId);
	}

	/// <summary>
	/// Kiểm tra xem item có kỹ năng thay thế với ID chỉ định không.
	/// </summary>
	/// <param name="skillId">ID kỹ năng cần kiểm tra.</param>
	/// <returns>True nếu tồn tại replacement skill với ID đó.</returns>
	public bool HasReplacementSkill(string skillId)
	{
		return PerkReplacementSkills.Any((TheLastStand.Model.Skill.Skill replacementSkill) => replacementSkill.Id == skillId);
	}

	/// <summary>
	/// Mô tả chi tiết item (debug/logging).
	/// Liệt kê: Id, Category, Rarity, Level, BaseStatBonuses, Affixes, AffixMalus,
	/// Resistance, Skills (với số lần dùng), Perks.
	/// </summary>
	public override string ToString()
	{
		string text = ItemDefinition.Id + "\n" + $"Category: {ItemDefinition.Category}\n" + $"Rarity: {Rarity}\n" + $"Level: {Level}";
		if (BaseStatBonuses != null)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> baseStatBonuse in BaseStatBonuses)
			{
				text += $"\nBase Stat Bonuses: {baseStatBonuse.Key} / {baseStatBonuse.Value} ";
			}
		}
		text += "\n";
		foreach (Affix additionalAffix in AdditionalAffixes)
		{
			text = text + "\nGenerated Affix: " + additionalAffix.AffixDefinition.Id;
		}
		text += "\n";
		foreach (AffixMalus additionalAffixesMalu in AdditionalAffixesMalus)
		{
			text += $"\nGenerated Malus Affix stat: {additionalAffixesMalu.AffixMalusDefinition.Stat}";
		}
		text += "\n";
		if (ItemDefinition.Resistance != Vector2Int.zero)
		{
			text += $"\n*Resistance:  {ItemDefinition.Resistance.x} - {ItemDefinition.Resistance.y}";
		}
		if (ItemDefinition.SkillsByLevel.TryGetValue(Level, out var value) && value != null)
		{
			foreach (KeyValuePair<string, int> item in value)
			{
				text = text + "\n*Skill " + item.Key;
				if (item.Value != -1)
				{
					text += $" (nb uses: {item.Value})";
				}
			}
		}
		if (ItemDefinition.PerksByLevel.TryGetValue(Level, out var value2) && value2 != null)
		{
			foreach (string item2 in value2)
			{
				text = text + "\n*Perk " + item2;
			}
		}
		return text + "\n";
	}

	#endregion Public Methods

	#region Serialization / Deserialization

	/// <summary>
	/// Deserialize Item từ save data.
	/// Thứ tự xử lý:
	/// 1. Tra cứu ItemDefinition từ database (throw MissingAssetException nếu không tìm thấy).
	/// 2. Đọc: Level, Resistance, Rarity, HasBeenSoldBefore.
	/// 3. Tạo Perks theo level (từ ItemDefinition.PerksByLevel).
	/// 4. Deserialize PerkReplacementSkills.
	/// 5. Deserialize Skills (match với ItemDefinition.SkillsByLevel).
	/// 6. Tạo Affix từ danh sách SerializedAffix.
	/// 7. Tạo AffixMalus từ danh sách SerializedAffixMalus.
	/// </summary>
	/// <param name="container">Dữ liệu serialized.</param>
	/// <param name="saveVersion">Phiên bản save.</param>
	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (!(container is SerializedItem serializedItem))
		{
			return;
		}
		// 1. Tra cứu ItemDefinition
		try
		{
			ItemDefinition = ItemDatabase.ItemDefinitions[serializedItem.Id];
		}
		catch (KeyNotFoundException)
		{
			throw new Database<ItemDatabase>.MissingAssetException(serializedItem.Id);
		}
		// 2. Đọc thông tin cơ bản
		Level = serializedItem.Level;
		Resistance = serializedItem.Resistance;
		Rarity = serializedItem.Rarity;
		HasBeenSoldBefore = serializedItem.HasBeenSoldBefore;
		// 3. Tạo Perks theo level
		if (ItemDefinition.PerksByLevel.ContainsKey(Level) && ItemDefinition.PerksByLevel[Level] != null)
		{
			foreach (string item in ItemDefinition.PerksByLevel[Level])
			{
				if (PlayableUnitDatabase.PerkDefinitions.ContainsKey(item) && !Perks.ContainsKey(item))
				{
					Perks.Add(item, new PerkController(PlayableUnitDatabase.PerkDefinitions[item], null, null, null, string.Empty, isNative: false, isFromRace: false).Perk);
				}
				PerksId.Add(item);
			}
		}
		// 4. Deserialize PerkReplacementSkills
		DeserializePerkReplacementSkills(serializedItem.PerkReplacementSkills);
		// 5. Deserialize Skills
		DeserializeSkills(serializedItem.Skills);
		// 6. Tạo Affix bonus từ save data
		foreach (SerializedAffix affix in serializedItem.Affixes)
		{
			AdditionalAffixes.Add(new AffixController(affix).Affix);
		}
		// 7. Tạo AffixMalus từ save data
		foreach (SerializedAffixMalus affixesMalu in serializedItem.AffixesMalus)
		{
			AdditionalAffixesMalus.Add(new AffixMalusController(affixesMalu).AffixMalus);
		}
	}

	/// <summary>
	/// Serialize Item thành dữ liệu lưu game.
	/// Lưu: Id, Level, Resistance, Rarity, HasBeenSoldBefore,
	/// Skills, PerkReplacementSkills, Affixes, AffixesMalus.
	/// </summary>
	/// <returns>SerializedItem chứa toàn bộ dữ liệu cần lưu.</returns>
	public ISerializedData Serialize()
	{
		return new SerializedItem
		{
			Id = ItemDefinition.Id,
			Level = Level,
			Resistance = Resistance,
			Rarity = Rarity,
			HasBeenSoldBefore = HasBeenSoldBefore,
			Skills = Skills.Select((TheLastStand.Model.Skill.Skill o) => o.Serialize() as SerializedSkill).ToList(),
			PerkReplacementSkills = PerkReplacementSkills.Select((TheLastStand.Model.Skill.Skill o) => o.Serialize() as SerializedSkill).ToList(),
			Affixes = AdditionalAffixes.Select((Affix o) => o.Serialize() as SerializedAffix).ToList(),
			AffixesMalus = AdditionalAffixesMalus.Select((AffixMalus o) => o.Serialize() as SerializedAffixMalus).ToList()
		};
	}

	#endregion Serialization / Deserialization

	#region Private Methods

	/// <summary>
	/// Deserialize danh sách PerkReplacementSkills từ save data.
	/// Tạo SkillController cho mỗi serialized skill và thêm vào PerkReplacementSkills.
	/// </summary>
	/// <param name="replacementSkills">Danh sách SerializedSkill từ save.</param>
	private void DeserializePerkReplacementSkills(List<SerializedSkill> replacementSkills)
	{
		if (replacementSkills == null || replacementSkills.Count == 0)
		{
			return;
		}
		foreach (SerializedSkill replacementSkill in replacementSkills)
		{
			PerkReplacementSkills.Add(new SkillController(replacementSkill, this).Skill);
		}
	}

	/// <summary>
	/// Deserialize danh sách Skills từ save data.
	/// Duyệt qua SkillsByLevel[Level] để tìm skill cần tạo:
	/// - Nếu có serialized data → khôi phục trạng thái (OverallUses, UsesPerTurn...).
	/// - Nếu không → tạo mới từ SkillDefinition.
	/// </summary>
	/// <param name="skills">Danh sách SerializedSkill từ save.</param>
	private void DeserializeSkills(List<SerializedSkill> skills)
	{
		if (ItemDefinition.SkillsByLevel[Level] == null)
		{
			return;
		}
		foreach (KeyValuePair<string, int> skillByLevel in ItemDefinition.SkillsByLevel[Level])
		{
			if (SkillDatabase.SkillDefinitions.TryGetValue(skillByLevel.Key, out var value))
			{
				// Tìm serialized data cho skill này
				if (skills.TryFind((SerializedSkill s) => s.Id == skillByLevel.Key, out var value2))
				{
					// Có save data → khôi phục trạng thái
					Skills.Add(new SkillController(value2, this).Skill);
					continue;
				}
				// Không có save data → tạo mới
				TheLastStand.Model.Skill.Skill skill = new SkillController(value, this, skillByLevel.Value).Skill;
				skill.SetUsesPerTurnRemaining(value.UsesPerTurnCount);
				Skills.Add(skill);
			}
		}
	}

	#endregion Private Methods
}
