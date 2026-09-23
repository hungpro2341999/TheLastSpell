using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Thưởng tài nguyên khởi đầu (InitResourcesBonus).
/// <para>Cung cấp thêm Vàng (Gold) và Vật liệu (Materials) khi người chơi bắt đầu một lượt chơi (run) mới.</para>
/// </summary>
public class InitResourcesBonusMetaEffectDefinition : MetaEffectDefinition
{
	/// <summary>
	/// Tên định danh của thẻ XML đại diện cho hiệu ứng này ("InitResourcesBonus").
	/// </summary>
	public const string Name = "InitResourcesBonus";

	/// <summary>
	/// Lượng Vàng (Gold) được cộng thêm khi bắt đầu lượt chơi.
	/// </summary>
	public int GoldBonus { get; private set; }

	/// <summary>
	/// Lượng Vật liệu (Materials) được cộng thêm khi bắt đầu lượt chơi.
	/// </summary>
	public int MaterialsBonus { get; private set; }

	/// <summary>
	/// Khởi tạo định nghĩa hiệu ứng từ XML container.
	/// </summary>
	public InitResourcesBonusMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Đọc và phân tích dữ liệu từ phần tử XML &lt;InitResourcesBonus&gt;.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		
		// Đọc giá trị thưởng Vàng
		XElement xElement = obj.Element("GoldBonus");
		if (xElement != null)
		{
			if (int.TryParse(xElement.Value, out var result))
			{
				GoldBonus = result;
			}
			else
			{
				Debug.LogError("GoldBonus element as an invalid value!");
			}
		}

		// Đọc giá trị thưởng Vật liệu
		XElement xElement2 = obj.Element("MaterialsBonus");
		if (xElement2 != null)
		{
			if (int.TryParse(xElement2.Value, out var result2))
			{
				MaterialsBonus = result2;
			}
			else
			{
				Debug.LogError("MaterialsBonus element as an invalid value!");
			}
		}
	}

	public override string ToString()
	{
		return string.Format("{0} ({1} gold / {2} materials)", "InitResourcesBonus", GoldBonus, MaterialsBonus);
	}
}
