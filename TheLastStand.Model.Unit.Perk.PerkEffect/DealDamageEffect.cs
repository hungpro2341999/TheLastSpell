using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class DealDamageEffect : APerkEffect
{
	public DealDamageEffectDefinition DealDamageEffectDefinition => base.APerkEffectDefinition as DealDamageEffectDefinition;

	public PerkTargeting PerkTargeting { get; private set; }

	public DealDamageEffect(DealDamageEffectDefinition aPerkEffectDefinition, DealDamageEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
		PerkTargeting = new PerkTargeting(aPerkEffectDefinition.PerkTargetingDefinition);
	}
}
