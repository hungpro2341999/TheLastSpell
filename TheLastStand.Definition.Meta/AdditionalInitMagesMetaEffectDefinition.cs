using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Thêm tướng/pháp sư khởi đầu (AdditionalInitMages).
/// <para>Tăng số lượng anh hùng mà người chơi có thể điều khiển khi bắt đầu một lượt chơi mới.</para>
/// </summary>
public class AdditionalInitMagesMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "AdditionalInitMages";

	/// <summary>
	/// Số lượng tướng được cộng thêm vào đội hình xuất phát.
	/// </summary>
	public int Amount { get; private set; }

	public AdditionalInitMagesMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		if (int.TryParse(xElement.Value, out var result))
		{
			Amount = result;
		}
		else
		{
			Debug.LogError("Could not parse value " + xElement.Value + " to an integer!");
		}
	}
}
