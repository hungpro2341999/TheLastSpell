using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class AllowDiagonalPropagationEffect : APerkEffect
{
	public AllowDiagonalPropagationEffectDefinition AllowDiagonalPropagationEffectDefinition => base.APerkEffectDefinition as AllowDiagonalPropagationEffectDefinition;

	public AllowDiagonalPropagationEffect(AllowDiagonalPropagationEffectDefinition aPerkEffectDefinition, AllowDiagonalPropagationEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
