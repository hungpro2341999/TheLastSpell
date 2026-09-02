using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class PermanentBaseStatModifierEffect : APerkEffect
{
	public PermanentBaseStatModifierEffectDefinition PermanentBaseStatModifierEffectDefinition => base.APerkEffectDefinition as PermanentBaseStatModifierEffectDefinition;

	public float Value => PermanentBaseStatModifierEffectDefinition.ValueExpression.EvalToInt(base.APerkModule.Perk);

	public PermanentBaseStatModifierEffect(PermanentBaseStatModifierEffectDefinition aPerkEffectDefinition, PermanentBaseStatModifierEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
