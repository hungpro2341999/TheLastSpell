using TheLastStand.Definition.Meta;
using TheLastStand.Manager;

namespace TheLastStand.Controller.Meta;

public class MetaNarrationDaysPlayedConditionController : MetaNarrationConditionController
{
	public MetaReplicaDaysPlayedConditionDefinition DaysPlayedDefinition => base.ConditionDefinition as MetaReplicaDaysPlayedConditionDefinition;

	public MetaNarrationDaysPlayedConditionController(MetaReplicaDaysPlayedConditionDefinition definition)
		: base(definition)
	{
	}

	public override bool Evaluate()
	{
		return ApplicationManager.Application.DaysPlayed >= DaysPlayedDefinition.DaysCount;
	}
}
