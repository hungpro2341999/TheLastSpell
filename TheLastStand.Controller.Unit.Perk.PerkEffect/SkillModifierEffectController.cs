using System.Collections.Generic;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class SkillModifierEffectController : APerkEffectController
{
	public SkillModifierEffect SkillModifierEffect => base.PerkEffect as SkillModifierEffect;

	public SkillModifierEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new SkillModifierEffect(aPerkEffectDefinition as SkillModifierEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		Dictionary<TheLastStand.Model.Skill.Skill.E_ComputationStat, List<SkillModifierEffect>> perkSkillModifierEffects = base.PerkEffect.APerkModule.Perk.Owner.PerkSkillModifierEffects;
		TheLastStand.Model.Skill.Skill.E_ComputationStat computationStat = SkillModifierEffect.SkillModifierEffectDefinition.ComputationStat;
		if (!perkSkillModifierEffects.ContainsKey(computationStat) || perkSkillModifierEffects[computationStat] == null)
		{
			perkSkillModifierEffects.Add(computationStat, new List<SkillModifierEffect>());
		}
		if (!perkSkillModifierEffects[computationStat].Contains(SkillModifierEffect))
		{
			perkSkillModifierEffects[computationStat].Add(SkillModifierEffect);
		}
	}

	public override void Lock(bool onLoad)
	{
		Dictionary<TheLastStand.Model.Skill.Skill.E_ComputationStat, List<SkillModifierEffect>> perkSkillModifierEffects = base.PerkEffect.APerkModule.Perk.Owner.PerkSkillModifierEffects;
		TheLastStand.Model.Skill.Skill.E_ComputationStat computationStat = SkillModifierEffect.SkillModifierEffectDefinition.ComputationStat;
		if (perkSkillModifierEffects.ContainsKey(computationStat) && perkSkillModifierEffects[computationStat] != null && perkSkillModifierEffects[computationStat].Contains(SkillModifierEffect))
		{
			perkSkillModifierEffects[computationStat].Remove(SkillModifierEffect);
		}
	}
}
