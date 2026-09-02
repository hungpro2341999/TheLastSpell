using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Database;
using TheLastStand.Definition.Tutorial;
using TheLastStand.Model.Tutorial;
using TheLastStand.View.Tutorial;
using UnityEngine;

namespace TheLastStand.Manager;

public class TutorialManager : Manager<TutorialManager>
{
	private readonly List<Tutorial> tutorials = new List<Tutorial>();

	public bool DisabledTutorials { get; private set; }

	public List<Tutorial> GetTutorialsOfCategory(E_TutorialCategory category, Func<Tutorial, bool> match = null)
	{
		List<Tutorial> list = new List<Tutorial>();
		for (int i = 0; i < tutorials.Count; i++)
		{
			Tutorial tutorial = tutorials[i];
			if (tutorial.TutorialDefinition.Category == category && (match == null || match(tutorial)))
			{
				list.Add(tutorial);
			}
		}
		return list;
	}

	public bool AnyMatchingTutorial(Func<Tutorial, bool> match)
	{
		return tutorials.Any(match.Invoke);
	}

	public void OnTrigger(E_TutorialTrigger trigger)
	{
		if (TPSingleton<TutorialManager>.Instance.DisabledTutorials)
		{
			return;
		}
		List<Tutorial> list = new List<Tutorial>();
		foreach (Tutorial tutorial in tutorials)
		{
			if (tutorial.TutorialDefinition.Trigger == trigger && tutorial.CanBeDisplayed())
			{
				list.Add(tutorial);
			}
		}
		if (list.Count != 0)
		{
			TPSingleton<TutorialView>.Instance.AddTutorialsToDisplay(list);
		}
	}

	public void OnTutorialRead(Tutorial tutorial)
	{
		ApplicationManager.Application.TutorialsRead.Add(tutorial.TutorialDefinition.Id);
		if (tutorial.TutorialDefinition.LockTutorials != null)
		{
			for (int i = 0; i < tutorial.TutorialDefinition.LockTutorials.Count; i++)
			{
				ApplicationManager.Application.TutorialsRead.Add(tutorial.TutorialDefinition.LockTutorials[i]);
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		foreach (KeyValuePair<string, TutorialDefinition> tutorialsDefinition in TutorialDatabase.TutorialsDefinitions)
		{
			Tutorial item = new Tutorial(tutorialsDefinition.Value);
			tutorials.Add(item);
		}
	}

	[DevConsoleCommand("TutorialsDisable")]
	private static void TutorialsDisable(bool state = true)
	{
		TPSingleton<TutorialManager>.Instance.DisabledTutorials = state;
	}

	[DevConsoleCommand("TutorialLocalizeActions")]
	private static void TutorialLocalizeActions([StringConverter(typeof(TutorialDefinition.StringToTutorialIdConverter))] string tutorialId)
	{
		TutorialDefinition tutorialDefinition = TutorialDatabase.TutorialsDefinitions[tutorialId];
		if (tutorialDefinition.RewiredActions != null)
		{
			for (int i = 0; i < tutorialDefinition.RewiredActions.Count; i++)
			{
				Debug.Log(string.Join(",", InputManager.GetLocalizedHotkeysForAction(tutorialDefinition.RewiredActions[i].RewiredLabel)[tutorialDefinition.RewiredActions[i].SpecificIndex]));
			}
		}
	}

	[DevConsoleCommand("TutorialsClearRead")]
	private static void ClearTutorialRead([StringConverter(typeof(TutorialDefinition.StringToTutorialIdConverter))] string tutorialId)
	{
		if (ApplicationManager.Application.TutorialsRead.Contains(tutorialId))
		{
			ApplicationManager.Application.TutorialsRead.Remove(tutorialId);
		}
	}

	[DevConsoleCommand("TutorialsClearReadAll")]
	private static void ClearAllTutorialsRead()
	{
		ApplicationManager.Application.TutorialsRead.Clear();
	}

	[DevConsoleCommand("TutorialsTrigger")]
	private static void TriggerTutorial([StringConverter(typeof(TutorialDefinition.StringToTutorialIdConverter))] string tutorialId)
	{
		TPSingleton<TutorialView>.Instance.AddTutorialsToDisplay(new List<Tutorial>
		{
			new Tutorial(TutorialDatabase.TutorialsDefinitions[tutorialId])
		});
	}
}
