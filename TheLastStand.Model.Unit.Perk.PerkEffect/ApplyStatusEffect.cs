using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class ApplyStatusEffect : APerkEffect
{
	public ApplyStatusEffectDefinition ApplyStatusEffectDefinition => base.APerkEffectDefinition as ApplyStatusEffectDefinition;

	public PerkTargeting PerkTargeting { get; private set; }

	public ApplyStatusEffect(ApplyStatusEffectDefinition aPerkEffectDefinition, ApplyStatusEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
		PerkTargeting = new PerkTargeting(aPerkEffectDefinition.PerkTargetingDefinition);
	}
}
