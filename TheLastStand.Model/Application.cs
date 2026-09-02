using System.Collections.Generic;
using TheLastStand.Controller;
using TheLastStand.Framework.Automaton;

namespace TheLastStand.Model;

public class Application : PushdownAutomata
{
	private uint damnedSouls;

	public bool ApplicationQuitInOraculum;

	public uint DamnedSoulsObtained;

	public uint DaysPlayed;

	public bool HasSeenIntroduction;

	public uint RunsCompleted;

	public uint RunsWon;

	public bool TutorialDone;

	public bool TutorialSkipped;

	public List<string> TutorialsRead = new List<string>();

	public Dictionary<string, State> StatesPool { get; } = new Dictionary<string, State>();

	public ApplicationController ApplicationController { get; }

	public uint DamnedSouls
	{
		get
		{
			return damnedSouls;
		}
		set
		{
			if (value > damnedSouls)
			{
				DamnedSoulsObtained += value - damnedSouls;
			}
			damnedSouls = value;
		}
	}

	public bool IsGameState => base.State.GetName() == "Game";

	public Application(ApplicationController applicationController)
	{
		ApplicationController = applicationController;
	}
}
