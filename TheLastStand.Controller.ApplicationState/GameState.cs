using TheLastStand.Framework.Automaton;

namespace TheLastStand.Controller.ApplicationState;

public class GameState : State
{
	public const string Name = "Game";

	public override string GetName()
	{
		return "Game";
	}
}
