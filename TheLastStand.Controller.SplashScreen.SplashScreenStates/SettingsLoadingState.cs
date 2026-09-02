using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;

namespace TheLastStand.Controller.SplashScreen.SplashScreenStates;

public class SettingsLoadingState : State
{
	public const string Name = "SettingsLoading";

	public override string GetName()
	{
		return "SettingsLoading";
	}

	public override void OnStateEnter()
	{
		TPSingleton<SettingsManager>.Instance.Init();
	}
}
