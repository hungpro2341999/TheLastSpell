using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class StatModifierEffect : APerkEffect
{
	public bool HasBeenUsed;

	public StatModifierEffectDefinition StatModifierEffectDefinition => base.APerkEffectDefinition as StatModifierEffectDefinition;

	public float Value => StatModifierEffectDefinition.ValueExpression.EvalToInt(base.APerkModule.Perk);

	public StatModifierEffect(StatModifierEffectDefinition aPerkEffectDefinition, StatModifierEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
