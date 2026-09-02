using TheLastStand.Definition.Meta;
using TheLastStand.Model.Meta;

namespace TheLastStand.Controller.Meta;

public class MetaReplicaConditionController
{
	public MetaReplicaConditionDefinition ConditionDefinition { get; protected set; }

	public MetaReplicaConditionController(MetaReplicaConditionDefinition definition)
	{
		ConditionDefinition = definition;
	}

	public virtual bool Evaluate(MetaNarration context)
	{
		return true;
	}
}
