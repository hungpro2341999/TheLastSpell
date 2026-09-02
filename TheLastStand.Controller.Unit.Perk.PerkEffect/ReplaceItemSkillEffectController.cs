using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class ReplaceItemSkillEffectController : APerkEffectController
{
	public ReplaceItemSkillEffect ReplaceItemSkillEffect => base.PerkEffect as ReplaceItemSkillEffect;

	public ReplaceItemSkillEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new ReplaceItemSkillEffect(aPerkEffectDefinition as ReplaceItemSkillEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		if (!base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitPerksController.PlayableUnitPerks.ReplaceItemSkillEffects.Contains(ReplaceItemSkillEffect))
		{
			base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitPerksController.PlayableUnitPerks.ReplaceItemSkillEffects.Add(ReplaceItemSkillEffect);
		}
	}

	public override void Lock(bool onLoad)
	{
		base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitPerksController.PlayableUnitPerks.ReplaceItemSkillEffects.Remove(ReplaceItemSkillEffect);
	}
}
