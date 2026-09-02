using System.Text;
using TMPro;
using TPLib.Localization;
using TheLastStand.Definition.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public class SkillWithoutCompendiumTooltip : TooltipBase
{
	[SerializeField]
	protected RectTransform skillDetailsRect;

	[SerializeField]
	private RectTransform bgRect;

	[SerializeField]
	protected SkillDisplay skillDisplay;

	[SerializeField]
	private RectTransform skillEffectsRect;

	[SerializeField]
	private GameObject invalidPhasePanel;

	[SerializeField]
	private GameObject invalidInjuryPanel;

	[SerializeField]
	private TextMeshProUGUI invalidPhaseText;

	[SerializeField]
	private TextMeshProUGUI invalidInjuryText;

	public RectTransform TooltipPanel => tooltipPanel;

	public bool DisplayInvalidityPanel { get; set; }

	public void RefreshOnTileChanged()
	{
		if (base.Displayed)
		{
			skillDisplay.RefreshEffects();
		}
	}

	public void SetContent(TheLastStand.Model.Skill.Skill skill, ISkillCaster skillOwner = null)
	{
		skillDisplay.Skill = skill;
		skillDisplay.SkillOwner = skillOwner;
	}

	protected override bool CanBeDisplayed()
	{
		return skillDisplay.Skill != null;
	}

	protected override void OnHide()
	{
		base.OnHide();
		DisplayInvalidityPanel = false;
	}

	protected override void RefreshContent()
	{
		skillDisplay.Refresh();
		float num = 0f - skillDetailsRect.localPosition.y + skillDetailsRect.sizeDelta.y + skillEffectsRect.sizeDelta.y;
		tooltipPanel.sizeDelta = new Vector2(tooltipPanel.sizeDelta.x, num);
		bgRect.sizeDelta = new Vector2(bgRect.sizeDelta.x, num + bgRect.localPosition.y);
		skillEffectsRect.localPosition = new Vector2(skillEffectsRect.localPosition.x, skillDetailsRect.localPosition.y - skillDetailsRect.sizeDelta.y);
		RefreshInvalidityPanel();
	}

	private void RefreshInvalidityPanel()
	{
		if (!DisplayInvalidityPanel)
		{
			invalidPhasePanel.SetActive(value: false);
			invalidInjuryPanel.SetActive(value: false);
			return;
		}
		if (skillDisplay.Skill.SkillController.CheckPhaseAllowed() || skillDisplay.SkillOwner is EnemyUnit)
		{
			invalidPhasePanel.SetActive(value: false);
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(Localizer.Get("SkillTooltip_InvalidPhase_Title"));
			if (!skillDisplay.Skill.SkillDefinition.AllowDuringPhase.HasFlag(SkillDefinition.E_Phase.Production))
			{
				stringBuilder.AppendLine(Localizer.Get(string.Format("{0}{1}", "SkillTooltip_InvalidPhase_", Game.E_DayTurn.Production)));
			}
			if (!skillDisplay.Skill.SkillDefinition.AllowDuringPhase.HasFlag(SkillDefinition.E_Phase.Deployment))
			{
				stringBuilder.AppendLine(Localizer.Get(string.Format("{0}{1}", "SkillTooltip_InvalidPhase_", Game.E_DayTurn.Deployment)));
			}
			if (!skillDisplay.Skill.SkillDefinition.AllowDuringPhase.HasFlag(SkillDefinition.E_Phase.Night))
			{
				stringBuilder.AppendLine(Localizer.Get(string.Format("{0}{1}", "SkillTooltip_InvalidPhase_", Game.E_Cycle.Night)));
			}
			invalidPhaseText.text = stringBuilder.ToString();
			invalidPhasePanel.SetActive(value: true);
		}
		bool flag = skillDisplay.SkillOwner.PreventedSkillsIds.Contains(skillDisplay.Skill.SkillDefinition.Id);
		if (!flag)
		{
			invalidInjuryPanel.SetActive(value: false);
			return;
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendLine(Localizer.Get("SkillTooltip_InvalidInjury_Title"));
		if (flag && skillDisplay.SkillOwner is TheLastStand.Model.Unit.Unit unit)
		{
			int num = 0;
			for (int i = 0; i < unit.UnitTemplateDefinition.InjuryDefinitions.Count; i++)
			{
				num++;
				if (unit.UnitTemplateDefinition.InjuryDefinitions[i].PreventedSkillsIds.Contains(skillDisplay.Skill.SkillDefinition.Id))
				{
					break;
				}
			}
			stringBuilder2.AppendLine(Localizer.Get(string.Format("{0}{1}", "SkillTooltip_Injuried_", num)));
			Debug.Log(num);
		}
		invalidInjuryText.text = stringBuilder2.ToString();
		invalidInjuryPanel.SetActive(value: true);
	}
}
