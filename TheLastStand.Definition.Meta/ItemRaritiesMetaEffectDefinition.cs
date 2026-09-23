using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Item;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Tinh chỉnh trọng số độ hiếm vật phẩm (ItemRaritiesModifier).
/// <para>Tăng xác suất (weight bonus) xuất hiện các độ hiếm cao cấp (Magic, Rare, Epic) khi tạo hoặc rơi trang bị trong game.</para>
/// </summary>
public class ItemRaritiesMetaEffectDefinition : MetaEffectDefinition
{
	/// <summary>
	/// Tên định danh của thẻ XML ("ItemRaritiesModifier").
	/// </summary>
	public const string Name = "ItemRaritiesModifier";

	/// <summary>
	/// Định danh cây xác suất độ hiếm chịu tác động (ví dụ: cây mặc định, shop, night reward...).
	/// </summary>
	public string RarityTreeId { get; private set; }

	/// <summary>
	/// Bảng tra cứu trọng số cộng thêm theo từng cấp độ hiếm: Key là giá trị int của E_Rarity, Value là trọng số bonus.
	/// </summary>
	public Dictionary<int, int> WeightBonusByRarityLevel { get; set; } = new Dictionary<int, int>();

	public ItemRaritiesMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Giải tuần tự hóa các thông số trọng số độ hiếm từ XML.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		if (xAttribute == null || string.IsNullOrEmpty(xAttribute.Value))
		{
			Debug.LogError("ItemRaritiesModifier has an invalid Id or Id doesn't exist !");
		}
		RarityTreeId = xAttribute.Value;

		// Đọc các thẻ <Probability Weight="...">value</Probability>
		foreach (XElement item in obj.Elements("Probability"))
		{
			XAttribute xAttribute2 = item.Attribute("Weight");
			int result2;
			if (xAttribute2 == null || !int.TryParse(xAttribute2.Value, out var result))
			{
				Debug.LogError("Probability has an invalid Weight or Weight doesn't exist !");
			}
			else if (!int.TryParse(item.Value, out result2) || result2 >= Enum.GetValues(typeof(ItemDefinition.E_Rarity)).Length || result2 < 0)
			{
				Debug.LogError("Probability has an invalid Value or Value can't be parse as E_Rarity !");
			}
			else if (WeightBonusByRarityLevel.ContainsKey(result2))
			{
				WeightBonusByRarityLevel[result2] += result;
			}
			else
			{
				WeightBonusByRarityLevel.Add(result2, result);
			}
		}
	}

	public override string ToString()
	{
		string text = "Rarity Probability Tree : <b>" + RarityTreeId + "</b> Modifications :\r\n";
		foreach (KeyValuePair<int, int> item in WeightBonusByRarityLevel)
		{
			text += $"\tRarityLevel : <b>{item.Key}</b> ; Bonus Weight : <b>{item.Value}</b>;\r\n";
		}
		return text;
	}
}
