using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class MomentumModifierEffectController : APerkEffectController
{
	public MomentumModifierEffect MomentumModifierEffect => base.PerkEffect as MomentumModifierEffect;

	public MomentumModifierEffectController(MomentumModifierEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new MomentumModifierEffect(aPerkEffectDefinition as MomentumModifierEffectDefinition, this, aPerkModule);
	}

	public override void Trigger(PerkDataContainer data)
	{
		base.Trigger(data);
		int num = Mathf.RoundToInt(MomentumModifierEffect.MomentumModifierEffectDefinition.ValueExpression.EvalToFloat(base.PerkEffect.APerkModule.Perk));
		base.PerkEffect.APerkModule.Perk.Owner.MomentumTilesActive += num;
	}
}
