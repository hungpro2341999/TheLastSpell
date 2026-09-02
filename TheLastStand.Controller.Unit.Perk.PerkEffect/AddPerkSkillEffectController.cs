using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class AddPerkSkillEffectController : APerkEffectController
{
	public AddPerkSkillEffect AddPerkSkillEffect => base.PerkEffect as AddPerkSkillEffect;

	public AddPerkSkillEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new AddPerkSkillEffect(aPerkEffectDefinition as AddPerkSkillEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		AddPerkSkillEffect.APerkModule.Perk.Owner.NativeSkills.Add(AddPerkSkillEffect.Skill);
	}

	public override void Lock(bool onLoad)
	{
		AddPerkSkillEffect.APerkModule.Perk.Owner.NativeSkills.Remove(AddPerkSkillEffect.Skill);
	}
}
