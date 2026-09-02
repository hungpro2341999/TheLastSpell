using System.Collections.Generic;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.Model.Unit.Stat;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class StatBoundaryModifierEffectController : APerkEffectController
{
	public StatBoundaryModifierEffect StatBoundaryModifierEffect => base.PerkEffect as StatBoundaryModifierEffect;

	public StatBoundaryModifierEffectController(StatBoundaryModifierEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new StatBoundaryModifierEffect(aPerkEffectDefinition as StatBoundaryModifierEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		PlayableUnitStat stat = base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(StatBoundaryModifierEffect.StatBoundaryModifierEffectDefinition.Stat);
		List<StatBoundaryModifierEffect> perkStatBoundaryModifierEffects = stat.PerkStatBoundaryModifierEffects;
		float final = stat.Final;
		float finalClamped = stat.FinalClamped;
		if (!perkStatBoundaryModifierEffects.Contains(StatBoundaryModifierEffect))
		{
			perkStatBoundaryModifierEffects.Add(StatBoundaryModifierEffect);
		}
		UnitStatDefinition.E_Stat childStatIfExists = UnitDatabase.UnitStatDefinitions[StatBoundaryModifierEffect.StatBoundaryModifierEffectDefinition.Stat].GetChildStatIfExists();
		if (childStatIfExists != UnitStatDefinition.E_Stat.Undefined)
		{
			PlayableUnitStat stat2 = StatBoundaryModifierEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(childStatIfExists);
			if (!stat2.PerkStatBoundaryModifierEffects.Contains(StatBoundaryModifierEffect))
			{
				stat2.PerkStatBoundaryModifierEffects.Add(StatBoundaryModifierEffect);
			}
			if (!onLoad && stat.PerkLocksBuffer == 0)
			{
				float num = final - finalClamped;
				if (num > 0f)
				{
					StatBoundaryModifierEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.IncreaseBaseStat(childStatIfExists, num, includeChildStat: false);
				}
			}
		}
		StatBoundaryModifierEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.SetBaseStat(stat.StatId, stat.Base);
	}

	public override void Lock(bool onLoad)
	{
		PlayableUnitStat stat = base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(StatBoundaryModifierEffect.StatBoundaryModifierEffectDefinition.Stat);
		List<StatBoundaryModifierEffect> perkStatBoundaryModifierEffects = stat.PerkStatBoundaryModifierEffects;
		if (perkStatBoundaryModifierEffects.Contains(StatBoundaryModifierEffect))
		{
			perkStatBoundaryModifierEffects.Remove(StatBoundaryModifierEffect);
		}
		UnitStatDefinition.E_Stat childStatIfExists = UnitDatabase.UnitStatDefinitions[StatBoundaryModifierEffect.StatBoundaryModifierEffectDefinition.Stat].GetChildStatIfExists();
		if (childStatIfExists != UnitStatDefinition.E_Stat.Undefined)
		{
			PlayableUnitStat stat2 = StatBoundaryModifierEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(childStatIfExists);
			perkStatBoundaryModifierEffects = StatBoundaryModifierEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(childStatIfExists).PerkStatBoundaryModifierEffects;
			if (perkStatBoundaryModifierEffects.Contains(StatBoundaryModifierEffect))
			{
				perkStatBoundaryModifierEffects.Remove(StatBoundaryModifierEffect);
			}
			StatBoundaryModifierEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.SetBaseStat(stat2.StatId, stat2.Base);
		}
		StatBoundaryModifierEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.SetBaseStat(stat.StatId, stat.Base);
	}
}
