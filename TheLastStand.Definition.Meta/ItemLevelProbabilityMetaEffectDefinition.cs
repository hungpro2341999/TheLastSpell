using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Tinh chỉnh xác suất cấp độ vật phẩm (ItemLevelProbabilityModifier).
/// <para>Tăng trọng số (weight bonus) xuất hiện các level cao hơn của trang bị trong quá trình sinh đồ ngẫu nhiên.</para>
/// </summary>
public class ItemLevelProbabilityMetaEffectDefinition : MetaEffectDefinition
{
	/// <summary>
	/// Tên định danh của thẻ XML ("ItemLevelProbabilityModifier").
	/// </summary>
	public const string Name = "ItemLevelProbabilityModifier";

	/// <summary>
	/// Định danh cây xác suất level trang bị (ví dụ: LevelTreeId theo ngày hoặc theo độ khó).
	/// </summary>
	public string LevelTreeId { get; private set; }

	/// <summary>
	/// Bảng tra cứu trọng số cộng thêm theo từng level trang bị: Key là level của item, Value là trọng số cộng thêm.
	/// </summary>
	public Dictionary<int, int> WeightBonusByLevelProbability { get; set; } = new Dictionary<int, int>();

	public ItemLevelProbabilityMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Giải tuần tự hóa các thông số xác suất level từ XML.
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
			Debug.LogError("ItemLevelProbabilityModifier has an invalid Id or Id doesn't exist !");
		}
		LevelTreeId = xAttribute.Value;

		// Đọc các thẻ <Probability Weight="...">Level</Probability>
		foreach (XElement item in obj.Elements("Probability"))
		{
			XAttribute xAttribute2 = item.Attribute("Weight");
			int result2;
			if (xAttribute2 == null || !int.TryParse(xAttribute2.Value, out var result))
			{
				Debug.LogError("Probability has an invalid Weight or Weight doesn't exist !");
			}
			else if (!int.TryParse(item.Value, out result2))
			{
				Debug.LogError("Probability has an invalid Value !");
			}
			else if (WeightBonusByLevelProbability.ContainsKey(result2))
			{
				WeightBonusByLevelProbability[result2] += result;
			}
			else
			{
				WeightBonusByLevelProbability.Add(result2, result);
			}
		}
	}

	public override string ToString()
	{
		string text = "Level Probability Tree : <b>" + LevelTreeId + "</b> Modifications :\r\n";
		foreach (KeyValuePair<int, int> item in WeightBonusByLevelProbability)
		{
			text += $"\tLevel : <b>{item.Key}</b> ; Bonus Weight : <b>{item.Value}</b>;\r\n";
		}
		return text;
	}
}
