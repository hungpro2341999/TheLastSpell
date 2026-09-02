using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class InjuryDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_ValueMultiplier
	{
		None,
		ThreeQuarter,
		TwoThird,
		Half,
		Third,
		Quarter,
		Zero
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ValueMultiplierComparer : IEqualityComparer<E_ValueMultiplier>
	{
		public bool Equals(E_ValueMultiplier x, E_ValueMultiplier y)
		{
			return x == y;
		}

		public int GetHashCode(E_ValueMultiplier obj)
		{
			return (int)obj;
		}
	}

	public static readonly Dictionary<E_ValueMultiplier, float> Multipliers = new Dictionary<E_ValueMultiplier, float>(default(ValueMultiplierComparer))
	{
		{
			E_ValueMultiplier.None,
			1f
		},
		{
			E_ValueMultiplier.ThreeQuarter,
			0.75f
		},
		{
			E_ValueMultiplier.TwoThird,
			0.66f
		},
		{
			E_ValueMultiplier.Half,
			0.5f
		},
		{
			E_ValueMultiplier.Third,
			0.33f
		},
		{
			E_ValueMultiplier.Quarter,
			0.25f
		},
		{
			E_ValueMultiplier.Zero,
			0f
		}
	};

	public float BaseHealth { get; private set; }

	public float BaseRatio { get; private set; }

	public float BaseThreshold => BaseHealth * BaseRatio;

	public Dictionary<string, List<SkillDefinition>> PreventedSkillsByGroupId { get; private set; } = new Dictionary<string, List<SkillDefinition>>();

	public List<string> PreventedSkillsIds { get; private set; } = new List<string>();

	public float RatioMultiplier { get; private set; }

	public List<RemoveStatusDefinition> RemoveStatusDefinitions { get; private set; } = new List<RemoveStatusDefinition>();

	public Dictionary<UnitStatDefinition.E_Stat, float> StatModifiers { get; private set; } = new Dictionary<UnitStatDefinition.E_Stat, float>(UnitStatDefinition.SharedStatComparer);

	public Dictionary<UnitStatDefinition.E_Stat, E_ValueMultiplier> StatMultipliers { get; private set; } = new Dictionary<UnitStatDefinition.E_Stat, E_ValueMultiplier>(UnitStatDefinition.SharedStatComparer);

	public List<StatusEffectDefinition> Statuses { get; private set; } = new List<StatusEffectDefinition>();

	public InjuryDefinition(XContainer container, float baseHealth)
		: base(container)
	{
		BaseHealth = baseHealth;
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("BaseRatio");
		if (xElement2.IsNullOrEmpty())
		{
			CLoggerManager.Log("Injury definition must have a BaseRatio element.", LogType.Error);
			return;
		}
		if (!float.TryParse(xElement2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			CLoggerManager.Log("Could not parse Injury definition BaseRatio element value " + xElement2.Value + " to a float value.", LogType.Error);
			return;
		}
		BaseRatio = result;
		XElement xElement3 = xElement.Element("RatioMultiplier");
		if (xElement3.IsNullOrEmpty())
		{
			CLoggerManager.Log("Injury definition must have a RatioMultiplier element.", LogType.Error);
			return;
		}
		if (!float.TryParse(xElement3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			CLoggerManager.Log("Could not parse Injury definition RatioMultiplier element value " + xElement3.Value + " to a float value.", LogType.Error);
			return;
		}
		RatioMultiplier = result2;
		XElement xElement4 = xElement.Element("StatModifiers");
		if (xElement4 != null)
		{
			foreach (XElement item9 in xElement4.Elements("StatModifier"))
			{
				XAttribute xAttribute = item9.Attribute("Id");
				if (xAttribute.IsNullOrEmpty())
				{
					CLoggerManager.Log("StatModifier must have an Id attribute.", LogType.Error);
					continue;
				}
				if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute.Value, out var result3))
				{
					CLoggerManager.Log($"StatModifier attribute Id {result3} must be a valid UnitStatDefinition.E_Stat value.", LogType.Error);
					continue;
				}
				XAttribute xAttribute2 = item9.Attribute("Offset");
				XAttribute xAttribute3 = item9.Attribute("Multiplier");
				E_ValueMultiplier result5;
				if (xAttribute2 != null)
				{
					float result4;
					if (xAttribute2.IsNullOrEmpty())
					{
						CLoggerManager.Log("StatModifier must have an Offset attribute.", LogType.Error);
					}
					else if (!float.TryParse(xAttribute2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result4))
					{
						CLoggerManager.Log("Could not parse Injury definition StatModifier Offset attribute value " + xAttribute2.Value + " to a float value.", LogType.Error);
					}
					else
					{
						StatModifiers.Add(result3, result4);
					}
				}
				else if (xAttribute3.IsNullOrEmpty())
				{
					CLoggerManager.Log("StatModifier must have a Multiplier attribute.", LogType.Error);
				}
				else if (!Enum.TryParse<E_ValueMultiplier>(xAttribute3.Value, out result5))
				{
					CLoggerManager.Log("Could not parse Injury definition StatModifier Multiplier attribute value " + xAttribute3.Value + " to a " + typeof(E_ValueMultiplier).Name + " value.", LogType.Error);
				}
				else
				{
					StatMultipliers.Add(result3, result5);
				}
			}
		}
		XElement xElement5 = xElement.Element("PreventSkills");
		if (xElement5 != null)
		{
			foreach (XElement item10 in xElement5.Elements("SkillId"))
			{
				PreventedSkillsIds.Add(item10.Value);
			}
			ComputePreventedSkillsByGroupId();
		}
		foreach (XElement item11 in xElement.Elements("Buff"))
		{
			BuffEffectDefinition item = new BuffEffectDefinition(item11);
			Statuses.Add(item);
		}
		foreach (XElement item12 in xElement.Elements("Debuff"))
		{
			DebuffEffectDefinition item2 = new DebuffEffectDefinition(item12);
			Statuses.Add(item2);
		}
		XElement xElement6 = xElement.Element("Stun");
		if (xElement6 != null)
		{
			StunEffectDefinition item3 = new StunEffectDefinition(xElement6);
			Statuses.Add(item3);
		}
		XElement xElement7 = xElement.Element("Poison");
		if (xElement7 != null)
		{
			PoisonEffectDefinition item4 = new PoisonEffectDefinition(xElement7);
			Statuses.Add(item4);
		}
		XElement xElement8 = xElement.Element("Charged");
		if (xElement8 != null)
		{
			ChargedEffectDefinition item5 = new ChargedEffectDefinition(xElement8);
			Statuses.Add(item5);
		}
		XElement xElement9 = xElement.Element("NegativeStatusImmunity");
		if (xElement9 != null)
		{
			ImmuneToNegativeStatusEffectDefinition item6 = new ImmuneToNegativeStatusEffectDefinition(xElement9);
			Statuses.Add(item6);
		}
		XElement xElement10 = xElement.Element("Contagion");
		if (xElement10 != null)
		{
			ContagionEffectDefinition item7 = new ContagionEffectDefinition(xElement10);
			Statuses.Add(item7);
		}
		foreach (XElement item13 in xElement.Elements("RemoveStatus"))
		{
			RemoveStatusDefinition item8 = new RemoveStatusDefinition(item13);
			RemoveStatusDefinitions.Add(item8);
		}
	}

	public static string GetFormatedStatModifierInjury(float value, UnitStatDefinition.E_Stat stat, bool statIsPercentage)
	{
		string text = $"{value}";
		if (value >= 0f)
		{
			text = "+" + text;
		}
		text += (statIsPercentage ? "<size=110%>%</size>" : string.Empty);
		text = "<style=" + ((value >= 0f) ? "GoodNb" : "BadNb") + ">" + text + "</style>";
		string arg = ((stat != UnitStatDefinition.E_Stat.Undefined) ? Localizer.Get(string.Format("{0}{1}", "UnitStat_Name_", stat)) : string.Empty);
		UnitStatDefinition stat2 = UnitDatabase.UnitStatDefinitions[stat];
		return $"{text} <style={stat2.GetChildStatIfExists()}>{arg}</style>";
	}

	public static string GetFormatedStatMultiplierInjury(E_ValueMultiplier multiplier, UnitStatDefinition.E_Stat stat, float currentLoss, bool statIsPercentage)
	{
		string arg = ((currentLoss != 0f) ? string.Format("(<style=BadNb>{0}{1}</style>)", currentLoss, statIsPercentage ? "<size=110%>%</size>" : string.Empty) : string.Empty);
		string arg2 = ((stat != UnitStatDefinition.E_Stat.Undefined) ? Localizer.Get(string.Format("{0}{1}", "UnitStat_Name_", stat)) : string.Empty);
		UnitStatDefinition stat2 = UnitDatabase.UnitStatDefinitions[stat];
		return string.Format(Localizer.Get(string.Format("{0}{1}", "Injury_Multiplier_", multiplier)), $"<style={stat2.GetChildStatIfExists()}>{arg2}</style>", arg);
	}

	public static string GetFormatedPreventedSkillInjury(string skillId)
	{
		return string.Format(Localizer.Get("Injury_PreventSkill_InjuryTooltip"), "<style=Skill>" + Localizer.Get("SkillName_" + skillId) + "</style>");
	}

	public static string GetFormatedStatusInjury(StatusEffectDefinition statusDefinition, bool canShowAsPercentage)
	{
		string result = string.Empty;
		if (statusDefinition is StatModifierEffectDefinition { Stat: var stat } statModifierEffectDefinition)
		{
			result = GetFormatedBuffStatusInjury(statModifierEffectDefinition.GetModifierValue(), stat, statusDefinition.TurnsCount, statModifierEffectDefinition is BuffEffectDefinition, canShowAsPercentage);
		}
		else if (statusDefinition is PoisonEffectDefinition poisonEffectDefinition)
		{
			result = GetFormatedPoisonStatusInjury(poisonEffectDefinition.TurnsCount, poisonEffectDefinition.DamagePerTurn);
		}
		else if (statusDefinition is StunEffectDefinition)
		{
			result = GetFormatedStunStatusInjury(statusDefinition.TurnsCount);
		}
		else if (statusDefinition is ContagionEffectDefinition)
		{
			result = GetFormatedContagionStatusInjury(statusDefinition.TurnsCount);
		}
		return result;
	}

	public static string GetFormatedBuffStatusInjury(float value, UnitStatDefinition.E_Stat stat, int turnsCount, bool isBuff, bool canShowAsPercentage)
	{
		string empty = string.Empty;
		empty = $"{value}";
		if (value > 0f && isBuff)
		{
			empty = "+" + empty;
		}
		else if (value > 0f)
		{
			empty = "-" + empty;
		}
		empty += ((stat.ShownAsPercentage() && canShowAsPercentage) ? "<size=80%>%</size>" : string.Empty);
		empty = "<style=" + (isBuff ? "GoodNb" : "BadNb") + ">" + empty + "</style>";
		string text = ((stat != UnitStatDefinition.E_Stat.Undefined) ? Localizer.Get(string.Format("{0}{1}", "UnitStat_Name_", stat)) : string.Empty);
		UnitStatDefinition stat2 = UnitDatabase.UnitStatDefinitions[stat];
		if (turnsCount == -1)
		{
			return $"{empty} <style={stat2.GetChildStatIfExists()}>{text}</style> ({AtlasIcons.TimeIcon} {AtlasIcons.InfiniteIcon})";
		}
		return $"{empty} <style={stat2.GetChildStatIfExists()}>{text}</style> ({AtlasIcons.TimeIcon} {turnsCount})";
	}

	public static string GetFormatedContagionStatusInjury(int turnsCount)
	{
		string text = "<style=Contagion>" + Localizer.Get("SkillEffectName_Contagion") + "</style>";
		if (turnsCount == -1)
		{
			return text + " (" + AtlasIcons.TimeIcon + " " + AtlasIcons.InfiniteIcon + ")";
		}
		return $"{text} ({AtlasIcons.TimeIcon} {turnsCount})";
	}

	public static string GetFormatedPoisonStatusInjury(int turnsCount, float damagePerTurn)
	{
		string text = "<style=Poison>" + Localizer.Get("SkillEffectName_Poison") + "</style>";
		if (turnsCount == -1)
		{
			return $"{text} <color=red>({damagePerTurn})</color> ({AtlasIcons.TimeIcon} {AtlasIcons.InfiniteIcon})";
		}
		return $"{text} <color=red>({damagePerTurn})</color> ({AtlasIcons.TimeIcon} {turnsCount})";
	}

	public static string GetFormatedStunStatusInjury(int turnsCount)
	{
		string text = "<style=Stun>" + Localizer.Get("SkillEffectName_Stun") + "</style>";
		if (turnsCount == -1)
		{
			return text + " (" + AtlasIcons.TimeIcon + " " + AtlasIcons.InfiniteIcon + ")";
		}
		return $"{text} ({AtlasIcons.TimeIcon} {turnsCount})";
	}

	private void ComputePreventedSkillsByGroupId()
	{
		foreach (string preventedSkillsId in PreventedSkillsIds)
		{
			if (SkillDatabase.SkillDefinitions.TryGetValue(preventedSkillsId, out var value))
			{
				if (!PreventedSkillsByGroupId.ContainsKey(value.GroupId))
				{
					PreventedSkillsByGroupId.Add(value.GroupId, new List<SkillDefinition> { value });
				}
				else
				{
					PreventedSkillsByGroupId[value.GroupId].Add(value);
				}
			}
		}
	}
}
