using TMPro;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.View.Unit.Injury;

public class InjuriedStatDisplay : MonoBehaviour
{
	[SerializeField]
	private bool canShowAsPercentage = true;

	[SerializeField]
	private TextMeshProUGUI valueText;

	private string localizedStat = string.Empty;

	private string valueString = string.Empty;

	public void RefreshAsModifier(float value, UnitStatDefinition.E_Stat stat, bool statIsPercentage)
	{
		valueText.text = InjuryDefinition.GetFormatedStatModifierInjury(value, stat, statIsPercentage && canShowAsPercentage);
	}

	public void RefreshAsMultiplier(InjuryDefinition.E_ValueMultiplier multiplier, UnitStatDefinition.E_Stat stat, float currentLoss, bool statIsPercentage)
	{
		valueText.text = InjuryDefinition.GetFormatedStatMultiplierInjury(multiplier, stat, currentLoss, statIsPercentage && canShowAsPercentage);
	}

	public void RefreshAsPreventedSkill(string skillId)
	{
		valueText.text = InjuryDefinition.GetFormatedPreventedSkillInjury(skillId);
	}

	public void RefreshAsRemovedStatus(RemoveStatusDefinition removeStatusDefinition)
	{
		string skillEffectName = SkillManager.GetSkillEffectName("Dispel");
		valueString = "<style=Dispel>" + skillEffectName + "</style>";
		string removedStatusIconName = RemoveStatusDefinition.GetRemovedStatusIconName(removeStatusDefinition.Status);
		if (!string.IsNullOrEmpty(removedStatusIconName))
		{
			valueString = valueString + " <size=16><sprite name=\"" + removedStatusIconName + "\"></size>";
		}
		valueText.text = valueString;
	}

	public void RefreshAsStatus(StatusEffectDefinition statusDefinition)
	{
		string text = $"({AtlasIcons.TimeIcon} {statusDefinition.TurnsCount})";
		if (statusDefinition is StatModifierEffectDefinition { Stat: var stat } statModifierEffectDefinition)
		{
			valueString = $"{statModifierEffectDefinition.GetModifierValue()}";
			valueString = ((statModifierEffectDefinition is BuffEffectDefinition) ? "+" : "-") + valueString;
			valueString += ((stat.ShownAsPercentage() && canShowAsPercentage) ? "<size=80%>%</size>" : string.Empty);
			valueString = "<style=" + ((statModifierEffectDefinition is BuffEffectDefinition) ? "GoodNb" : "BadNb") + ">" + valueString + "</style>";
			localizedStat = ((stat != UnitStatDefinition.E_Stat.Undefined) ? Localizer.Get(string.Format("{0}{1}", "UnitStat_Name_", stat)) : string.Empty);
			UnitStatDefinition stat2 = UnitDatabase.UnitStatDefinitions[stat];
			valueText.text = $"{valueString} <style={stat2.GetChildStatIfExists()}>{localizedStat}</style> {text}";
		}
		else if (statusDefinition is PoisonEffectDefinition poisonEffectDefinition)
		{
			valueString = "<style=Poison>" + Localizer.Get("SkillEffectName_Poison") + "</style>";
			valueText.text = $"{valueString} <color=red>({poisonEffectDefinition.DamagePerTurn})</color> {text}";
		}
		else if (statusDefinition is StunEffectDefinition)
		{
			valueString = "<style=Stun>" + Localizer.Get("SkillEffectName_Stun") + "</style>";
			valueText.text = valueString + " " + text;
		}
		else if (statusDefinition is ChargedEffectDefinition)
		{
			valueString = "<style=Charged>" + Localizer.Get("SkillEffectName_Charged") + "</style>";
			valueText.text = valueString + " " + text;
		}
		else if (statusDefinition is ImmuneToNegativeStatusEffectDefinition immuneToNegativeStatusEffectDefinition)
		{
			string immunityStyleId = ImmuneToNegativeStatusEffectDefinition.GetImmunityStyleId(immuneToNegativeStatusEffectDefinition.StatusImmunity);
			valueString = "<style=" + immunityStyleId + ">" + Localizer.Get("SkillEffectName_NegativeStatusImmunityEffect") + "</style>";
			valueText.text = valueString + " " + text;
		}
	}

	public void Display(bool show)
	{
		base.gameObject.SetActive(show);
	}
}
