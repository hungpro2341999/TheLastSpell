using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class StatBoundaryModifierEffect : APerkEffect
{
	public bool HasBeenUsed;

	public StatBoundaryModifierEffectDefinition StatBoundaryModifierEffectDefinition => base.APerkEffectDefinition as StatBoundaryModifierEffectDefinition;

	public float MinModifierValue => StatBoundaryModifierEffectDefinition.MinModifierValueExpression.EvalToInt(base.APerkModule.Perk);

	public float MaxModifierValue => StatBoundaryModifierEffectDefinition.MaxModifierValueExpression.EvalToInt(base.APerkModule.Perk);

	public StatBoundaryModifierEffect(StatBoundaryModifierEffectDefinition aPerkEffectDefinition, StatBoundaryModifierEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
