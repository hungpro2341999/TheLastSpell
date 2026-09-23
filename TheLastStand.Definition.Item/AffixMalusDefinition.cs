using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

/// <summary>
/// Bản thiết kế Affix phạt (negative modifier) - áp dụng trong chế độ Apocalypse.
/// 
/// Trong chế độ Apocalypse, item có thể bị "nguyền rủa" với stat âm.
/// Ví dụ: "-5 Dodge", "-3 Resistance".
/// 
/// Mỗi AffixMalusDefinition mô tả 1 loại phạt cho 1 stat:
/// - Stat nào bị phạt (Dodge, Resistance...)
/// - Weight (tỉ lệ bị chọn)
/// - Giá trị phạt theo từng MalusLevel (Small/Medium/Big)
/// 
/// Ví dụ XML:
/// <code>
/// &lt;AffixMalus StatId="Dodge"&gt;
///   &lt;Weight&gt;50&lt;/Weight&gt;
///   &lt;Values&gt;
///     &lt;Small&gt;-2&lt;/Small&gt;
///     &lt;Medium&gt;-5&lt;/Medium&gt;
///     &lt;Big&gt;-10&lt;/Big&gt;
///   &lt;/Values&gt;
/// &lt;/AffixMalus&gt;
/// </code>
/// </summary>
public class AffixMalusDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>
	/// Mức độ phạt. Dùng cùng bảng xác suất AffixMalusLevelsProbas để random.
	/// </summary>
	public enum E_MalusLevel
	{
		/// <summary>Không có phạt (item may mắn thoát).</summary>
		Undefined,
		/// <summary>Phạt nhẹ. Ví dụ: -2 Dodge.</summary>
		Small,
		/// <summary>Phạt vừa. Ví dụ: -5 Dodge.</summary>
		Medium,
		/// <summary>Phạt nặng. Ví dụ: -10 Dodge.</summary>
		Big
	}

	/// <summary>
	/// Custom comparer cho E_MalusLevel enum - tối ưu performance khi dùng làm Dictionary key.
	/// Tránh boxing khi so sánh enum (default EqualityComparer gây boxing).
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct MalusLevelComparer : IEqualityComparer<E_MalusLevel>
	{
		public bool Equals(E_MalusLevel x, E_MalusLevel y)
		{
			return x == y;
		}

		public int GetHashCode(E_MalusLevel obj)
		{
			return (int)obj;
		}
	}

	/// <summary>Shared instance để tránh tạo comparer mới mỗi lần tạo Dictionary.</summary>
	public static readonly MalusLevelComparer SharedMalusLevelComparer;

	/// <summary>
	/// Giá trị phạt theo từng MalusLevel.
	/// Ví dụ: { Small: -2, Medium: -5, Big: -10 }
	/// </summary>
	public Dictionary<E_MalusLevel, float> MalusPerLevel { get; private set; }

	/// <summary>Stat bị phạt. Ví dụ: Dodge, Resistance, PhysicalDamage.</summary>
	public UnitStatDefinition.E_Stat Stat { get; private set; }

	/// <summary>
	/// Tỉ lệ (weight) để random chọn malus này.
	/// Weight cao = xuất hiện thường xuyên hơn.
	/// </summary>
	public int Weight { get; private set; }

	public AffixMalusDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Đọc AffixMalusDefinition từ XML.
	/// Thứ tự: StatId → Weight → Values (Small/Medium/Big).
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		// Đọc StatId - stat nào bị phạt
		XAttribute xAttribute = xElement.Attribute("StatId");
		if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("Could not parse StatId Attribute value " + xAttribute.Value + " to a valid E_Stat value.");
			return;
		}
		Stat = result;
		// Đọc Weight - tỉ lệ xuất hiện
		XElement xElement2 = xElement.Element("Weight");
		if (!int.TryParse(xElement2.Value, out var result2))
		{
			CLoggerManager.Log("Could not parse AffixMalusDefinition Weight element value " + xElement2.Value + " to a valid int value.");
			return;
		}
		Weight = result2;
		// Đọc Values - giá trị phạt theo từng MalusLevel
		MalusPerLevel = new Dictionary<E_MalusLevel, float>(SharedMalusLevelComparer);
		foreach (XElement item in xElement.Element("Values").Elements())
		{
			// Element name = MalusLevel (Small/Medium/Big), value = giá trị phạt
			string localName = item.Name.LocalName;
			if (!Enum.TryParse<E_MalusLevel>(localName, out var result3))
			{
				CLoggerManager.Log($"Could not parse value Element Name of AffixMalusDefinition with Id {Stat} {localName} to a valid E_MalusLevel value.", LogType.Error);
				break;
			}
			if (!float.TryParse(item.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result4))
			{
				CLoggerManager.Log($"Could not parse Malus value element of AffixMalusDefinition {Stat} {item.Value} to a valid float value.", LogType.Error);
				break;
			}
			MalusPerLevel.Add(result3, result4);
		}
	}

	/// <summary>
	/// Kiểm tra xem MalusLevel này có được định nghĩa giá trị phạt không.
	/// Dùng trong ItemManager.ApplyMaluses() để lọc malus khả dụng.
	/// </summary>
	/// <param name="malusLevel">Level phạt cần kiểm tra.</param>
	/// <returns>True nếu có giá trị phạt cho level này.</returns>
	public bool IsMalusLevelDefined(E_MalusLevel malusLevel)
	{
		return MalusPerLevel.ContainsKey(malusLevel);
	}
}
