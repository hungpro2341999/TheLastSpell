using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class LockSkillEffect : APerkEffect
{
	public LockSkillEffectDefinition LockSkillEffectDefinition => base.APerkEffectDefinition as LockSkillEffectDefinition;

	public LockSkillEffect(LockSkillEffectDefinition aPerkEffectDefinition, LockSkillEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
