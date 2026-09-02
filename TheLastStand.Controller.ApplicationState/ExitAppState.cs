using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;

namespace TheLastStand.Controller.ApplicationState;

public class ExitAppState : State
{
	public const string Name = "ExitApp";

	public override string GetName()
	{
		return "ExitApp";
	}

	public override void OnStateEnter()
	{
		ApplicationManager.Quit();
	}
}
