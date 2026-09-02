using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class RestoreStatEffect : APerkEffect
{
	public RestoreStatEffectDefinition RestoreStatEffectDefinition => base.APerkEffectDefinition as RestoreStatEffectDefinition;

	public RestoreStatEffect(RestoreStatEffectDefinition aPerkEffectDefinition, RestoreStatEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
