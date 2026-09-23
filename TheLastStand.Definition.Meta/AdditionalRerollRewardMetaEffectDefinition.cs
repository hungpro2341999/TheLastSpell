using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Thêm lượt reroll phần thưởng (AdditionalRerollReward).
/// <para>Cung cấp thêm số lần quay lại (reroll) cho các bảng phần thưởng thăng cấp (level up) hoặc sản xuất đêm.</para>
/// </summary>
public class AdditionalRerollRewardMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "AdditionalRerollReward";

	/// <summary>
	/// Số lượt reroll phần thưởng được cộng thêm.
	/// </summary>
	public int RerollReward { get; private set; }

	public AdditionalRerollRewardMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		if (int.TryParse(xElement.Value, out var result))
		{
			RerollReward = result;
		}
		else
		{
			Debug.LogError("Could not parse value " + xElement.Value + " to an integer!");
		}
	}
}
