using TheLastStand.Framework.Automaton;

namespace TheLastStand.Controller.ApplicationState;

public class SplashScreen : State
{
	public const string Name = "SplashScreen";

	public override string GetName()
	{
		return "SplashScreen";
	}
}
