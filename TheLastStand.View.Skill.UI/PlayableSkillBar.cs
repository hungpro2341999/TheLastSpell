using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Yield;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.View.HUD;
using TheLastStand.View.HUD.UnitManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public class PlayableSkillBar : SkillBar
{
	[SerializeField]
	private SkillDisplayButton skillDisplayButtonPrefab;

	[SerializeField]
	private HorizontalLayoutGroup skillsLayoutGroup;

	[SerializeField]
	private int scrollbarEnabledBottomPadding = 7;

	[SerializeField]
	private Scrollbar scrollbar;

	[SerializeField]
	private RectTransform scrollViewport;

	[SerializeField]
	private RectTransform equippedSkillsHolder;

	[SerializeField]
	private RectTransform contextualSkillsHolder;

	[SerializeField]
	private RectTransform backpackSkillsHolder;

	[SerializeField]
	private RectTransform equippedSkillsCategory;

	[SerializeField]
	private RectTransform contextualSkillsCategory;

	[SerializeField]
	private RectTransform backpackSkillsCategory;

	[SerializeField]
	private GamepadInputDisplay gamepadInputDisplayNextSkillScrollEnabled;

	[SerializeField]
	private GamepadInputDisplay gamepadInputDisplayNextSkillScrollDisabled;

	private List<SkillDisplayButton> backpackSkillDisplayButtons = new List<SkillDisplayButton>();

	private List<SkillDisplayButton> contextualSkillDisplayButtons = new List<SkillDisplayButton>();

	private List<SkillDisplayButton> equippedSkillDisplayButtons = new List<SkillDisplayButton>();

	private int scrollbarDisabledBottomPadding;

	public void ChangeSelectedSkill()
	{
		for (int num = backpackSkillDisplayButtons.Count - 1; num >= 0; num--)
		{
			backpackSkillDisplayButtons[num].Select(PlayableUnitManager.SelectedSkill != null && PlayableUnitManager.SelectedSkill == backpackSkillDisplayButtons[num].Skill);
		}
		for (int num2 = contextualSkillDisplayButtons.Count - 1; num2 >= 0; num2--)
		{
			contextualSkillDisplayButtons[num2].Select(PlayableUnitManager.SelectedSkill != null && PlayableUnitManager.SelectedSkill == contextualSkillDisplayButtons[num2].Skill);
		}
		for (int num3 = equippedSkillDisplayButtons.Count - 1; num3 >= 0; num3--)
		{
			equippedSkillDisplayButtons[num3].Select(PlayableUnitManager.SelectedSkill != null && PlayableUnitManager.SelectedSkill == equippedSkillDisplayButtons[num3].Skill);
		}
	}

	public void CheckSkillButtonsFocus()
	{
		for (int i = 0; i < equippedSkillDisplayButtons.Count; i++)
		{
			if (equippedSkillDisplayButtons[i].HasFocus)
			{
				equippedSkillDisplayButtons[i].OnPointerEnter();
			}
		}
	}

	public void Refresh(bool fullRefresh = false)
	{
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			SetEquippedSkills(TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GetWeaponSkills(), fullRefresh);
			SetContextualSkills(TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GetContextualSkills(), fullRefresh);
			SetBackpackSkills(TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GetEquipmentSkills(), fullRefresh);
			StartCoroutine(RefreshGamepadInputDisplays());
		}
	}

	public void RefreshSkillDisplays()
	{
		List<SkillDisplayButton> list = new List<SkillDisplayButton>();
		list.AddRange(backpackSkillDisplayButtons);
		list.AddRange(contextualSkillDisplayButtons);
		list.AddRange(equippedSkillDisplayButtons);
		foreach (SkillDisplayButton item in list)
		{
			item.Refresh(fullRefresh: true);
		}
	}

	public void SetEquippedSkills(List<TheLastStand.Model.Skill.Skill> equippedSkills, bool fullRefresh = false)
	{
		SetSkills(equippedSkills, equippedSkillsHolder, equippedSkillsCategory, fullRefresh, ref equippedSkillDisplayButtons);
	}

	public void SetContextualSkills(List<TheLastStand.Model.Skill.Skill> contextualSkills, bool fullRefresh = false)
	{
		SetSkills(contextualSkills, contextualSkillsHolder, contextualSkillsCategory, fullRefresh, ref contextualSkillDisplayButtons);
	}

	public void SetBackpackSkills(List<TheLastStand.Model.Skill.Skill> backpackSkills, bool fullRefresh = false)
	{
		SetSkills(backpackSkills, backpackSkillsHolder, backpackSkillsCategory, fullRefresh, ref backpackSkillDisplayButtons);
	}

	public void SelectFirstSkill()
	{
		joystickSkillBar.SelectFirstSkill(1);
	}

	public void OnSkillHovered(SkillDisplayButton skillDisplayButton)
	{
		if (scrollbar.gameObject.activeSelf)
		{
			GUIHelpers.AdjustHorizontalScrollViewToFocusedItem(skillDisplayButton.transform as RectTransform, scrollViewport, scrollbar, 0.01f, 0.01f, 0.1f);
		}
	}

	public override void SelectNextSkill(bool next)
	{
		joystickSkillBar.Clear();
		joystickSkillBar.Register(TPSingleton<PlayableUnitManagementView>.Instance.ChangeEquipmentBoxView);
		joystickSkillBar.Register(equippedSkillDisplayButtons.Where((SkillDisplayButton button) => button.Skill != null));
		joystickSkillBar.Register(backpackSkillDisplayButtons.Where((SkillDisplayButton button) => button.Skill != null));
		joystickSkillBar.Register(contextualSkillDisplayButtons.Where((SkillDisplayButton button) => button.Skill != null));
		joystickSkillBar.SelectNextSkill(next, 1);
	}

	private void Awake()
	{
		scrollbarDisabledBottomPadding = skillsLayoutGroup.padding.bottom;
	}

	private IEnumerator RefreshGamepadInputDisplays()
	{
		int i = 0;
		while (i < 2)
		{
			yield return SharedYields.WaitForEndOfFrame;
			int num = i + 1;
			i = num;
		}
		gamepadInputDisplayNextSkillScrollDisabled.gameObject.SetActive(!scrollbar.gameObject.activeSelf);
		gamepadInputDisplayNextSkillScrollEnabled.gameObject.SetActive(scrollbar.gameObject.activeSelf);
	}

	private void SetSkills(List<TheLastStand.Model.Skill.Skill> skills, RectTransform skillHolder, RectTransform skillCategory, bool fullRefresh, ref List<SkillDisplayButton> skillDisplayButtons)
	{
		int i = 0;
		skillCategory.gameObject.SetActive(skills != null && skills.Count > 0);
		if (skills != null && skills.Count > 0)
		{
			skillHolder.gameObject.SetActive(value: true);
			for (; i < skills.Count; i++)
			{
				while (skillDisplayButtons.Count <= i)
				{
					SkillDisplayButton skillDisplayButton = Object.Instantiate(skillDisplayButtonPrefab, skillHolder);
					skillDisplayButton.Clicked += SkillDisplayButton_Clicked;
					skillDisplayButtons.Add(skillDisplayButton);
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
			skillDisplayButtons[i].SetSkill(null, null);
			skillDisplayButtons[i].gameObject.SetActive(value: false);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(skillHolder);
		StartCoroutine(WaitRebuildLayout());
	}

	private void SkillDisplayButton_Clicked(SkillDisplayButton sender)
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night || TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			Game.E_State state = TPSingleton<GameManager>.Instance.Game.State;
			if (state == Game.E_State.Management || state == Game.E_State.UnitPreparingSkill)
			{
				TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonClickAudioClip);
				PlayableUnitManager.SelectedSkill = sender.Skill;
			}
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < backpackSkillDisplayButtons.Count; i++)
		{
			backpackSkillDisplayButtons[i].Clicked -= SkillDisplayButton_Clicked;
		}
		for (int j = 0; j < contextualSkillDisplayButtons.Count; j++)
		{
			contextualSkillDisplayButtons[j].Clicked -= SkillDisplayButton_Clicked;
		}
		for (int k = 0; k < equippedSkillDisplayButtons.Count; k++)
		{
			equippedSkillDisplayButtons[k].Clicked -= SkillDisplayButton_Clicked;
		}
	}

	private IEnumerator WaitRebuildLayout()
	{
		int i = 0;
		while (i < 2)
		{
			yield return SharedYields.WaitForEndOfFrame;
			int num = i + 1;
			i = num;
		}
		skillsLayoutGroup.padding.bottom = (scrollbar.gameObject.activeSelf ? scrollbarEnabledBottomPadding : scrollbarDisabledBottomPadding);
	}
}
