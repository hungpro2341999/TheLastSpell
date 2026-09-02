using System.Collections.Generic;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class LockSkillEffectController : APerkEffectController
{
	public LockSkillEffect LockSkillEffect => base.PerkEffect as LockSkillEffect;

	public LockSkillEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new LockSkillEffect(aPerkEffectDefinition as LockSkillEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		string skillId = LockSkillEffect.LockSkillEffectDefinition.SkillId;
		Dictionary<string, int> skillLocksBuffers = LockSkillEffect.APerkModule.Perk.Owner.SkillLocksBuffers;
		if (skillLocksBuffers.ContainsKey(skillId))
		{
			skillLocksBuffers[skillId]++;
		}
		else
		{
			skillLocksBuffers[skillId] = 1;
		}
	}

	public override void Lock(bool onLoad)
	{
		string skillId = LockSkillEffect.LockSkillEffectDefinition.SkillId;
		Dictionary<string, int> skillLocksBuffers = LockSkillEffect.APerkModule.Perk.Owner.SkillLocksBuffers;
		if (skillLocksBuffers.ContainsKey(skillId))
		{
			skillLocksBuffers[skillId]--;
		}
	}
}
