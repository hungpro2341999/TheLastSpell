using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Injury;

public class UnitInjuryEffectDisplay : MonoBehaviour
{
	[SerializeField]
	private Image injuryIcon;

	[SerializeField]
	private DataSpriteTable injuryIcons;

	[SerializeField]
	private TextMeshProUGUI titleText;

	private UnitStatDefinition.E_Stat stat;

	private string preventedSkill;

	private float modifier;

	public virtual void Init(UnitStatDefinition.E_Stat stat, float modifier, int injuryStage)
	{
		this.stat = stat;
		this.modifier = modifier;
		injuryIcon.sprite = injuryIcons.GetSpriteAt(injuryStage - 1);
		RefreshTitle();
	}

	public virtual void Init(string preventedSkill, int injuryStage)
	{
		this.preventedSkill = preventedSkill;
		injuryIcon.sprite = injuryIcons.GetSpriteAt(injuryStage - 1);
		RefreshTitle();
	}

	private void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshTitle();
		}
	}

	private void RefreshTitle()
	{
		if (string.IsNullOrEmpty(preventedSkill))
		{
			titleText.text = string.Format("{0}{1}{2} <sprite name={3}>{4}", (modifier >= 0f) ? "+" : string.Empty, modifier, stat.ShownAsPercentage() ? "%" : string.Empty, stat, UnitDatabase.UnitStatDefinitions[stat].Name);
		}
		else
		{
			titleText.text = string.Format(Localizer.Get("Injury_PreventSkill_UnitTooltip"), "<style=Skill>" + Localizer.Get("SkillName_" + preventedSkill) + "</style>");
		}
	}
}
