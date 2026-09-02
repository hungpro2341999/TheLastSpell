using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class SwapContextualSkillEffect : APerkEffect
{
	public int LockedSkillOverallUses { get; set; } = -1;

	public SwapContextualSkillEffectDefinition SwapContextualSkillEffectDefinition => base.APerkEffectDefinition as SwapContextualSkillEffectDefinition;

	public SwapContextualSkillEffect(SwapContextualSkillEffectDefinition aPerkEffectDefinition, SwapContextualSkillEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
