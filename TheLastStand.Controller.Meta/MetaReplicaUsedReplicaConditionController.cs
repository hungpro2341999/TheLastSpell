using System.Linq;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager.Meta;

namespace TheLastStand.Controller.Meta;

public class MetaReplicaUsedReplicaConditionController : MetaNarrationConditionController
{
	public MetaReplicaUsedReplicaConditionDefinition UsedReplicaDefinition => base.ConditionDefinition as MetaReplicaUsedReplicaConditionDefinition;

	public MetaReplicaUsedReplicaConditionController(MetaReplicaUsedReplicaConditionDefinition definition)
		: base(definition)
	{
	}

	public override bool Evaluate()
	{
		return MetaNarrationsManager.GetAllUsedReplicas().Contains(UsedReplicaDefinition.ReplicaId);
	}
}
