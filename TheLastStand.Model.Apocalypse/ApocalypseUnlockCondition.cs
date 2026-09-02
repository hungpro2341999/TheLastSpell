using TheLastStand.Definition.Apocalypse;

namespace TheLastStand.Model.Apocalypse;

public class ApocalypseUnlockCondition
{
	public virtual bool IsValid => false;

	public ApocalypseUnlockConditionDefinition Definition { get; }

	public ApocalypseUnlockCondition(ApocalypseUnlockConditionDefinition definition)
	{
		Definition = definition;
	}
}
