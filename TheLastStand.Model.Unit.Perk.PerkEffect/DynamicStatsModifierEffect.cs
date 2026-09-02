using System;
using System.Collections.Generic;
using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.Model.Unit.Stat;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class DynamicStatsModifierEffect : APerkEffect
{
	public bool HasBeenUsed;

	public DynamicStatsModifierEffectDefinition DynamicStatsModifierEffectDefinition => base.APerkEffectDefinition as DynamicStatsModifierEffectDefinition;

	public float Value => DynamicStatsModifierEffectDefinition.ValueExpression.EvalToInt(base.APerkModule.Perk);

	public List<(float, UnitStatDefinition.E_Stat)> SortedDamageTypes
	{
		get
		{
			HasBeenUsed = true;
			List<(float, UnitStatDefinition.E_Stat)> obj = new List<(float, UnitStatDefinition.E_Stat)>(3)
			{
				(base.APerkModule.Perk.Owner.PhysicalDamage, UnitStatDefinition.E_Stat.PhysicalDamage),
				(base.APerkModule.Perk.Owner.MagicalDamage, UnitStatDefinition.E_Stat.MagicalDamage),
				(base.APerkModule.Perk.Owner.RangedDamage, UnitStatDefinition.E_Stat.RangedDamage)
			};
			HasBeenUsed = false;
			obj.Sort(((float, UnitStatDefinition.E_Stat) a, (float, UnitStatDefinition.E_Stat) b) => (a.Item1 != b.Item1) ? b.Item1.CompareTo(a.Item1) : Array.IndexOf(PlayableUnitStat.damageTypesByPriority, a.Item2).CompareTo(Array.IndexOf(PlayableUnitStat.damageTypesByPriority, b.Item2)));
			return obj;
		}
	}

	public UnitStatDefinition.E_Stat HighestDamageType => SortedDamageTypes[0].Item2;

	public UnitStatDefinition.E_Stat FirstOtherDamageType => SortedDamageTypes[1].Item2;

	public UnitStatDefinition.E_Stat SecondOtherDamageType => SortedDamageTypes[2].Item2;

	public float HighestDamageValue => SortedDamageTypes[0].Item1;

	public float FirstOtherDamageValue => SortedDamageTypes[1].Item1;

	public float SecondOtherDamageValue => SortedDamageTypes[2].Item1;

	public DynamicStatsModifierEffect(DynamicStatsModifierEffectDefinition aPerkEffectDefinition, DynamicStatsModifierEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}

	public PlayableUnitStat GetHighestStat()
	{
		PlayableUnitStat result = null;
		float num = float.MinValue;
		HasBeenUsed = true;
		foreach (UnitStatDefinition.E_Stat stat2 in DynamicStatsModifierEffectDefinition.Stats)
		{
			PlayableUnitStat stat = base.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(stat2);
			if (stat.FinalClamped > num)
			{
				result = stat;
				num = stat.FinalClamped;
			}
		}
		HasBeenUsed = false;
		return result;
	}
}
