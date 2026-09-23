using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

/// <summary>
/// Bản thiết kế (blueprint) đầy đủ cho 1 loại vật phẩm trong game.
/// Đây là file Definition LỚN NHẤT trong hệ thống item — chứa TẤT CẢ dữ liệu cấu hình
/// bao gồm: category, hands, resistance, stats/damage/price/skills/perks theo từng level.
/// 
/// Dữ liệu được tổ chức theo Level (LevelVariations):
/// - Mỗi level có damage, price, stat bonuses, skills, perks riêng.
/// - Nếu level mới không định nghĩa giá trị → kế thừa từ level trước đó.
/// 
/// Ví dụ XML:
/// <code>
/// &lt;Item Id="IronSword"&gt;
///   &lt;Category&gt;MeleeWeapon&lt;/Category&gt;
///   &lt;Hands&gt;OneHand&lt;/Hands&gt;
///   &lt;Resistance Min="3" Max="5"/&gt;
///   &lt;LevelVariations&gt;
///     &lt;Level Id="0"&gt;
///       &lt;BaseDamage Min="3" Max="5"/&gt;
///       &lt;BasePrice&gt;10&lt;/BasePrice&gt;
///       &lt;BaseStatBonuses&gt;
///         &lt;BaseStatBonus Stat="PhysicalDamage"&gt;2&lt;/BaseStatBonus&gt;
///       &lt;/BaseStatBonuses&gt;
///       &lt;Skills&gt;
///         &lt;Skill OverallUsesCount="3"&gt;Slash&lt;/Skill&gt;
///       &lt;/Skills&gt;
///       &lt;Perks&gt;&lt;Perk&gt;SwordMastery&lt;/Perk&gt;&lt;/Perks&gt;
///     &lt;/Level&gt;
///   &lt;/LevelVariations&gt;
/// &lt;/Item&gt;
/// </code>
/// </summary>
public class ItemDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>
	/// Phân loại vật phẩm (flags enum — có thể kết hợp bằng bitwise OR).
	/// 
	/// Cấu trúc phân cấp:
	/// - Weapon = MeleeWeapon | RangeWeapon | MagicWeapon
	/// - BodyArmor = Cloth | Light | Medium | Heavy BodyArmor
	/// - Helm = Cloth | Light | Medium | Heavy Helm
	/// - Boots = Cloth | Light | Medium | Heavy Boots
	/// - Armor = BodyArmor | Helm | Boots | Trinket
	/// - Equipment = Weapon | Armor | Utility
	/// - Usable = Potion | Scroll
	/// </summary>
	[Flags]
	public enum E_Category
	{
		None = 0,
		MeleeWeapon = 1,
		RangeWeapon = 2,
		MagicWeapon = 4,
		Shield = 8,
		ClothBodyArmor = 0x10,
		LightBodyArmor = 0x20,
		MediumBodyArmor = 0x40,
		HeavyBodyArmor = 0x80,
		ClothHelm = 0x100,
		LightHelm = 0x200,
		MediumHelm = 0x400,
		HeavyHelm = 0x800,
		ClothBoots = 0x1000,
		LightBoots = 0x2000,
		MediumBoots = 0x4000,
		HeavyBoots = 0x8000,
		Trinket = 0x10000,
		Utility = 0x20000,
		Potion = 0x40000,
		Scroll = 0x80000,
		/// <summary>Kết hợp: Potion | Scroll — vật phẩm tiêu hao.</summary>
		Usable = 0xC0000,
		/// <summary>Kết hợp: MeleeWeapon | RangeWeapon | MagicWeapon.</summary>
		Weapon = 7,
		/// <summary>Kết hợp: tất cả Helm variants.</summary>
		Helm = 0xF00,
		/// <summary>Kết hợp: tất cả Boots variants.</summary>
		Boots = 0xF000,
		/// <summary>Kết hợp: tất cả BodyArmor variants.</summary>
		BodyArmor = 0xF0,
		/// <summary>Kết hợp: BodyArmor | Helm | Boots | Trinket.</summary>
		Armor = 0xFFF0,
		/// <summary>Kết hợp: Weapon | Armor | Utility.</summary>
		Equipment = 0x1FFF8,
		/// <summary>Kết hợp: Utility | Shield — trang bị tay phụ.</summary>
		OffHand = 0x20008,
		/// <summary>Tất cả categories.</summary>
		All = 0xFFFFF
	}

	/// <summary>Kiểu cầm vật phẩm.</summary>
	public enum E_Hands
	{
		/// <summary>Không cầm (armor, trinket...).</summary>
		None,
		/// <summary>Cầm 1 tay — có thể dual wield hoặc cầm kèm shield.</summary>
		OneHand,
		/// <summary>Cầm 2 tay — chiếm cả 2 slot tay.</summary>
		TwoHands,
		/// <summary>Trang bị tay phụ (shield, utility).</summary>
		OffHand
	}

	/// <summary>Độ hiếm vật phẩm — ảnh hưởng số lượng Affix bonus.</summary>
	public enum E_Rarity
	{
		/// <summary>Chưa xác định (dùng cho random).</summary>
		None,
		/// <summary>Thường — 0 Affix.</summary>
		Common,
		/// <summary>Ma thuật — 1 Affix.</summary>
		Magic,
		/// <summary>Hiếm — 2 Affix.</summary>
		Rare,
		/// <summary>Sử thi — 3 Affix (1 trong đó là Epic).</summary>
		Epic
	}

	/// <summary>
	/// Custom comparer cho E_Category — tối ưu performance khi dùng làm Dictionary key.
	/// Tránh boxing enum (default EqualityComparer gây allocation).
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CategoryComparer : IEqualityComparer<E_Category>
	{
		public bool Equals(E_Category x, E_Category y)
		{
			return x == y;
		}

		public int GetHashCode(E_Category obj)
		{
			return (int)obj;
		}
	}

	/// <summary>Custom comparer cho E_Rarity — tương tự CategoryComparer.</summary>
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct RarityComparer : IEqualityComparer<E_Rarity>
	{
		public bool Equals(E_Rarity x, E_Rarity y)
		{
			return x == y;
		}

		public int GetHashCode(E_Rarity obj)
		{
			return (int)obj;
		}
	}

	/// <summary>Shared instances để tránh tạo comparer mới mỗi lần.</summary>
	public static readonly CategoryComparer SharedCategoryComparer;
	public static readonly RarityComparer SharedRarityComparer;

	/// <summary>ID art riêng (nếu khác Id chính). Dùng để load sprite/animation.</summary>
	private string artId = string.Empty;

	/// <summary>Danh sách level đã được định nghĩa (dùng cho GetHigher/LowerExistingLevel).</summary>
	private List<int> definedLevels;

	/// <summary>Tags phân loại bổ sung. Ví dụ: "starter", "legendary", "dlc_weapon".</summary>
	public HashSet<string> Tags = new HashSet<string>();

	/// <summary>
	/// ID art để load asset. Nếu không set riêng → dùng Id chính.
	/// Cho phép nhiều item dùng chung visual (ví dụ: IronSword_v2 dùng art của IronSword).
	/// </summary>
	public string ArtId
	{
		get
		{
			if (!(artId != string.Empty))
			{
				return Id;
			}
			return artId;
		}
	}

	/// <summary>
	/// Sát thương cơ bản theo level. Key = level, Value = Vector2(minDmg, maxDmg).
	/// Ví dụ: { 0: (3,5), 1: (4,7), 2: (5,9) }
	/// </summary>
	public Dictionary<int, Vector2> BaseDamageByLevel { get; } = new Dictionary<int, Vector2>();

	/// <summary>Tên hiển thị vật phẩm (localized). Tra bảng "ItemName_{Id}".</summary>
	public string BaseName => Localizer.Get("ItemName_" + Id);

	/// <summary>
	/// Giá bán cơ bản theo level. Key = level, Value = giá.
	/// Ví dụ: { 0: 10, 1: 15, 2: 22 }
	/// </summary>
	public Dictionary<int, float> BasePriceByLevel { get; } = new Dictionary<int, float>();

	/// <summary>
	/// Stat bonuses cơ bản theo level. 
	/// Key ngoài = level, Key trong = E_Stat, Value = bonus value.
	/// Ví dụ: Level 0 → { PhysicalDamage: 2, CritChance: 5 }
	/// </summary>
	public Dictionary<int, Dictionary<UnitStatDefinition.E_Stat, float>> BaseStatBonusesByLevel { get; } = new Dictionary<int, Dictionary<UnitStatDefinition.E_Stat, float>>();

	/// <summary>Định nghĩa body parts cho visual (sprite, animation). Null nếu không có.</summary>
	public Dictionary<string, BodyPartDefinition> BodyPartsDefinitions { get; private set; }

	/// <summary>Phân loại vật phẩm. Ví dụ: MeleeWeapon, Shield, Potion.</summary>
	public E_Category Category { get; private set; }

	/// <summary>Tên category đã localize. Ví dụ: "Vũ khí cận chiến".</summary>
	public string CategoryName => Category.GetLocalizedName();

	/// <summary>Kiểu cầm. Ví dụ: OneHand, TwoHands, OffHand.</summary>
	public E_Hands Hands { get; private set; }

	/// <summary>Tên kiểu cầm đã localize. Ví dụ: "Một tay".</summary>
	public string HandsName => Localizer.Get(string.Format("{0}{1}", "HandsName_", Hands));

	/// <summary>ID duy nhất. Ví dụ: "IronSword", "HealthPotion".</summary>
	public string Id { get; private set; }

	/// <summary>
	/// Kiểm tra có phải vũ khí không (Melee | Range | Magic).
	/// Shield KHÔNG phải weapon.
	/// </summary>
	public bool IsWeapon
	{
		get
		{
			if (Category != E_Category.MagicWeapon && Category != E_Category.MeleeWeapon)
			{
				return Category == E_Category.RangeWeapon;
			}
			return true;
		}
	}

	/// <summary>Kiểm tra có phải item cầm tay không (OneHand, TwoHands, OffHand).</summary>
	public bool IsHandItem => Hands != E_Hands.None;

	/// <summary>Kiểm tra có phải trang bị tay phụ không.</summary>
	public bool IsOffHand => Hands == E_Hands.OffHand;

	/// <summary>Kiểm tra có phải trang bị tay chính không (OneHand hoặc TwoHands).</summary>
	public bool IsMainHand
	{
		get
		{
			if (Hands != E_Hands.OneHand)
			{
				return Hands == E_Hands.TwoHands;
			}
			return true;
		}
	}

	/// <summary>
	/// Stat bonus NỔI BẬT (highlight) theo level — stat chính của item.
	/// Hiển thị lớn hơn các stat khác trong tooltip.
	/// Ví dụ: Level 0 → (PhysicalDamage, 5.0)
	/// </summary>
	public Dictionary<int, Tuple<UnitStatDefinition.E_Stat, float>> MainStatBonusByLevel { get; } = new Dictionary<int, Tuple<UnitStatDefinition.E_Stat, float>>();

	/// <summary>
	/// Độ bền vật phẩm. Vector2Int(min, max).
	/// Khi tạo item, random giá trị Resistance trong khoảng [min, max].
	/// </summary>
	public Vector2Int Resistance { get; private set; }

	/// <summary>
	/// Danh sách Perk IDs theo level. Null = không có perk ở level đó.
	/// Perk là passive ability đi kèm item.
	/// </summary>
	public Dictionary<int, HashSet<string>> PerksByLevel { get; } = new Dictionary<int, HashSet<string>>();

	/// <summary>
	/// Danh sách Skill theo level. Key ngoài = level, Key trong = skillId, Value = overallUsesCount.
	/// overallUsesCount = -1 → vô hạn lần dùng.
	/// Null = không có skill ở level đó.
	/// </summary>
	public Dictionary<int, Dictionary<string, int>> SkillsByLevel { get; } = new Dictionary<int, Dictionary<string, int>>();

	public ItemDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Đọc toàn bộ ItemDefinition từ XML.
	/// 
	/// Thứ tự đọc:
	/// 1. Id
	/// 2. Category (MeleeWeapon, Potion...)
	/// 3. Tags (optional)
	/// 4. Hands (optional - OneHand, TwoHands...)
	/// 5. Resistance (optional - min/max)
	/// 6. LevelVariations — cho mỗi level:
	///    - BaseDamage (min/max)
	///    - BasePrice
	///    - BaseStatBonuses
	///    - MainStatBonus
	///    - Skills
	///    - Perks
	/// 
	/// Nếu level mới không định nghĩa giá trị → KẾ THỪA từ level trước đó.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		// 1. Đọc Id
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			CLoggerManager.Log("An item hasn't an Id !", LogType.Error);
			return;
		}
		Id = xAttribute.Value;
		artId = Id;
		// 2. Đọc Category
		XElement xElement2 = xElement.Element("Category");
		if (xElement2 != null)
		{
			if (Enum.TryParse<E_Category>(xElement2.Value, out var result))
			{
				Category = result;
				// 3. Đọc Tags (optional)
				XElement xElement3 = xElement.Element("Tags");
				if (xElement3 != null)
				{
					foreach (XElement item in xElement3.Elements("Tag"))
					{
						string value = item.Value;
						// Đăng ký tag vào ItemDatabase.ItemsByTag (tra cứu ngược)
						if (ItemDatabase.ItemsByTag.ContainsKey(value))
						{
							ItemDatabase.ItemsByTag[value].Add(Id);
						}
						else
						{
							ItemDatabase.ItemsByTag.Add(value, new List<string> { Id });
						}
						if (!Tags.Contains(value))
						{
							Tags.Add(value);
						}
					}
				}
				// 4. Đọc Hands (optional)
				XElement xElement4 = xElement.Element("Hands");
				if (xElement4 != null)
				{
					if (!Enum.TryParse<E_Hands>(xElement4.Value, out var result2))
					{
						CLoggerManager.Log("Item " + Id + "'s Hands " + HasAnInvalid("E_Hands", xElement4.Value), LogType.Error);
						return;
					}
					Hands = result2;
				}
				// 5. Đọc Resistance (optional)
				XElement xElement5 = xElement.Element(UnitStatDefinition.E_Stat.Resistance.ToString());
				if (xElement5 != null)
				{
					Resistance = xElement5.ParseMinMax();
				}
				// 6. Đọc LevelVariations — dữ liệu theo từng level
				XElement xElement6 = xElement.Element("LevelVariations");
				// Giá trị "mặc định" kế thừa từ level trước
				Vector2 value2 = Vector2.zero;
				float value3 = -1f;
				Dictionary<UnitStatDefinition.E_Stat, float> value4 = null;
				Dictionary<string, int> value5 = null;
				HashSet<string> value6 = null;
				definedLevels = new List<int>();
				{
					foreach (XElement item2 in xElement6.Elements("Level"))
					{
						// Đọc level Id
						XAttribute xAttribute2 = item2.Attribute("Id");
						if (!int.TryParse(xAttribute2.Value, out var result3))
						{
							CLoggerManager.Log("Item " + Id + "'s Level " + HasAnInvalidInt(xAttribute2.Value), LogType.Error);
							continue;
						}
						definedLevels.Add(result3);
						// 6a. BaseDamage — nếu không định nghĩa → kế thừa value2
						XElement xElement7 = item2.Element("BaseDamage");
						if (xElement7 != null)
						{
							XAttribute xAttribute3 = xElement7.Attribute("Min");
							if (xAttribute3.IsNullOrEmpty())
							{
								CLoggerManager.Log("The BaseDamage " + OfTheItem(Id, result3) + " hasn't a Min !", LogType.Error);
								break;
							}
							if (!int.TryParse(xAttribute3.Value, out var result4))
							{
								CLoggerManager.Log($"Item {Id}(Level : {result3})'s BaseDamage Min {HasAnInvalidInt(xAttribute3.Value)}", LogType.Error);
								break;
							}
							XAttribute xAttribute4 = xElement7.Attribute("Max");
							if (xAttribute4.IsNullOrEmpty())
							{
								CLoggerManager.Log("The BaseDamage " + OfTheItem(Id, result3) + " hasn't a Max !", LogType.Error);
								break;
							}
							if (!int.TryParse(xAttribute4.Value, out var result5))
							{
								CLoggerManager.Log($"Item {Id}(Level : {result3})'s BaseDamage Max {HasAnInvalidInt(xAttribute4.Value)}", LogType.Error);
								break;
							}
							Vector2 vector = new Vector2(result4, result5);
							value2 = vector;
							BaseDamageByLevel.Add(result3, vector);
						}
						else
						{
							BaseDamageByLevel.Add(result3, value2);
						}
						// 6b. BasePrice — nếu không định nghĩa → kế thừa value3
						XElement xElement8 = item2.Element("BasePrice");
						if (xElement8 != null)
						{
							if (!float.TryParse(xElement8.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result6))
							{
								CLoggerManager.Log($"Item {Id}(Level : {result3})'s Price {HasAnInvalidFloat(xElement8.Value)}", LogType.Error);
								break;
							}
							value3 = result6;
							BasePriceByLevel.Add(result3, result6);
						}
						else
						{
							BasePriceByLevel.Add(result3, value3);
						}
						// 6c. BaseStatBonuses — nếu không định nghĩa → kế thừa value4
						BaseStatBonusesByLevel.Add(result3, new Dictionary<UnitStatDefinition.E_Stat, float>());
						XElement xElement9 = item2.Element("BaseStatBonuses");
						if (xElement9 != null)
						{
							foreach (XElement item3 in xElement9.Elements("BaseStatBonus"))
							{
								XAttribute xAttribute5 = item3.Attribute("Stat");
								UnitStatDefinition.E_Stat result7;
								float result8;
								if (xAttribute5.IsNullOrEmpty())
								{
									CLoggerManager.Log("A BaseStatBonus " + OfTheItem(Id, result3) + " hasn't a Stat!", LogType.Error);
								}
								else if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute5.Value, out result7))
								{
									CLoggerManager.Log("A BaseStatBonus " + OfTheItem(Id, result3) + " " + HasAnInvalidStat(xAttribute5.Value), LogType.Error);
								}
								else if (item3.IsNullOrEmpty())
								{
									CLoggerManager.Log("A BaseStatBonus (" + result7.ToString() + ") " + OfTheItem(Id, result3) + " is empty !", LogType.Error);
								}
								else if (!float.TryParse(item3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result8))
								{
									CLoggerManager.Log("A BaseStatBonus (" + result7.ToString() + ") " + OfTheItem(Id, result3) + " " + HasAnInvalidFloat(item3.Value), LogType.Error);
								}
								else
								{
									BaseStatBonusesByLevel[result3].Add(result7, result8);
								}
							}
							value4 = BaseStatBonusesByLevel[result3];
						}
						else
						{
							BaseStatBonusesByLevel[result3] = value4;
						}
						// 6d. MainStatBonus — stat nổi bật highlight trong tooltip
						XElement xElement10 = item2.Element("MainStatBonus");
						if (!xElement10.IsNullOrEmpty())
						{
							XAttribute xAttribute6 = xElement10.Attribute("Stat");
							if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute6.Value, out var result9))
							{
								CLoggerManager.Log("The MainStatBonus " + OfTheItem(Id, result3) + " " + HasAnInvalidStat(xAttribute6.Value), LogType.Error);
							}
							if (!float.TryParse(xElement10.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result10))
							{
								CLoggerManager.Log("The MainStatBonus " + OfTheItem(Id, result3) + " " + HasAnInvalidFloat(xElement10.Value), LogType.Error);
								break;
							}
							MainStatBonusByLevel.Add(result3, new Tuple<UnitStatDefinition.E_Stat, float>(result9, result10));
						}
						else
						{
							MainStatBonusByLevel.Add(result3, null);
						}
						// 6e. Skills — danh sách skill + overall uses count
						SkillsByLevel.Add(result3, null);
						XElement xElement11 = item2.Element("Skills");
						if (xElement11 != null)
						{
							SkillsByLevel[result3] = new Dictionary<string, int>();
							foreach (XElement item4 in xElement11.Elements())
							{
								if (item4.IsNullOrEmpty())
								{
									CLoggerManager.Log("A skill " + OfTheItem(Id, result3) + " is Empty !", LogType.Error);
									continue;
								}
								int result11 = -1;
								XAttribute xAttribute7 = item4.Attribute("OverallUsesCount");
								if (xAttribute7 != null && !int.TryParse(xAttribute7.Value, out result11))
								{
									CLoggerManager.Log("The skill " + item4.Value + " " + OfTheItem(Id, result3) + " " + HasAnInvalidInt(xAttribute7.Value), LogType.Error);
								}
								else
								{
									SkillsByLevel[result3].Add(item4.Value, result11);
								}
							}
							value5 = SkillsByLevel[result3];
						}
						else
						{
							SkillsByLevel[result3] = value5;
						}
						// 6f. Perks — danh sách perk IDs
						PerksByLevel.Add(result3, null);
						XElement xElement12 = item2.Element("Perks");
						if (xElement12 != null)
						{
							PerksByLevel[result3] = new HashSet<string>();
							foreach (XElement item5 in xElement12.Elements())
							{
								if (item5.IsNullOrEmpty())
								{
									CLoggerManager.Log("A Perk " + OfTheItem(Id, result3) + " is Empty !", LogType.Error);
								}
								else if (!PerksByLevel[result3].Contains(item5.Value))
								{
									PerksByLevel[result3].Add(item5.Value);
								}
							}
							value6 = PerksByLevel[result3];
						}
						else
						{
							PerksByLevel[result3] = value6;
						}
					}
					return;
				}
			}
			CLoggerManager.Log("Item " + Id + "'s Category " + HasAnInvalid("E_Category", xElement2.Value), LogType.Error);
		}
		else
		{
			CLoggerManager.Log("Item " + Id + " must have a Category", LogType.Error);
		}
	}

	/// <summary>
	/// Đọc dữ liệu liên quan đến art/visual (ArtId, BodyParts).
	/// Được gọi riêng sau Deserialize() vì art data nằm trong file XML khác.
	/// </summary>
	public void DeserializeArtRelatedDatas(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("ArtId");
		if (!xElement.IsNullOrEmpty())
		{
			artId = xElement.Value;
		}
		else
		{
			artId = Id;
		}
		XElement xElement2 = obj.Element("BodyParts");
		if (xElement2 == null)
		{
			return;
		}
		BodyPartsDefinitions = new Dictionary<string, BodyPartDefinition>();
		foreach (XElement item in xElement2.Elements("BodyPartDefinition"))
		{
			BodyPartDefinition bodyPartDefinition = new BodyPartDefinition(item);
			BodyPartsDefinitions.Add(bodyPartDefinition.Id, bodyPartDefinition);
		}
	}

	/// <summary>
	/// Tìm level cao nhất ≤ giá trị khởi tạo mà có dữ liệu.
	/// 
	/// Ví dụ: definedLevels = [0, 2, 5]
	/// - GetHigherExistingLevelFromInitValue(3) → 2 (level 3 không có, lùi về 2)
	/// - GetHigherExistingLevelFromInitValue(5) → 5
	/// - GetHigherExistingLevelFromInitValue(0) → 0
	/// - GetHigherExistingLevelFromInitValue(-1) → -1 (không tìm thấy)
	/// </summary>
	/// <param name="level">Level khởi tạo.</param>
	/// <returns>Level tồn tại gần nhất (≤ level), hoặc -1 nếu không tìm thấy.</returns>
	public int GetHigherExistingLevelFromInitValue(int level)
	{
		while (level > -1)
		{
			if (definedLevels.Contains(level))
			{
				return level;
			}
			level--;
		}
		return -1;
	}

	/// <summary>
	/// Tìm level thấp nhất ≥ giá trị khởi tạo mà có dữ liệu.
	/// 
	/// Ví dụ: definedLevels = [0, 2, 5]
	/// - GetLowerExistingLevelFromInitValue(1) → 2 (level 1 không có, tiến lên 2)
	/// - GetLowerExistingLevelFromInitValue(0) → 0
	/// - GetLowerExistingLevelFromInitValue(6) → -1 (không tìm thấy)
	/// </summary>
	/// <param name="level">Level khởi tạo.</param>
	/// <returns>Level tồn tại gần nhất (≥ level), hoặc -1 nếu không tìm thấy.</returns>
	public int GetLowerExistingLevelFromInitValue(int level)
	{
		while (level < 999)
		{
			if (definedLevels.Contains(level))
			{
				return level;
			}
			level++;
		}
		return -1;
	}

	/// <summary>
	/// Kiểm tra item có tag cụ thể không.
	/// Tra cứu qua ItemDatabase.ItemsByTag (tra cứu ngược: tag → list item IDs).
	/// </summary>
	/// <param name="tag">Tag cần kiểm tra. Ví dụ: "starter", "legendary".</param>
	/// <returns>True nếu item có tag này.</returns>
	public bool HasTag(string tag)
	{
		if (ItemDatabase.ItemsByTag.TryGetValue(tag, out var value))
		{
			return value.Contains(Id);
		}
		return false;
	}
}
