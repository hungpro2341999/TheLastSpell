using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class AddSkillEffect : APerkEffect
{
	public AddSkillEffectDefinition AddSkillEffectDefinition => base.APerkEffectDefinition as AddSkillEffectDefinition;

	public AddSkillEffect(AddSkillEffectDefinition aPerkEffectDefinition, AddSkillEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
