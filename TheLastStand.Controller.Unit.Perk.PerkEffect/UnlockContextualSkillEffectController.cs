using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class UnlockContextualSkillEffectController : APerkEffectController
{
	public UnlockContextualSkillEffect UnlockContextualSkillEffect => base.PerkEffect as UnlockContextualSkillEffect;

	public UnlockContextualSkillEffectController(UnlockContextualSkillEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new UnlockContextualSkillEffect(aPerkEffectDefinition as UnlockContextualSkillEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		base.PerkEffect.APerkModule.Perk.Owner.ToggleContextualSkillLock(UnlockContextualSkillEffect.UnlockContextualSkillEffectDefinition.ContextualSkillId, locks: false, null, UnlockContextualSkillEffect.UnlockContextualSkillEffectDefinition.GetOverallUses(base.PerkEffect.APerkModule.Perk));
	}

	public override void Lock(bool onLoad)
	{
		base.PerkEffect.APerkModule.Perk.Owner.ToggleContextualSkillLock(UnlockContextualSkillEffect.UnlockContextualSkillEffectDefinition.ContextualSkillId, locks: true, null, UnlockContextualSkillEffect.UnlockContextualSkillEffectDefinition.GetOverallUses(base.PerkEffect.APerkModule.Perk));
	}
}
