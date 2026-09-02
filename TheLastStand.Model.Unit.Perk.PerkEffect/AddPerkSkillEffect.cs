using TheLastStand.Controller.Skill;
using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class AddPerkSkillEffect : APerkEffect
{
	public AddPerkSkillEffectDefinition AddPerkSkillEffectDefinition => base.APerkEffectDefinition as AddPerkSkillEffectDefinition;

	public TheLastStand.Model.Skill.Skill Skill { get; }

	public AddPerkSkillEffect(AddPerkSkillEffectDefinition addPerkSkillEffectDefinition, AddPerkSkillEffectController addPerkSkillEffectController, APerkModule aPerkModule)
		: base(addPerkSkillEffectDefinition, addPerkSkillEffectController, aPerkModule)
	{
		Skill = new SkillController(addPerkSkillEffectDefinition.SkillDefinition, base.APerkModule.Perk.Owner, addPerkSkillEffectDefinition.UsesPerNight, addPerkSkillEffectDefinition.UsesPerTurn).Skill;
	}
}
