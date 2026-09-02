using TheLastStand.Controller.SplashScreen;
using TheLastStand.Framework.Automaton;

namespace TheLastStand.Model.SplashScreen;

public class SplashScreen : PushdownAutomata
{
	public SplashScreenController SplashScreenController { get; private set; }

	public SplashScreen(SplashScreenController splashScreenController)
	{
		SplashScreenController = splashScreenController;
	}
}
