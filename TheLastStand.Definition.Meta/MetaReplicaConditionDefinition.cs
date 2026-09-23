using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Lớp cơ sở trừu tượng cho các điều kiện kích hoạt câu thoại kịch bản (Meta Replica Condition).
/// </summary>
public abstract class MetaReplicaConditionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public MetaReplicaConditionDefinition(XContainer container)
		: base(container)
	{
	}
}
