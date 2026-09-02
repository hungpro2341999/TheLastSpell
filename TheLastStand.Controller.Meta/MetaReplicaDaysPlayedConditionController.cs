using TheLastStand.Definition.Meta;
using TheLastStand.Manager;
using TheLastStand.Model.Meta;

namespace TheLastStand.Controller.Meta;

public class MetaReplicaDaysPlayedConditionController : MetaReplicaConditionController
{
	public MetaReplicaDaysPlayedConditionDefinition DaysPlayedDefinition => base.ConditionDefinition as MetaReplicaDaysPlayedConditionDefinition;

	public MetaReplicaDaysPlayedConditionController(MetaReplicaDaysPlayedConditionDefinition definition)
		: base(definition)
	{
	}

	public override bool Evaluate(MetaNarration context)
	{
		return ApplicationManager.Application.DaysPlayed >= DaysPlayedDefinition.DaysCount;
	}
}
