using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Skill;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Serialization.Item;

namespace TheLastStand.Controller.Item;

/// <summary>
/// Controller chính quản lý một vật phẩm (Item) trong game.
/// Xử lý toàn bộ logic liên quan đến vật phẩm:
/// - Khởi tạo vật phẩm mới hoặc từ save data (với Definition, level, rarity, affixes)
/// - Merge/tổng hợp các Affix bonus và Affix malus thành stat modifiers cuối cùng
/// - Quản lý Perk Replacement Skills (kỹ năng thay thế do Perk cung cấp)
/// - Nạp lại lượt sử dụng kỹ năng khi bắt đầu lượt mới (StartTurn/RefillOverallUses)
/// - Khởi tạo dữ liệu bổ sung: Skills từ SkillsOverallUses, Perks theo level
/// 
/// Mỗi Item có 1 ItemController duy nhất, được tạo cùng lúc với Model Item.
/// </summary>
public class ItemController
{
	#region Properties

	/// <summary>
	/// Model Item mà controller này quản lý.
	/// Chứa: ItemDefinition, Level, Rarity, Affixes, AffixesMalus, Skills, Perks, Resistance.
	/// </summary>
	public TheLastStand.Model.Item.Item Item { get; }

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor khởi tạo Item từ dữ liệu save (deserialization).
	/// Phục hồi toàn bộ trạng thái vật phẩm từ file save.
	/// </summary>
	/// <param name="container">Dữ liệu serialized của Item.</param>
	/// <param name="itemSlot">Ô slot đang chứa item (có thể null).</param>
	public ItemController(SerializedItem container, ItemSlot itemSlot)
	{
		Item = new TheLastStand.Model.Item.Item(container, this, itemSlot);
	}

	/// <summary>
	/// Constructor tạo Item mới từ ItemDefinition (khi sinh vật phẩm - loot/shop/reward).
	/// Tính random Resistance trong khoảng [min, max] từ definition.
	/// Sau đó gọi InitItemAdditionalData() để tạo Skills và Perks theo level.
	/// </summary>
	/// <param name="itemDefinition">Định nghĩa vật phẩm.</param>
	/// <param name="level">Cấp độ vật phẩm (ảnh hưởng stats, skills, perks).</param>
	/// <param name="rarity">Độ hiếm (Common, Uncommon, Rare, Epic, Legendary).</param>
	public ItemController(ItemDefinition itemDefinition, int level, ItemDefinition.E_Rarity rarity)
	{
		Item = new TheLastStand.Model.Item.Item(itemDefinition, this)
		{
			Level = level,
			Rarity = rarity,
			// Random Resistance trong khoảng [min, max]. Nếu min == max thì dùng giá trị cố định.
			Resistance = ((itemDefinition.Resistance.x == itemDefinition.Resistance.y) ? itemDefinition.Resistance.x : RandomManager.GetRandomRange(this, itemDefinition.Resistance.x, itemDefinition.Resistance.y + 1))
		};
		InitItemAdditionalData();
	}

