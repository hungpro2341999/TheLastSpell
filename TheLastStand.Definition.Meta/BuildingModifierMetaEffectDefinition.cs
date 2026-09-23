using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Tinh chỉnh chỉ số công trình (BuildingModifier).
/// <para>Áp dụng các thay đổi cho công trình như: giảm giá vàng xây dựng, tăng máu, tăng sản lượng tài nguyên bị động, hoặc tăng giới hạn số lượng công trình được xây trong thành phố.</para>
/// </summary>
public class BuildingModifierMetaEffectDefinition : MetaEffectDefinition
{
	/// <summary>
	/// Tên định danh của thẻ XML ("BuildingModifier").
	/// </summary>
	public const string Name = "BuildingModifier";

	/// <summary>
	/// Mã định danh của công trình chịu tác động (ví dụ: "Inn", "MagicShop", "Catapult"...).
	/// </summary>
	public string BuildingId { get; private set; }

	/// <summary>
	/// Số lượng Vàng được giảm khi xây dựng công trình.
	/// </summary>
	public int GoldCostReduction { get; private set; }

	/// <summary>
	/// Lượng Máu (Health) được cộng thêm cho công trình.
	/// </summary>
	public int HealthBonus { get; private set; }

	/// <summary>
	/// Lượng tài nguyên sản xuất bị động (Passive Production) được cộng thêm mỗi ngày.
	/// </summary>
	public int PassiveProductionBonus { get; private set; }

	/// <summary>
	/// Số lượng tối đa công trình loại này có thể được xây trong thành phố được tăng thêm.
	/// </summary>
	public sbyte MaxCityInstancesBonus { get; private set; }

	public BuildingModifierMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Giải tuần tự hóa các thông số điều chỉnh công trình từ XML.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		BuildingId = xElement.Attribute("Id").Value;

		// Đọc giảm giá Vàng xây dựng
		XElement xElement2 = xElement.Element("GoldCostReduction");
		if (xElement2 != null)
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				Debug.LogError("GoldCostReduction element has an invalid value!");
				return;
			}
			GoldCostReduction = result;
		}

		// Đọc tăng giới hạn số lượng xây trong thành phố
		XElement xElement3 = xElement.Element("MaxCityInstancesBonus");
		if (xElement3 != null)
		{
			if (!sbyte.TryParse(xElement3.Value, out var result2))
			{
				Debug.LogError("MaxCityInstancesBonus element has an invalid value!");
				return;
			}
			MaxCityInstancesBonus = result2;
		}

		// Đọc tăng Máu công trình
		XElement xElement4 = xElement.Element("HealthBonus");
		if (xElement4 != null)
		{
			if (!int.TryParse(xElement4.Value, out var result3))
			{
				Debug.LogError("HealthBonus element has an invalid value!");
				return;
			}
			HealthBonus = result3;
		}

		// Đọc tăng sản lượng sản xuất bị động
		XElement xElement5 = xElement.Element("PassiveProductionBonus");
		if (xElement5 != null)
		{
			if (int.TryParse(xElement5.Value, out var result4))
			{
				PassiveProductionBonus = result4;
			}
			else
			{
				Debug.LogError("PassiveProductionBonus element has an invalid value!");
			}
		}
	}

	public override string ToString()
	{
		return "BuildingModifier (" + BuildingId + ")";
	}
}
