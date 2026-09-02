using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class ModifyContextualSkillOverallUsesEffect : APerkEffect
{
	public ModifyContextualSkillOverallUsesEffectDefinition ModifyContextualSkillOverallUsesEffectDefinition => base.APerkEffectDefinition as ModifyContextualSkillOverallUsesEffectDefinition;

	public ModifyContextualSkillOverallUsesEffect(ModifyContextualSkillOverallUsesEffectDefinition aPerkEffectDefinition, ModifyContextualSkillOverallUsesEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
