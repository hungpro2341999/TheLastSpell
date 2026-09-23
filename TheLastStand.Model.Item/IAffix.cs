using System.Collections.Generic;
using TheLastStand.Definition.Unit;

namespace TheLastStand.Model.Item;

/// <summary>
/// Interface chung cho cả Affix (bonus) và AffixMalus (phạt).
/// Cho phép ItemController.MergeAffixes() xử lý đồng nhất cả 2 loại
/// thông qua polymorphism - chỉ cần gọi GetFinalStatModifiers().
/// </summary>
public interface IAffix
{
	/// <summary>
	/// Lấy danh sách stat modifiers cuối cùng mà affix này cung cấp.
	/// Trả về Dictionary: E_Stat → giá trị modifier.
	/// Với Affix: bao gồm cả base + epic modifiers (nếu IsEpic).
	/// Với AffixMalus: trả về stat + giá trị phạt theo MalusLevel.
	/// </summary>
	Dictionary<UnitStatDefinition.E_Stat, float> GetFinalStatModifiers();
}

