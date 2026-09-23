using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

/// <summary>
/// Bản thiết kế (blueprint) cho 1 loại Affix bonus trên vật phẩm.
/// Affix là thuộc tính bonus ngẫu nhiên được thêm vào item khi sinh (ví dụ: "+3 PhysicalDamage").
/// 
/// Cấu trúc dữ liệu:
/// - 1 AffixDefinition có NHIỀU LeveledAffixDefinition (level 1-10).
/// - Mỗi level cho stat modifier khác nhau (level cao = bonus lớn hơn).
/// - Affix chỉ xuất hiện trên item có Category phù hợp và item level trong [LevelMin, LevelMax].
/// - EpicStatModifiers: bonus thêm khi affix được đánh dấu là Epic.
/// 
/// Ví dụ XML:
/// <code>
/// &lt;Affix Id="AffixPhysDmg" MaxOccurrences="2" Droppable="true"&gt;
///   &lt;ItemLevel Min="0" Max="10"/&gt;
///   &lt;ItemCategories&gt;
///     &lt;ItemCategory Weight="100"&gt;MeleeWeapon&lt;/ItemCategory&gt;
///     &lt;ItemCategory Weight="50"&gt;RangeWeapon&lt;/ItemCategory&gt;
///   &lt;/ItemCategories&gt;
///   &lt;Levels&gt;
///     &lt;Level Id="1"&gt;&lt;Modifier Stat="PhysicalDamage"&gt;2&lt;/Modifier&gt;&lt;/Level&gt;
///     &lt;Level Id="2"&gt;&lt;Modifier Stat="PhysicalDamage"&gt;4&lt;/Modifier&gt;&lt;/Level&gt;
///   &lt;/Levels&gt;
///   &lt;EpicBonus&gt;&lt;Modifier Stat="PhysicalDamage"&gt;3&lt;/Modifier&gt;&lt;/EpicBonus&gt;
/// &lt;/Affix&gt;
/// </code>
/// </summary>
public class AffixDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>
	/// Định nghĩa Affix theo level cụ thể.
	/// Mỗi level chứa danh sách stat modifiers riêng.
	/// Ví dụ: Level 1 → +2 PhysicalDamage, Level 3 → +6 PhysicalDamage.
	/// </summary>
	public class LeveledAffixDefinition : TheLastStand.Framework.Serialization.Definition
	{
		/// <summary>AffixDefinition cha chứa LeveledAffixDefinition này.</summary>
		public AffixDefinition AffixDefinition { get; private set; }

		/// <summary>Level của Affix (1-10). Level cao → stat modifier mạnh hơn.</summary>
		public int Level { get; private set; }

		/// <summary>
		/// Danh sách stat modifiers ở level này.
		/// Ví dụ: { PhysicalDamage: 4.0, CriticalHitChance: 2.0 }
		/// </summary>
		public Dictionary<UnitStatDefinition.E_Stat, float> StatModifiers { get; private set; } = new Dictionary<UnitStatDefinition.E_Stat, float>(UnitStatDefinition.SharedStatComparer);

		public LeveledAffixDefinition(AffixDefinition affixDefinition, XContainer container)
			: base(container)
		{
			AffixDefinition = affixDefinition;
		}

		/// <summary>
		/// Đọc level + danh sách Modifier từ XML.
		/// Validate: Level phải là int trong [1, 10].
		/// </summary>
		public override void Deserialize(XContainer container)
		{
			XElement xElement = container as XElement;
			XAttribute xAttribute = xElement.Attribute("Id");
			if (xAttribute.IsNullOrEmpty())
			{
				Debug.LogError("The Level has no Id!");
				return;
			}
			if (!int.TryParse(xAttribute.Value, out var result) || result < 1 || result > 10)
			{
				Debug.LogError("The Level (" + xAttribute.Value + ") is invalid!");
				return;
			}
			Level = result;
			// Đọc từng Modifier: Stat attribute → key, value → float
			foreach (XElement item in xElement.Elements("Modifier"))
			{
				if (item.IsNullOrEmpty())
				{
					Debug.LogError("The Modifier is empty!");
					continue;
				}
				XAttribute xAttribute2 = item.Attribute("Stat");
				if (xAttribute2.IsNullOrEmpty())
				{
					Debug.LogError("The Modifier has no Stat!");
				}
				else
				{
					StatModifiers.Add((UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), xAttribute2.Value), float.Parse(item.Value, NumberStyles.Float, CultureInfo.InvariantCulture));
				}
			}
		}
	}

	/// <summary>
	/// Affix có thể xuất hiện ngẫu nhiên trên item drop không.
	/// false = chỉ dùng cho item cố định (quest reward, debug...).
	/// </summary>
	public bool Droppable { get; private set; } = true;

	/// <summary>
	/// Stat bonus THÊM khi affix được đánh dấu là Epic.
	/// Cộng dồn lên trên StatModifiers thông thường.
	/// </summary>
	public Dictionary<UnitStatDefinition.E_Stat, float> EpicStatModifiers { get; private set; } = new Dictionary<UnitStatDefinition.E_Stat, float>(UnitStatDefinition.SharedStatComparer);

	/// <summary>ID duy nhất của Affix. Ví dụ: "AffixPhysDmg", "AffixCritChance".</summary>
	public string Id { get; private set; }

	/// <summary>
	/// Category item nào có thể nhận Affix này + weight (tỉ lệ) tương ứng.
	/// Ví dụ: { MeleeWeapon: 100, RangeWeapon: 50 } → MeleeWeapon có tỉ lệ gấp đôi.
	/// </summary>
	public Dictionary<ItemDefinition.E_Category, float> ItemCategoriesWithWeight { get; private set; } = new Dictionary<ItemDefinition.E_Category, float>(ItemDefinition.SharedCategoryComparer);

	/// <summary>
	/// Danh sách định nghĩa Affix theo level.
	/// Key = level (1-10), Value = LeveledAffixDefinition chứa stat modifiers.
	/// </summary>
	public Dictionary<int, LeveledAffixDefinition> LevelDefinitions { get; private set; } = new Dictionary<int, LeveledAffixDefinition>();

	/// <summary>Item level TỐI ĐA mà Affix này có thể xuất hiện.</summary>
	public int LevelMax { get; private set; }

	/// <summary>Item level TỐI THIỂU mà Affix này có thể xuất hiện.</summary>
	public int LevelMin { get; private set; }

	/// <summary>
	/// Số lần tối đa Affix này có thể xuất hiện trên 1 item.
	/// -1 = không giới hạn. Dùng để tránh trùng lặp quá nhiều.
	/// </summary>
	public int MaxOccurrences { get; private set; } = -1;

	/// <summary>Tổng weight của tất cả categories. Dùng để tính xác suất tương đối.</summary>
	public float TotalCategoryWeight { get; private set; }

	public AffixDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Đọc toàn bộ AffixDefinition từ XML.
	/// Thứ tự: Id → MaxOccurrences → Droppable → ItemLevel[Min,Max] → ItemCategories → Levels → EpicBonus.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		// 1. Đọc Id
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			Debug.LogError("The AffixDefinition has no Id!");
			return;
		}
		Id = xAttribute.Value;
		// 2. Đọc MaxOccurrences (optional, default = -1 = vô hạn)
		XAttribute xAttribute2 = xElement.Attribute("MaxOccurrences");
		if (xAttribute2 != null)
		{
			if (int.TryParse(xAttribute2.Value, out var result))
			{
				MaxOccurrences = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse MaxOccurrences attribute into an int : " + xAttribute2.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "AffixDefinition");
			}
		}
		// 3. Đọc Droppable (optional, default = true)
		XAttribute xAttribute3 = xElement.Attribute("Droppable");
		if (xAttribute3 != null)
		{
			if (!bool.TryParse(xAttribute3.Value, out var result2))
			{
				Debug.LogError("AffixDefinition " + Id + " has an invalid Droppable!");
				return;
			}
			Droppable = result2;
		}
		// 4. Đọc ItemLevel range [Min, Max] - affix chỉ xuất hiện trên item trong khoảng này
		XElement xElement2 = xElement.Element("ItemLevel");
		if (xElement2 == null)
		{
			Debug.LogError("The AffixDefinition " + Id + " has no ItemLevel!");
			return;
		}
		XAttribute xAttribute4 = xElement2.Attribute("Min");
		if (xAttribute4 == null)
		{
			Debug.LogError("The AffixDefinition " + Id + " has no ItemLevel Min!");
			return;
		}
		if (!int.TryParse(xAttribute4.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result3) || result3 < 0)
		{
			Debug.LogError("The AffixDefinition " + Id + " has an invalid ItemLevel Min " + xAttribute4.Value + "!");
			return;
		}
		LevelMin = result3;
		XAttribute xAttribute5 = xElement2.Attribute("Max");
		if (xAttribute5 == null)
		{
			Debug.LogError("The AffixDefinition " + Id + " has no ItemLevel Max!");
			return;
		}
		if (!int.TryParse(xAttribute5.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result4) || result4 < 0)
		{
			Debug.LogError("The AffixDefinition " + Id + " has an invalid ItemLevel Max " + xAttribute5.Value + "!");
			return;
		}
		LevelMax = result4;
		// 5. Đọc ItemCategories - category nào dùng được + weight
		XElement xElement3 = xElement.Element("ItemCategories");
		if (xElement3 == null)
		{
			Debug.LogError("The AffixDefinition " + Id + " has no ItemCategories!");
			return;
		}
		foreach (XElement item in xElement3.Elements("ItemCategory"))
		{
			if (item.IsNullOrEmpty())
			{
				Debug.LogError("AffixDefinition " + Id + "'s ItemCategory is empty!");
				continue;
			}
			if (!Enum.TryParse<ItemDefinition.E_Category>(item.Value, out var result5))
			{
				Debug.LogError("AffixDefinition " + Id + "'s ItemCategory " + HasAnInvalid("E_Category", item.Value));
				continue;
			}
			if (ItemCategoriesWithWeight.ContainsKey(result5))
			{
				Debug.LogError($"The affix {Id} already contains an  ItemCategory {result5}!");
				continue;
			}
			XAttribute xAttribute6 = item.Attribute("Weight");
			if (xAttribute6.IsNullOrEmpty())
			{
				TPDebug.Log($"The affix {Id} must have a Weight to its ItemCategory {result5}");
				return;
			}
			if (!float.TryParse(xAttribute6.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result6))
			{
				TPDebug.Log($"The affix {Id} must have a valid Weight (float) to its ItemCategory {result5}");
				return;
			}
			ItemCategoriesWithWeight.Add(result5, result6);
			TotalCategoryWeight += result6;
		}
		// 6. Đọc Levels - định nghĩa stat modifiers theo từng Affix level
		XElement xElement4 = xElement.Element("Levels");
		if (xElement4 == null)
		{
			Debug.LogError("The AffixDefinition " + Id + " has no Levels!");
			return;
		}
		foreach (XElement item2 in xElement4.Elements("Level"))
		{
			LeveledAffixDefinition leveledAffixDefinition = new LeveledAffixDefinition(this, item2);
			LevelDefinitions.Add(leveledAffixDefinition.Level, leveledAffixDefinition);
		}
		// 7. Đọc EpicBonus - stat bonus thêm khi Affix là Epic
		XElement xElement5 = xElement.Element("EpicBonus");
		if (xElement5.IsNullOrEmpty())
		{
			Debug.LogError("The AffixDefinition " + Id + " has no EpicBonus!");
			return;
		}
		foreach (XElement item3 in xElement5.Elements("Modifier"))
		{
			if (item3.IsNullOrEmpty())
			{
				Debug.LogError("The Modifier is empty!");
				continue;
			}
			XAttribute xAttribute7 = item3.Attribute("Stat");
			if (xAttribute7.IsNullOrEmpty())
			{
				Debug.LogError("The Modifier has no Stat!");
			}
			else
			{
				EpicStatModifiers.Add((UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), xAttribute7.Value), float.Parse(item3.Value, NumberStyles.Float, CultureInfo.InvariantCulture));
			}
		}
	}
}
