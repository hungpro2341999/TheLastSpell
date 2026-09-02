using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class AddSkillEffectController : APerkEffectController
{
	public AddSkillEffect AddSkillEffect => base.PerkEffect as AddSkillEffect;

	public AddSkillEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new AddSkillEffect(aPerkEffectDefinition as AddSkillEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		if (!base.PerkEffect.APerkModule.Perk.Owner.PerkAddedSkillEffects.Contains(AddSkillEffect))
		{
			base.PerkEffect.APerkModule.Perk.Owner.PerkAddedSkillEffects.Add(AddSkillEffect);
		}
	}

	public override void Lock(bool onLoad)
	{
		base.PerkEffect.APerkModule.Perk.Owner.PerkAddedSkillEffects.Remove(AddSkillEffect);
	}
}
