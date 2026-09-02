using System.Collections.Generic;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class DynamicStatsModifierEffectController : APerkEffectController
{
	public DynamicStatsModifierEffect DynamicStatsModifierEffect => base.PerkEffect as DynamicStatsModifierEffect;

	public DynamicStatsModifierEffectController(DynamicStatsModifierEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new DynamicStatsModifierEffect(aPerkEffectDefinition as DynamicStatsModifierEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		foreach (UnitStatDefinition.E_Stat stat in DynamicStatsModifierEffect.DynamicStatsModifierEffectDefinition.Stats)
		{
			List<DynamicStatsModifierEffect> perkDynamicStatsModifierEffects = base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(stat).PerkDynamicStatsModifierEffects;
			if (!perkDynamicStatsModifierEffects.Contains(DynamicStatsModifierEffect))
			{
				perkDynamicStatsModifierEffects.Add(DynamicStatsModifierEffect);
			}
		}
	}

	public override void Lock(bool onLoad)
	{
		foreach (UnitStatDefinition.E_Stat stat in DynamicStatsModifierEffect.DynamicStatsModifierEffectDefinition.Stats)
		{
			List<DynamicStatsModifierEffect> perkDynamicStatsModifierEffects = base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(stat).PerkDynamicStatsModifierEffects;
			if (perkDynamicStatsModifierEffects.Contains(DynamicStatsModifierEffect))
			{
				perkDynamicStatsModifierEffects.Remove(DynamicStatsModifierEffect);
			}
		}
	}
}
