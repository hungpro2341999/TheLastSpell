using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class ModifyContextualSkillOverallUsesEffectController : APerkEffectController
{
	public ModifyContextualSkillOverallUsesEffect ModifyContextualSkillOverallUsesEffect => base.PerkEffect as ModifyContextualSkillOverallUsesEffect;

	public ModifyContextualSkillOverallUsesEffectController(ModifyContextualSkillOverallUsesEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new ModifyContextualSkillOverallUsesEffect(aPerkEffectDefinition as ModifyContextualSkillOverallUsesEffectDefinition, this, aPerkModule);
	}

	public override void Trigger(PerkDataContainer data)
	{
		base.Trigger(data);
		TheLastStand.Model.Skill.Skill skill = base.PerkEffect.APerkModule.Perk.Owner.ContextualSkills.Find((TheLastStand.Model.Skill.Skill x) => x.SkillDefinition.Id == ModifyContextualSkillOverallUsesEffect.ModifyContextualSkillOverallUsesEffectDefinition.ContextualSkillId);
		if (skill == null || skill.OverallUses <= 0)
		{
			return;
		}
		int overallUses = ModifyContextualSkillOverallUsesEffect.ModifyContextualSkillOverallUsesEffectDefinition.GetOverallUses(base.PerkEffect.APerkModule.Perk);
		if (overallUses > 0)
		{
			skill.SkillController.ModifyOverallUses(overallUses);
			if (ModifyContextualSkillOverallUsesEffect.ModifyContextualSkillOverallUsesEffectDefinition.RefillOverallUses)
			{
				skill.OverallUsesRemaining = skill.ComputeTotalUses();
			}
		}
	}
}
