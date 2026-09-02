using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class GetAdditionalExperienceEffect : APerkEffect
{
	public GetAdditionalExperienceEffectDefinition GetAdditionalExperienceEffectDefinition => base.APerkEffectDefinition as GetAdditionalExperienceEffectDefinition;

	public GetAdditionalExperienceEffect(GetAdditionalExperienceEffectDefinition aPerkEffectDefinition, GetAdditionalExperienceEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
