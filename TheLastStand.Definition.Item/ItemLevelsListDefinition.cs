using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

/// <summary>
/// Bảng xác suất level item — dùng khi cần random level cho item được sinh.
/// 
/// Mỗi entry = 1 level + weight (tỉ lệ).
/// Weight cao = level đó xuất hiện thường xuyên hơn.
/// 
/// Ví dụ: { Level 0: 100, Level 1: 60, Level 2: 30 }
/// → Xác suất Level 0 = 100/(100+60+30) ≈ 53%.
/// 
/// Ví dụ XML:
/// <code>
/// &lt;ItemLevelsList Id="StandardLevels"&gt;
///   &lt;ItemLevel Id="0" Odd="100"/&gt;
///   &lt;ItemLevel Id="1" Odd="60"/&gt;
///   &lt;ItemLevel Id="2" Odd="30"/&gt;
/// &lt;/ItemLevelsList&gt;
/// </code>
/// </summary>
public class ItemLevelsListDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>ID duy nhất. Ví dụ: "StandardLevels", "HighTierLevels".</summary>
	public string Id { get; set; }

	/// <summary>
	/// Bảng level → weight (tỉ lệ).
	/// Key = level (int), Value = weight (int, "Odd").
	/// </summary>
	public Dictionary<int, int> ItemLevelsWithOdd { get; set; } = new Dictionary<int, int>();

	public ItemLevelsListDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Đọc danh sách level + weight từ XML.
	/// Mỗi element ItemLevel có: Id (level number) + Odd (weight).
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			Debug.LogError("xItemLevelsListDefinition must have a valid Id");
			return;
		}
		Id = xAttribute.Value;
		foreach (XElement item in xElement.Elements("ItemLevel"))
		{
			// Đọc Odd (weight)
			XAttribute xAttribute2 = item.Attribute("Odd");
			if (xAttribute2.IsNullOrEmpty() || !int.TryParse(xAttribute2.Value, out var result))
			{
				Debug.LogError(Id + " Invalid odd!");
				continue;
			}
			// Đọc level Id
			XAttribute xAttribute3 = item.Attribute("Id");
			if (xAttribute3.IsNullOrEmpty() || !int.TryParse(xAttribute3.Value, out var result2))
			{
				Debug.LogError(Id + " Invalid level!");
			}
			else
			{
				ItemLevelsWithOdd.Add(result2, result);
			}
		}
	}
}
