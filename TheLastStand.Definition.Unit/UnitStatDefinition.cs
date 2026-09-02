using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Definition.Tooltip.Compendium;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

[Serializable]
public class UnitStatDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_Stat
	{
		Undefined = -1,
		Armor,
		ArmorTotal,
		Health,
		HealthTotal,
		Mana,
		ManaTotal,
		ActionPoints,
		ActionPointsTotal,
		Block,
		Critical,
		Dodge,
		Resistance,
		PhysicalDamage,
		MagicalDamage,
		InjuryDamageMultiplier,
		[Obsolete("Not used anymore", true)]
		Fate,
		HealthRegen,
		ManaRegen,
		MovePoints,
		MovePointsTotal,
		RangedDamage,
		PropagationDamage,
		CriticalPower,
		HealingReceived,
		SkillRangeModifier,
		ExperienceGainMultiplier,
		OverallDamage,
		Reliability,
		Accuracy,
		StunChanceModifier,
		PropagationBouncesModifier,
		ResistanceReduction,
		MomentumAttacks,
		OpportunisticAttacks,
		IsolatedAttacks,
		ArmorShreddingAttacks,
		PoisonDamageModifier,
		Panic,
		MultiHitsCountModifier,
		StunResistance,
		DamnedSoulsEarned,
		ExperienceGain,
		PoisonDurationModifier,
		DebuffDurationModifier,
		BuffDurationModifier,
		PotionRangeModifier,
		MagicDamageReductionModifier,
		PercentageResistanceReduction,
		BonusSkillUses,
		BonusUsableItemsUses,
		StunDurationModifier,
		ContagionDurationModifier,
		EnemyEvolutionDamageMultiplier,
		GauntletRangeModifier,
		BonusMainHandSkillUses
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct StatComparer : IEqualityComparer<E_Stat>
	{
		public bool Equals(E_Stat x, E_Stat y)
		{
			return x == y;
		}

		public int GetHashCode(E_Stat obj)
		{
			return (int)obj;
		}
	}

	public static readonly StatComparer SharedStatComparer = default(StatComparer);

	public static Dictionary<E_Stat, bool> ShownAsPercentageDictionary = new Dictionary<E_Stat, bool>(SharedStatComparer);

	public Dictionary<DamageableType, Vector2> Boundaries { get; private set; } = new Dictionary<DamageableType, Vector2>(3, UnitTemplateDefinition.SharedUnitTypeComparer);

	public E_Stat ChildStatId { get; private set; } = E_Stat.Undefined;

	public HashSet<CompendiumEntryDefinition> CompendiumEntries { get; private set; } = new HashSet<CompendiumEntryDefinition>();

	public string Description => Localizer.Get(string.Format("{0}{1}", "UnitStat_Desc_", Id));

	public E_Stat Id { get; private set; }

	public string Name => Localizer.Get(string.Format("{0}{1}", "UnitStat_Name_", Id));

	public E_Stat ParentStatId { get; private set; } = E_Stat.Undefined;

	public string ShortName => Localizer.Get(string.Format("{0}{1}", "UnitStat_ShortName_", Id));

	public UnitStatDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			CLoggerManager.Log("The unit stat has no Id!", LogType.Error);
			return;
		}
		Id = (E_Stat)Enum.Parse(typeof(E_Stat), xAttribute.Value);
		XAttribute xAttribute2 = xElement.Attribute("ShownAsPercentage");
		if (xAttribute2 != null)
		{
			if (int.TryParse(xAttribute2.Value, out var result))
			{
				ShownAsPercentageDictionary.Add(Id, result == 1);
			}
		}
		else
		{
			ShownAsPercentageDictionary.Add(Id, value: false);
		}
		XAttribute xAttribute3 = xElement.Attribute("ParentStat");
		if (xAttribute3 != null)
		{
			if (!Enum.TryParse<E_Stat>(xAttribute3.Value, out var result2))
			{
				CLoggerManager.Log($"Could not parse {Id} ParentStat attribute value {xAttribute3.Value} to a valid stat!", LogType.Error);
				return;
			}
			ParentStatId = result2;
		}
		XAttribute xAttribute4 = xElement.Attribute("ChildStat");
		if (xAttribute4 != null)
		{
			if (!Enum.TryParse<E_Stat>(xAttribute4.Value, out var result3))
			{
				CLoggerManager.Log($"Could not parse {Id} ChildStat attribute value {xAttribute4.Value} to a valid stat!", LogType.Error);
				return;
			}
			ChildStatId = result3;
		}
		foreach (XElement item in xElement.Elements("Boundaries"))
		{
			XAttribute xAttribute5 = item.Attribute("UnitType");
			DamageableType key = (DamageableType)Enum.Parse(typeof(DamageableType), xAttribute5.Value);
			Vector2 value = new Vector2(int.Parse(item.Element("Min").Value), int.Parse(item.Element("Max").Value));
			Boundaries.Add(key, value);
		}
		CompendiumEntries = new HashSet<CompendiumEntryDefinition>();
		XElement xElement2 = xElement.Element("CompendiumEntries");
		if (xElement2 == null)
		{
			return;
		}
		foreach (XElement item2 in xElement2.Elements("CompendiumEntry"))
		{
			CompendiumEntries.Add(new CompendiumEntryDefinition(item2));
		}
	}
}
