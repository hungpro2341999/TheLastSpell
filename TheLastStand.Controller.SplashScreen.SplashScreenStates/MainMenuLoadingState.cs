using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;

namespace TheLastStand.Controller.SplashScreen.SplashScreenStates;

public class MainMenuLoadingState : State
{
	public const string Name = "MainMenuLoading";

	public override string GetName()
	{
		return "MainMenuLoading";
	}

	public override void OnStateEnter()
	{
		TPSingleton<SplashScreenManager>.Instance.SplashScreenSceneLoader.StartLoadScene();
	}
}
