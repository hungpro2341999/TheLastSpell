using TheLastStand.Definition.Meta;

namespace TheLastStand.Controller.Meta;

public class MetaNarrationConditionController
{
	public MetaReplicaConditionDefinition ConditionDefinition { get; protected set; }

	public MetaNarrationConditionController(MetaReplicaConditionDefinition definition)
	{
		ConditionDefinition = definition;
	}

	public virtual bool Evaluate()
	{
		return true;
	}
}
