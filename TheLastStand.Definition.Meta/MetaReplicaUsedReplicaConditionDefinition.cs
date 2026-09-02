using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class MetaReplicaUsedReplicaConditionDefinition : MetaReplicaConditionDefinition
{
	public class Constants
	{
		public const string Name = "UsedReplica";
	}

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
