using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;

namespace TheLastStand.Controller.ApplicationState;

public class SettingsState : State
{
	public const string Name = "Settings";

	public override string GetName()
	{
		return "Settings";
	}

	public override void OnStateEnter()
	{
		SettingsManager.OpenSettings();
	}
}
