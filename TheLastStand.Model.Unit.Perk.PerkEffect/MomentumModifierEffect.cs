using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class MomentumModifierEffect : APerkEffect
{
	public MomentumModifierEffectDefinition MomentumModifierEffectDefinition => base.APerkEffectDefinition as MomentumModifierEffectDefinition;

	public MomentumModifierEffect(MomentumModifierEffectDefinition aPerkEffectDefinition, MomentumModifierEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
