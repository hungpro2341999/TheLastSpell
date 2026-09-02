using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public class SkillListButtonsDisplay : SerializedMonoBehaviour
{
	[SerializeField]
	private GridLayoutGroup grid;

	[SerializeField]
	private SkillDisplay rightDisplay;

	private List<SkillDisplayButton> skillDisplayButtons = new List<SkillDisplayButton>();

	private void Awake()
	{
		grid.transform.DestroyChildren();
		for (int i = 0; i < skillDisplayButtons.Count; i++)
		{
			skillDisplayButtons[i].Clicked += SkillDisplayButton_Clicked;
		}
	}

	private void SkillDisplayButton_Clicked(SkillDisplayButton sender)
	{
		rightDisplay.Skill = sender.Skill;
		rightDisplay.Refresh();
	}

	private void OnDestroy()
	{
		for (int i = 0; i < skillDisplayButtons.Count; i++)
		{
			skillDisplayButtons[i].Clicked -= SkillDisplayButton_Clicked;
		}
	}
}
