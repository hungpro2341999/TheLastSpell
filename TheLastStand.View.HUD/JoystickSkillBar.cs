using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.HUD;

public class JoystickSkillBar : MonoBehaviour
{
	private List<IJoystickSelect> joystickSkills = new List<IJoystickSelect>();

	private IJoystickSelect currentSkillSelected;

	private IJoystickSelect previousSkillSelected;

	private int indexSkill;

	public IJoystickSelect CurrentSkillSelected => currentSkillSelected;

	public int NumberOfSkills => joystickSkills.Count;

	public void Clear()
	{
		joystickSkills.Clear();
	}

	public void DeselectCurrentSkill(bool deselectConfirmAndSkill = false)
	{
		if (currentSkillSelected == null)
		{
			return;
		}
		if (currentSkillSelected is IJoystickSkillConfirmHandler { IsConfirmSelected: not false } joystickSkillConfirmHandler)
		{
			joystickSkillConfirmHandler.DeselectConfirmButton(deselectConfirmAndSkill);
			if (!deselectConfirmAndSkill)
			{
				return;
			}
		}
		currentSkillSelected.OnSkillHover(select: false);
		if (currentSkillSelected != null)
		{
			previousSkillSelected = currentSkillSelected;
		}
		currentSkillSelected = null;
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
	}

	public bool IsOnSkillBarSelection()
	{
		return currentSkillSelected != null;
	}

	public void Register(IJoystickSelect joystickSkill)
	{
		joystickSkills.Add(joystickSkill);
	}

	public void Register(IEnumerable<IJoystickSelect> joystickSkills)
	{
		this.joystickSkills.AddRange(joystickSkills);
	}

	public void ResetSkillIndex(int index = 0)
	{
		DeselectCurrentSkill(deselectConfirmAndSkill: true);
		indexSkill = ((joystickSkills.Count <= index) ? joystickSkills.Count : index);
	}

	public void SelectFirstSkill(int index = 0)
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CharacterSheet)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		}
		DeselectCurrentSkill(deselectConfirmAndSkill: true);
		if (joystickSkills.Count > index)
		{
			if (currentSkillSelected != null)
			{
				previousSkillSelected = currentSkillSelected;
			}
			currentSkillSelected = joystickSkills[indexSkill = index];
			currentSkillSelected.OnSkillHover(select: true);
		}
	}

	public void SelectNextSkill(bool next, int beginAt)
	{
		TakeNextSkillIndex(next, beginAt);
		TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		DeselectCurrentSkill(deselectConfirmAndSkill: true);
		if (joystickSkills.Count > 0)
		{
			if (currentSkillSelected != null)
			{
				previousSkillSelected = currentSkillSelected;
			}
			currentSkillSelected = joystickSkills[indexSkill];
			currentSkillSelected.OnSkillHover(select: true);
		}
	}

	public void SelectCurrentOrPreviousSkill()
	{
		if (currentSkillSelected != null)
		{
			currentSkillSelected.OnSkillHover(select: true);
			return;
		}
		if (previousSkillSelected == null)
		{
			SelectFirstSkill();
			return;
		}
		currentSkillSelected = previousSkillSelected;
		currentSkillSelected.OnSkillHover(select: true);
	}

	public void TakeNextSkillIndex(bool next, int beginAt)
	{
		if (next)
		{
			if (currentSkillSelected == null && previousSkillSelected == null)
			{
				indexSkill = beginAt;
			}
			else if (currentSkillSelected != null)
			{
				indexSkill = ((indexSkill + 1 < joystickSkills.Count) ? (indexSkill + 1) : 0);
			}
			else if (indexSkill >= joystickSkills.Count)
			{
				indexSkill = joystickSkills.Count - 1;
			}
		}
		else if (currentSkillSelected == null && previousSkillSelected == null)
		{
			indexSkill = joystickSkills.Count - 1;
		}
		else if (currentSkillSelected != null)
		{
			indexSkill = ((indexSkill - 1 < 0) ? (joystickSkills.Count - 1) : (indexSkill - 1));
		}
		else if (indexSkill >= joystickSkills.Count)
		{
			indexSkill = joystickSkills.Count - 1;
		}
	}

	private void Awake()
	{
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
		HUDJoystickNavigationManager.HUDNavigationToggled += OnHUDNavigationToggled;
	}

	private void OnDestroy()
	{
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
		HUDJoystickNavigationManager.HUDNavigationToggled -= OnHUDNavigationToggled;
	}

	private void OnHUDNavigationToggled(bool state)
	{
		if (IsOnSkillBarSelection() && state)
		{
			GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			DeselectCurrentSkill();
			EventSystem.current.SetSelectedGameObject(currentSelectedGameObject);
		}
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if (IsOnSkillBarSelection() && (!InputManager.JoystickConfig.HUDNavigation.AlwaysShowTooltipOnPlayableSkillHovering || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips))
		{
			currentSkillSelected.OnDisplayTooltip(showTooltips);
		}
	}
}
