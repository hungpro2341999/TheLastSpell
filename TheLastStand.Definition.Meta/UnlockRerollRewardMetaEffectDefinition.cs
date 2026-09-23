using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Mở khóa cơ chế reroll phần thưởng (UnlockRerollReward).
/// <para>Mở khóa tính năng và cấp số lượt reroll cơ sở ban đầu.</para>
/// </summary>
public class UnlockRerollRewardMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockRerollReward";

	/// <summary>
	/// Số lượt reroll phần thưởng cơ sở được mở khóa.
	/// </summary>
	public int RerollReward { get; private set; }

	public UnlockRerollRewardMetaEffectDefinition(XContainer container)
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
