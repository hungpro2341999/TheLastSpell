using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Điều kiện câu thoại Meta: Yêu cầu số ngày/đêm đã chơi trong game (DaysPlayed).
/// </summary>
public class MetaReplicaDaysPlayedConditionDefinition : MetaReplicaConditionDefinition
{
	public class Constants
	{
		public const string Name = "DaysPlayed";
	}

	/// <summary>
	/// Số ngày yêu cầu đã trải qua để thỏa mãn điều kiện.
	/// </summary>
	public int DaysCount { get; private set; }

	public MetaReplicaDaysPlayedConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		if (!int.TryParse(xElement.Value, out var result))
		{
			Debug.LogError("Could not parse " + xElement.Value + " to a valid int value.");
		}
		else
		{
			DaysCount = result;
		}
	}
}
