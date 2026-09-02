using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class AttackDataModifierEffect : APerkEffect
{
	public AttackDataModifierEffectDefinition AttackDataModifierEffectDefinition => base.APerkEffectDefinition as AttackDataModifierEffectDefinition;

	public AttackDataModifierEffect(AttackDataModifierEffectDefinition aPerkEffectDefinition, AttackDataModifierEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
