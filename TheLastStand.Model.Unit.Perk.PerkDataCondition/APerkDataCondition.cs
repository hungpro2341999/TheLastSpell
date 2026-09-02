using TheLastStand.Definition.Unit.Perk.PerkDataCondition;

namespace TheLastStand.Model.Unit.Perk.PerkDataCondition;

public abstract class APerkDataCondition
{
	public Perk Perk { get; private set; }

	public APerkDataConditionDefinition PerkDataConditionDefinition { get; private set; }

	public APerkDataCondition(APerkDataConditionDefinition aPerkDataConditionDefinition, Perk perk)
	{
		PerkDataConditionDefinition = aPerkDataConditionDefinition;
		Perk = perk;
	}

	public abstract bool IsValid();
}
