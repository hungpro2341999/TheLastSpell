using System;
using System.Collections.Generic;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Tutorial;
using TheLastStand.Definition.Tutorial;
using TheLastStand.Manager;

namespace TheLastStand.Model.Tutorial;

public class Tutorial
{
	public static class Constants
	{
		public const string TutorialLocalizationPrefix = "TutorialPopup_";

		public const string TutorialLocalizationGamepad = "Gamepad";
	}

	public TutorialDefinition TutorialDefinition { get; }

	public List<TutorialConditionController> ConditionControllers { get; private set; }

	public Tutorial(TutorialDefinition tutorialDefinition)
	{
		TutorialDefinition = tutorialDefinition;
		GenerateConditionControllers();
	}

	public bool CanBeDisplayed()
	{
		if (ApplicationManager.Application.TutorialsRead.Contains(TutorialDefinition.Id))
		{
			return false;
		}
		if (!AreConditionsValid())
		{
			return false;
		}
		return true;
	}

	public bool CanBeDisplayedInSettings()
	{
		if (TutorialDefinition.HiddenUntilReadOnce)
		{
			return ApplicationManager.Application.TutorialsRead.Contains(TutorialDefinition.Id);
		}
		return true;
	}

	public bool AreConditionsValid()
	{
		if (ConditionControllers == null)
		{
			return true;
		}
		for (int i = 0; i < ConditionControllers.Count; i++)
		{
			if (!ConditionControllers[i].IsValid())
			{
				return false;
			}
		}
		return true;
	}

	public bool TryGetLocalizedHotkeys(out string[] hotkeys)
	{
		if (TutorialDefinition.RewiredActions == null)
		{
			hotkeys = null;
			return false;
		}
		hotkeys = new string[TutorialDefinition.RewiredActions.Count];
		for (int i = 0; i < hotkeys.Length; i++)
		{
			string[] localizedHotkeysForAction = InputManager.GetLocalizedHotkeysForAction(TutorialDefinition.RewiredActions[i].RewiredLabel);
			hotkeys[i] = ((TutorialDefinition.RewiredActions[i].SpecificIndex < localizedHotkeysForAction?.Length) ? localizedHotkeysForAction[TutorialDefinition.RewiredActions[i].SpecificIndex] : "Unassigned");
		}
		return true;
	}

	private void GenerateConditionControllers()
	{
		if (TutorialDefinition.ConditionDefinitions != null && TutorialDefinition.ConditionDefinitions.Count != 0)
		{
			ConditionControllers = new List<TutorialConditionController>();
			for (int i = 0; i < TutorialDefinition.ConditionDefinitions.Count; i++)
			{
				TutorialConditionDefinition tutorialConditionDefinition = TutorialDefinition.ConditionDefinitions[i];
				TutorialConditionController tutorialConditionController = ((tutorialConditionDefinition is InCityTutorialConditionDefinition conditionDefinition) ? new InCityTutorialConditionController(conditionDefinition) : ((tutorialConditionDefinition is DuringDayTurnTutorialConditionDefinition conditionDefinition2) ? new DuringDayTurnTutorialConditionController(conditionDefinition2) : ((tutorialConditionDefinition is DuringNightTutorialConditionDefinition conditionDefinition3) ? new DuringNightTutorialConditionController(conditionDefinition3) : ((tutorialConditionDefinition is CurrentNightHourTutorialConditionDefinition conditionDefinition4) ? new CurrentNightHourTutorialConditionController(conditionDefinition4) : ((tutorialConditionDefinition is TotalActionPointsSpentTutorialConditionDefinition conditionDefinition5) ? new TotalActionPointsSpentTutorialConditionController(conditionDefinition5) : ((tutorialConditionDefinition is TutorialMapSkippedTutorialConditionDefinition conditionDefinition6) ? ((TutorialConditionController)new TutorialMapSkippedTutorialConditionController(conditionDefinition6)) : ((TutorialConditionController)((!(tutorialConditionDefinition is IsWeaponRestrictionAvailableTutorialConditionDefinition conditionDefinition7)) ? null : new IsWeaponRestrictionAvailableTutorialConditionController(conditionDefinition7)))))))));
				TutorialConditionController item = tutorialConditionController;
				ConditionControllers.Add(item);
			}
		}
	}

	public List<string> LocalizeTexts()
	{
		List<string> list = new List<string>();
		string value;
		if (InputManager.IsLastControllerJoystick)
		{
			if (Localizer.TryGet("TutorialPopup_" + TutorialDefinition.Id + "_Gamepad", out value))
			{
				list.Add(value);
				return list;
			}
			list.AddRange(LocalizeGamepadTextsCount());
			if (list.Count > 0)
			{
				return list;
			}
		}
		if (Localizer.TryGet("TutorialPopup_" + TutorialDefinition.Id, out value))
		{
			if (TryGetLocalizedHotkeys(out var hotkeys))
			{
				try
				{
					string format = value;
					object[] args = hotkeys;
					value = string.Format(format, args);
				}
				catch (FormatException ex)
				{
					TPSingleton<TutorialManager>.Instance.LogError("Format error on Tutorial " + TutorialDefinition.Id + "! Exception: " + ex.Message);
					value = "FORMAT_ERROR";
				}
			}
			list.Add(value);
		}
		return list;
	}

	private List<string> LocalizeGamepadTextsCount()
	{
		int num = 0;
		bool flag = true;
		List<string> list = new List<string>();
		while (flag)
		{
			flag = Localizer.TryGet(string.Format("{0}{1}_{2}_{3:D2}", "TutorialPopup_", TutorialDefinition.Id, "Gamepad", num), out var value);
			if (flag)
			{
				list.Add(value);
			}
			num++;
		}
		return list;
	}
}
