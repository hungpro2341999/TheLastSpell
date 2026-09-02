using TheLastStand.Controller.Skill;
using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class CastSkillEffect : APerkEffect
{
	public readonly TheLastStand.Model.Skill.Skill Skill;

	public readonly PerkTargeting PerkTargeting;

	public CastSkillEffectDefinition CastSkillEffectDefinition => base.APerkEffectDefinition as CastSkillEffectDefinition;

	public CastSkillEffect(CastSkillEffectDefinition aPerkEffectDefinition, CastSkillEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
		Skill = new SkillController(aPerkEffectDefinition.SkillDefinition, base.APerkModule.Perk).Skill;
		PerkTargeting = new PerkTargeting(aPerkEffectDefinition.PerkTargetingDefinition);
	}
}
