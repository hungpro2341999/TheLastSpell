using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class UnlockContextualSkillEffect : APerkEffect
{
	public UnlockContextualSkillEffectDefinition UnlockContextualSkillEffectDefinition => base.APerkEffectDefinition as UnlockContextualSkillEffectDefinition;

	public UnlockContextualSkillEffect(UnlockContextualSkillEffectDefinition aPerkEffectDefinition, UnlockContextualSkillEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
