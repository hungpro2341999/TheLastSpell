using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Điều kiện câu thoại Meta: Yêu cầu người chơi đã từng nghe/kích hoạt một câu thoại trước đó (UsedReplica).
/// </summary>
public class MetaReplicaUsedReplicaConditionDefinition : MetaReplicaConditionDefinition
{
	public class Constants
	{
		public const string Name = "UsedReplica";
	}

	/// <summary>
	/// Mã định danh của câu thoại tiên quyết cần phải được kích hoạt trước đó.
	/// </summary>
	public string ReplicaId { get; private set; }

	public MetaReplicaUsedReplicaConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		ReplicaId = xElement.Value;
	}
}
