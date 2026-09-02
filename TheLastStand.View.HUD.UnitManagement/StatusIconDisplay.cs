using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class StatusIconDisplay : UnitModifiersIconDisplay
{
	[SerializeField]
	private Image statusIcon;

	[SerializeField]
	private TextMeshProUGUI valueDisplay;

	[SerializeField]
	private DataSpriteDictionary statusAndWoundIcons;

	[SerializeField]
	private List<LocalizedFont> localizedFonts;

	private Status[] statuses;

	private Status.E_StatusType statusType;

	public List<LocalizedFont> LocalizedFonts => localizedFonts;

	public void Refresh(Status.E_StatusType statusType, params Status[] statuses)
	{
		this.statuses = statuses;
		this.statusType = statusType;
		statusIcon.sprite = statusAndWoundIcons.GetSpriteById(statusType.ToString());
		if (statusType == Status.E_StatusType.Poison)
		{
			float num = this.statuses.Sum((Status o) => (o as PoisonStatus).DamagePerTurn);
			valueDisplay.text = num.ToString();
		}
		else
		{
			valueDisplay.text = string.Empty;
		}
	}

	protected override void DiplayTooltip()
	{
		UIManager.UnitHUDStatusesTooltip.TitleLocalized = GetTitle();
		UIManager.UnitHUDStatusesTooltip.LinesLocalized = GetLines();
		string text = ((statusType == Status.E_StatusType.AllNegativeImmunity) ? "NegativeStatusImmunityEffect" : statusType.ToString());
		UIManager.UnitHUDStatusesTooltip.DescriptionLocalized = Localizer.Get("SkillEffectDescription_" + text);
		UIManager.UnitHUDStatusesTooltip.UseInjuryBox = false;
		UIManager.UnitHUDStatusesTooltip.FollowElement.ChangeTarget(base.transform);
		UIManager.UnitHUDStatusesTooltip.Display();
	}

	protected override void HideTooltip()
	{
		UIManager.UnitHUDStatusesTooltip.Hide();
	}

	private string GetTitle()
	{
		return $"<style={statusType}>{statuses[0].Name}</style>";
	}

	private string GetLines()
	{
		string text = string.Empty;
		for (int i = 0; i < statuses.Length; i++)
		{
			text += statuses[i].GetStylizedStatus();
			text += "\r\n";
		}
		return text;
	}
}
