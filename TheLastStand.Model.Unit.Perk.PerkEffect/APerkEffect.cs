using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkDataCondition;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public abstract class APerkEffect
{
	public APerkEffectController APerkEffectController { get; private set; }

	public APerkEffectDefinition APerkEffectDefinition { get; private set; }

	public APerkModule APerkModule { get; private set; }

	public PerkDataConditions PerkDataConditions { get; private set; }

	public APerkEffect(APerkEffectDefinition aPerkEffectDefinition, APerkEffectController aPerkEffectController, APerkModule aPerkModule)
	{
		APerkEffectController = aPerkEffectController;
		APerkEffectDefinition = aPerkEffectDefinition;
		APerkModule = aPerkModule;
		PerkDataConditions = new PerkDataConditions(APerkEffectDefinition.PerkDataConditionsDefinition, aPerkModule.Perk);
	}
}
