using System.Collections.Generic;
using System.Linq;
using TheLastStand.Manager;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public class EnemySkillBar : SkillBar
{
	[SerializeField]
	private SkillDisplayButton skillDisplayButtonPrefab;

	[SerializeField]
	private RectTransform skillsHolder;

	private List<SkillDisplayButton> skillDisplayButtons = new List<SkillDisplayButton>();

	public void CheckSkillButtonsFocus()
	{
		for (int i = 0; i < skillDisplayButtons.Count; i++)
		{
			if (skillDisplayButtons[i].HasFocus)
			{
				skillDisplayButtons[i].OnPointerEnter();
			}
		}
	}

	public override void SelectNextSkill(bool next)
	{
		joystickSkillBar.Clear();
		joystickSkillBar.Register(skillDisplayButtons);
		joystickSkillBar.SelectNextSkill(next, 0);
	}

	public void Refresh(bool fullRefresh = false)
	{
		if (!TileObjectSelectionManager.HasEnemyUnitSelected)
		{
			return;
		}
		List<TheLastStand.Model.Skill.Skill> list = new List<TheLastStand.Model.Skill.Skill>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < TileObjectSelectionManager.SelectedEnemyUnit.EnemyUnitTemplateDefinition.SkillsToDisplayIds.Count; i++)
		{
			string skillToDisplayId = TileObjectSelectionManager.SelectedEnemyUnit.EnemyUnitTemplateDefinition.SkillsToDisplayIds[i];
			Goal goal = null;
			if (!list2.Contains(skillToDisplayId) && TileObjectSelectionManager.SelectedEnemyUnit.Goals.Any(delegate(Goal x)
			{
				if (x.Skill.SkillDefinition.GroupId == skillToDisplayId)
				{
					goal = x;
					return true;
				}
				return false;
			}))
			{
				list.Add(goal.Skill);
				list2.Add(goal.Skill.SkillDefinition.Id);
			}
		}
		SetSkills(list, skillsHolder, fullRefresh, ref skillDisplayButtons);
	}

	private void SetSkills(List<TheLastStand.Model.Skill.Skill> skills, RectTransform skillHolder, bool fullRefresh, ref List<SkillDisplayButton> skillDisplayButtons)
	{
		int i = 0;
		if (skills != null && skills.Count > 0)
		{
			skillHolder.gameObject.SetActive(value: true);
			for (; i < skills.Count; i++)
			{
				while (skillDisplayButtons.Count <= i)
				{
					SkillDisplayButton item = Object.Instantiate(skillDisplayButtonPrefab, skillHolder);
					skillDisplayButtons.Add(item);
				}
				skillDisplayButtons[i].SetSkill(skills[i], TileObjectSelectionManager.SelectedUnit);
				skillDisplayButtons[i].Refresh(fullRefresh);
				skillDisplayButtons[i].gameObject.SetActive(value: true);
			}
		}
		else
		{
			skillHolder.gameObject.SetActive(value: false);
		}
		for (; i < skillDisplayButtons.Count; i++)
		{
			skillDisplayButtons[i].gameObject.SetActive(value: false);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(skillHolder);
	}
}
