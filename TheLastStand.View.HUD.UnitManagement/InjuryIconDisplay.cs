using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class InjuryIconDisplay : UnitModifiersIconDisplay
{
	[SerializeField]
	private Image statusIcon;

	[SerializeField]
	private DataSpriteDictionary statusAndWoundIcons;

	private TheLastStand.Model.Unit.Unit unit;

	private int injuryStage;

	public void Refresh(TheLastStand.Model.Unit.Unit unit)
	{
		this.unit = unit;
		injuryStage = unit.UnitStatsController.UnitStats.InjuryStage;
		statusIcon.sprite = statusAndWoundIcons.GetSpriteById("Injury" + (injuryStage - 1));
	}

	protected override void DiplayTooltip()
	{
		UIManager.UnitHUDStatusesTooltip.TitleLocalized = GetTitle();
		UIManager.UnitHUDStatusesTooltip.LinesLocalized = GetLines();
		UIManager.UnitHUDStatusesTooltip.DescriptionLocalized = Localizer.Get(string.Format("GameConceptDescription_{0}", "Wounds"));
		UIManager.UnitHUDStatusesTooltip.UseInjuryBox = true;
		UIManager.UnitHUDStatusesTooltip.FollowElement.ChangeTarget(base.transform);
		UIManager.UnitHUDStatusesTooltip.Display();
	}

	private string GetTitle()
	{
		return "<color=white>" + Localizer.Get("SkillTooltip_Injuried_" + injuryStage) + "</color>";
	}

	private string GetLines()
	{
		string text = string.Empty;
		foreach (UnitStatDefinition.E_Stat statsKey in unit.UnitStatsController.UnitStats.StatsKeys)
		{
			UnitStat stat = unit.UnitStatsController.GetStat(statsKey);
			if (stat.Injuries != 0f)
			{
				text += InjuryDefinition.GetFormatedStatModifierInjury(stat.Injuries, statsKey, UnitDatabase.UnitStatDefinitions[statsKey].Id.ShownAsPercentage());
				text += "\r\n";
			}
			if (stat.InjuriesValueMultiplier != InjuryDefinition.E_ValueMultiplier.None)
			{
				_ = stat.InjuryMultiplierLoss;
				text += InjuryDefinition.GetFormatedStatMultiplierInjury(stat.InjuriesValueMultiplier, statsKey, stat.Base - stat.BaseWithInjuryMultiplier, UnitDatabase.UnitStatDefinitions[statsKey].Id.ShownAsPercentage());
				text += "\r\n";
			}
		}
		foreach (string preventedSkillsLocalizationId in unit.GetPreventedSkillsLocalizationIds())
		{
			text += InjuryDefinition.GetFormatedPreventedSkillInjury(preventedSkillsLocalizationId);
			text += "\r\n";
		}
		for (int i = 0; i < unit.StatusList.Count; i++)
		{
			if (unit.StatusList[i].IsFromInjury)
			{
				if (unit.StatusList[i] is StatModifierStatus statModifierStatus)
				{
					text += InjuryDefinition.GetFormatedBuffStatusInjury(statModifierStatus.ModifierValue, statModifierStatus.Stat, statModifierStatus.RemainingTurnsCount, statModifierStatus is BuffStatus, canShowAsPercentage: true);
					text += "\r\n";
				}
				else if (unit.StatusList[i] is PoisonStatus poisonStatus)
				{
					text += InjuryDefinition.GetFormatedPoisonStatusInjury(poisonStatus.RemainingTurnsCount, poisonStatus.DamagePerTurn);
					text += "\r\n";
				}
				else if (unit.StatusList[i] is StunStatus stunStatus)
				{
					text += InjuryDefinition.GetFormatedStunStatusInjury(stunStatus.RemainingTurnsCount);
					text += "\r\n";
				}
				else if (unit.StatusList[i] is ContagionStatus contagionStatus)
				{
					text += InjuryDefinition.GetFormatedContagionStatusInjury(contagionStatus.RemainingTurnsCount);
					text += "\r\n";
				}
			}
		}
		return text;
	}

	protected override void HideTooltip()
	{
		UIManager.UnitHUDStatusesTooltip.Hide();
	}
}
