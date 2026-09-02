using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class RestoreUsesEffect : APerkEffect
{
	public RestoreUsesEffectDefinition RestoreUsablesUsesEffectDefinition => base.APerkEffectDefinition as RestoreUsesEffectDefinition;

	public int Value => RestoreUsablesUsesEffectDefinition.ValueExpression.EvalToInt(base.APerkModule.Perk);

	public RestoreUsesEffect(RestoreUsesEffectDefinition aPerkEffectDefinition, RestoreUsesEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
