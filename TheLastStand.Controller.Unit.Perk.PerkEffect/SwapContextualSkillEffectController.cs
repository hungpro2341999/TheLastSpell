using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class SwapContextualSkillEffectController : APerkEffectController
{
	public SwapContextualSkillEffect SwapContextualSkillEffect => base.PerkEffect as SwapContextualSkillEffect;

	public SwapContextualSkillEffectController(SwapContextualSkillEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new SwapContextualSkillEffect(aPerkEffectDefinition as SwapContextualSkillEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		base.PerkEffect.APerkModule.Perk.Owner.ToggleContextualSkillLock(SwapContextualSkillEffect.SwapContextualSkillEffectDefinition.ContextualSkillIdToUnlock, locks: false, base.PerkEffect.APerkModule.Perk, SwapContextualSkillEffect.SwapContextualSkillEffectDefinition.OverallUses);
		TheLastStand.Model.Skill.Skill skill = base.PerkEffect.APerkModule.Perk.Owner.ContextualSkills.Find((TheLastStand.Model.Skill.Skill x) => x.SkillDefinition.Id == SwapContextualSkillEffect.SwapContextualSkillEffectDefinition.ContextualSkillIdToLock);
		if (skill != null)
		{
			SwapContextualSkillEffect.LockedSkillOverallUses = skill.OverallUses;
		}
		base.PerkEffect.APerkModule.Perk.Owner.ToggleContextualSkillLock(SwapContextualSkillEffect.SwapContextualSkillEffectDefinition.ContextualSkillIdToLock, locks: true, base.PerkEffect.APerkModule.Perk, SwapContextualSkillEffect.SwapContextualSkillEffectDefinition.OverallUses);
	}

	public override void Lock(bool onLoad)
	{
		base.PerkEffect.APerkModule.Perk.Owner.ToggleContextualSkillLock(SwapContextualSkillEffect.SwapContextualSkillEffectDefinition.ContextualSkillIdToLock, locks: false, base.PerkEffect.APerkModule.Perk, SwapContextualSkillEffect.LockedSkillOverallUses);
		base.PerkEffect.APerkModule.Perk.Owner.ToggleContextualSkillLock(SwapContextualSkillEffect.SwapContextualSkillEffectDefinition.ContextualSkillIdToUnlock, locks: true, base.PerkEffect.APerkModule.Perk, SwapContextualSkillEffect.SwapContextualSkillEffectDefinition.OverallUses);
	}
}
