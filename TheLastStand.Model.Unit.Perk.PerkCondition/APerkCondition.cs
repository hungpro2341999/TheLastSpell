using TheLastStand.Definition.Unit.Perk.PerkCondition;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkCondition;

public abstract class APerkCondition
{
	public APerkConditionDefinition PerkConditionDefinition { get; private set; }

	public APerkModule PerkModule { get; private set; }

	public APerkCondition(APerkConditionDefinition aPerkConditionDefinition, APerkModule perkModule)
	{
		PerkConditionDefinition = aPerkConditionDefinition;
		PerkModule = perkModule;
	}

	public abstract bool IsValid();
}
