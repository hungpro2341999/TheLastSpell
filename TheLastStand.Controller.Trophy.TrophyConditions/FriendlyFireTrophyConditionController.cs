using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class FriendlyFireTrophyConditionController : ValueIntTrophyConditionController
{
	public override string Name { get; }

	public FriendlyFireTrophyDefinition FriendlyFireTrophyDefinition => base.TrophyConditionDefinition as FriendlyFireTrophyDefinition;

	public FriendlyFireTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override string ToString()
	{
		if (base.ValueProgression <= FriendlyFireTrophyDefinition.Value)
		{
			return "No traitor here !";
		}
		return "An hero hit his ally !";
	}

	protected override void CheckCompleteState()
	{
		if (!isCompleted)
		{
			isCompleted = base.ValueProgression > FriendlyFireTrophyDefinition.Value;
		}
	}
}
