using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class ReplaceItemSkillEffect : APerkEffect
{
	public ReplaceItemSkillEffectDefinition ReplaceItemSkillEffectDefinition => base.APerkEffectDefinition as ReplaceItemSkillEffectDefinition;

	public ReplaceItemSkillEffect(ReplaceItemSkillEffectDefinition aPerkEffectDefinition, ReplaceItemSkillEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
