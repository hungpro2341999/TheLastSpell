using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class AllowDiagonalPropagationEffectController : APerkEffectController
{
	public AllowDiagonalPropagationEffect AllowDiagonalPropagationEffect => base.PerkEffect as AllowDiagonalPropagationEffect;

	public AllowDiagonalPropagationEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new AllowDiagonalPropagationEffect(aPerkEffectDefinition as AllowDiagonalPropagationEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		if (!base.PerkEffect.APerkModule.Perk.Owner.AllowDiagonalPropagationEffects.Contains(AllowDiagonalPropagationEffect))
		{
			base.PerkEffect.APerkModule.Perk.Owner.AllowDiagonalPropagationEffects.Add(AllowDiagonalPropagationEffect);
		}
	}

	public override void Lock(bool onLoad)
	{
		base.PerkEffect.APerkModule.Perk.Owner.AllowDiagonalPropagationEffects.Remove(AllowDiagonalPropagationEffect);
	}
}