	/// <summary>
	/// Constructor copy - tạo bản sao (clone) của một Item đã có.
	/// Sao chép: Level, Rarity, Affixes, AffixesMalus, Resistance.
	/// Dùng khi cần duplicate item (ví dụ: preview trong UI).
	/// </summary>
	/// <param name="itemToCopy">Item nguồn cần sao chép.</param>
	public ItemController(TheLastStand.Model.Item.Item itemToCopy)
	{
		Item = new TheLastStand.Model.Item.Item(itemToCopy.ItemDefinition, this)
		{
			Level = itemToCopy.Level,
			Rarity = itemToCopy.Rarity,
			AdditionalAffixes = itemToCopy.AdditionalAffixes,
			AdditionalAffixesMalus = itemToCopy.AdditionalAffixesMalus,
			Resistance = itemToCopy.Resistance
		};
		InitItemAdditionalData();
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Gộp (merge) danh sách Affix thành Dictionary stat → tổng giá trị modifier.
	/// Cùng loại stat sẽ được cộng dồn. 
	/// Ví dụ: 2 affix "+3 PhysicalDamage" và "+2 PhysicalDamage" → PhysicalDamage: 5.
	/// </summary>
	/// <param name="affixes">Danh sách Affix cần gộp.</param>
	/// <returns>Dictionary: E_Stat → tổng giá trị modifier.</returns>
	public Dictionary<UnitStatDefinition.E_Stat, float> MergeAffixes(IEnumerable<IAffix> affixes)
	{
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary = new Dictionary<UnitStatDefinition.E_Stat, float>();
		foreach (IAffix affix in affixes)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> finalStatModifier in affix.GetFinalStatModifiers())
			{
				if (dictionary.ContainsKey(finalStatModifier.Key))
				{
					dictionary[finalStatModifier.Key] += finalStatModifier.Value;
				}
				else
				{
					dictionary.Add(finalStatModifier.Key, finalStatModifier.Value);
				}
			}
		}
		return dictionary;
	}

	/// <summary>
	/// Gộp TẤT CẢ Affix (bonus + malus) thành stat modifiers cuối cùng.
	/// Affix bonus được cộng, AffixMalus được TRỪ.
	/// Kết quả: giá trị ròng (net) cho mỗi stat.
	/// Ví dụ: Affix +5 PhysicalDamage, Malus -2 PhysicalDamage → PhysicalDamage: 3.
	/// </summary>
	/// <returns>Dictionary: E_Stat → giá trị ròng sau khi tính cả bonus và malus.</returns>
	public Dictionary<UnitStatDefinition.E_Stat, float> MergeAllAffixes()
	{
		// Bước 1: Merge tất cả Affix bonus
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary = MergeAffixes(Item.AdditionalAffixes);
		// Bước 2: Trừ đi tất cả AffixMalus
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item in MergeAffixes(Item.AdditionalAffixesMalus))
		{
			if (dictionary.ContainsKey(item.Key))
			{
				dictionary[item.Key] -= item.Value;
			}
			else
			{
				dictionary.Add(item.Key, 0f - item.Value);
			}
		}
		return dictionary;
	}

	/// <summary>
	/// Thêm kỹ năng thay thế (Replacement Skill) do Perk cung cấp.
	/// Perk có thể thay thế kỹ năng gốc của item bằng phiên bản nâng cao.
	/// Chỉ thêm nếu chưa có kỹ năng cùng Id trong danh sách.
	/// </summary>
	/// <param name="replacementSkill">Kỹ năng thay thế cần thêm.</param>
	public void PerkAddReplacementSkill(TheLastStand.Model.Skill.Skill replacementSkill)
	{
		if (Item.PerkReplacementSkills.All((TheLastStand.Model.Skill.Skill itemSkill) => itemSkill.Id != replacementSkill.Id))
		{
			Item.PerkReplacementSkills.Add(replacementSkill);
		}
	}

	/// <summary>
	/// Xóa tất cả kỹ năng thay thế do Perk cung cấp.
	/// Được gọi khi gỡ item khỏi slot hoặc khi Perk bị mất.
	/// </summary>
	public void PerkClearAllReplacementSkills()
	{
		Item.PerkReplacementSkills.Clear();
	}

	/// <summary>
	/// Nạp lại (refill) số lần sử dụng tổng thể (OverallUses) cho tất cả kỹ năng của item.
	/// Được gọi vào đầu pha Production (ban ngày). Chỉ nạp lại cho kỹ năng có giới hạn sử dụng (!= -1).
	/// Cũng xử lý PerkReplacementSkills.
	/// </summary>
	public void RefillOverallUses()
	{
		// Nạp lại OverallUses cho các kỹ năng gốc
		for (int num = Item.Skills.Count - 1; num >= 0; num--)
		{
			if (Item.Skills[num].OverallUsesRemaining != -1)
			{
				Item.Skills[num].OverallUsesRemaining = Item.Skills[num].ComputeTotalUses(Item.Holder);
			}
		}
		if (Item.PerkReplacementSkills.Count <= 0)
		{
			return;
		}
		// Nạp lại cho PerkReplacementSkills
		for (int num2 = Item.PerkReplacementSkills.Count - 1; num2 >= 0; num2--)
		{
			if (Item.PerkReplacementSkills[num2].OverallUsesRemaining != -1)
			{
				Item.PerkReplacementSkills[num2].OverallUsesRemaining = Item.PerkReplacementSkills[num2].ComputeTotalUses(Item.Holder);
			}
		}
	}

	/// <summary>
	/// Xử lý logic đầu lượt cho item.
	/// 1. Nếu đang ở pha Production → nạp lại OverallUses (số lần dùng tổng thể).
	/// 2. Reset UsesPerTurnRemaining cho tất cả kỹ năng có giới hạn lượt.
	/// 3. Xử lý tương tự cho PerkReplacementSkills (trừ skill liên kết - IsLinkedWithAnotherSkillForUses).
	/// </summary>
	public void StartTurn()
	{
		// Nạp lại OverallUses vào đầu pha Production
		if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			RefillOverallUses();
		}
		// Reset UsesPerTurn cho kỹ năng gốc
		for (int num = Item.Skills.Count - 1; num >= 0; num--)
		{
			if (Item.Skills[num].SkillDefinition.UsesPerTurnCount != -1)
			{
				Item.Skills[num].SetUsesPerTurnRemaining(Item.Skills[num].UsesPerTurn);
			}
		}
		if (Item.PerkReplacementSkills.Count <= 0)
		{
			return;
		}
		// Reset UsesPerTurn cho PerkReplacementSkills (trừ kỹ năng liên kết)
		for (int num2 = Item.PerkReplacementSkills.Count - 1; num2 >= 0; num2--)
		{
			if (!Item.PerkReplacementSkills[num2].IsLinkedWithAnotherSkillForUses && Item.PerkReplacementSkills[num2].SkillDefinition.UsesPerTurnCount != -1)
			{
				Item.PerkReplacementSkills[num2].SetUsesPerTurnRemaining(Item.PerkReplacementSkills[num2].UsesPerTurn);
			}
		}
	}

	#endregion Public Methods

	#region Private Methods

	/// <summary>
	/// Khởi tạo dữ liệu bổ sung cho item sau khi tạo:
	/// 1. Tạo Skill instances từ SkillsOverallUses (danh sách skillId → totalUses).
	/// 2. Tạo Perk instances từ PerksByLevel theo level hiện tại của item.
	/// Được gọi trong constructor (trừ constructor từ save data).
	/// </summary>
	private void InitItemAdditionalData()
	{
		// Tạo Skills từ SkillsOverallUses dictionary
		if (Item.SkillsOverallUses != null)
		{
			foreach (KeyValuePair<string, int> skillsOverallUse in Item.SkillsOverallUses)
			{
				if (SkillDatabase.SkillDefinitions.ContainsKey(skillsOverallUse.Key))
				{
					Item.Skills.Add(new SkillController(SkillDatabase.SkillDefinitions[skillsOverallUse.Key], Item, skillsOverallUse.Value, SkillDatabase.SkillDefinitions[skillsOverallUse.Key].UsesPerTurnCount).Skill);
				}
			}
		}
		// Tạo Perks theo level - nếu level hiện tại có danh sách perk
		if (Item.ItemDefinition.PerksByLevel[Item.Level] == null)
		{
			return;
		}
		foreach (string item in Item.ItemDefinition.PerksByLevel[Item.Level])
		{
			if (PlayableUnitDatabase.PerkDefinitions.ContainsKey(item) && !Item.Perks.ContainsKey(item))
			{
				Item.Perks.Add(item, new PerkController(PlayableUnitDatabase.PerkDefinitions[item], null, null, null, string.Empty, isNative: false, isFromRace: false).Perk);
			}
			Item.PerksId.Add(item);
		}
	}

	#endregion Private Methods
}
