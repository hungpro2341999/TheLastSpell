using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa danh sách các điều kiện dẫn chuyện/kịch bản (Meta Narration Conditions).
/// <para>Chứa tập hợp các điều kiện như số ngày đã chơi (DaysPlayed), câu thoại đã dùng (UsedReplica), hoặc nâng cấp Meta đã mở khóa (MetaUpgradeUnlocked).</para>
/// </summary>
public class MetaNarrationConditionsDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>
	/// Danh sách các điều kiện cụ thể cần thỏa mãn.
	/// </summary>
	public List<MetaReplicaConditionDefinition> Conditions { get; private set; }

	public MetaNarrationConditionsDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Giải tuần tự hóa các loại điều kiện thoại từ XML.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		Conditions = new List<MetaReplicaConditionDefinition>();
		foreach (XElement item in obj.Elements())
		{
			switch (item.Name.LocalName)
			{
			case "DaysPlayed":
				Conditions.Add(new MetaReplicaDaysPlayedConditionDefinition(item));
				break;
			case "UsedReplica":
				Conditions.Add(new MetaReplicaUsedReplicaConditionDefinition(item));
				break;
			case "MetaUpgradeUnlocked":
				Conditions.Add(new MetaReplicaMetaUpgradeUnlockedConditionDefinition(item));
				break;
			default:
				Debug.LogError("Invalid condition name " + item.Name.LocalName + ".");
				return;
			}
		}
	}
}
