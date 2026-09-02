using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta;

public abstract class MetaReplicaConditionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public MetaReplicaConditionDefinition(XContainer container)
		: base(container)
	{
	}
}
