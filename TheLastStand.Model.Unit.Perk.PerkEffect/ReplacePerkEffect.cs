using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class ReplacePerkEffect : APerkEffect
{
	public ReplacePerkEffectDefinition ReplacePerkEffectDefinition => base.APerkEffectDefinition as ReplacePerkEffectDefinition;

	public ReplacePerkEffect(ReplacePerkEffectDefinition aPerkEffectDefinition, APerkEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
