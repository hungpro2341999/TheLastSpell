using TPLib;
using TPLib.Log;
using TheLastStand.Controller.SplashScreen.SplashScreenStates;
using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using TheLastStand.Model.SplashScreen;

namespace TheLastStand.Controller.SplashScreen;

public class SplashScreenController
{
	public delegate void SplashScreenStateChangeHandler(State state);

	public TheLastStand.Model.SplashScreen.SplashScreen SplashScreen { get; private set; }

	public event SplashScreenStateChangeHandler SplashScreenStateChangeEvent;

	public SplashScreenController()
	{
		SplashScreen = new TheLastStand.Model.SplashScreen.SplashScreen(this);
	}

	public void SetState(string stateName)
	{
		switch (stateName)
		{
		case "MainMenuLoading":
			SetState(new MainMenuLoadingState());
			break;
		case "ModsLoading":
			SetState(new ModsLoadingState());
			break;
		case "SettingsLoading":
			SetState(new SettingsLoadingState());
			break;
		default:
			TPSingleton<SplashScreenManager>.Instance.LogError("Trying to set an unknown State : " + stateName);
			break;
		}
	}

	public void SetState(State newState)
	{
		TPSingleton<SplashScreenManager>.Instance.Log($"State transition : {SplashScreen.State} -> {newState}", CLogLevel.DETAILED);
		SplashScreen.SetState(newState);
		SplashScreenManager.CurrentStateName = SplashScreen.State.GetName();
		this.SplashScreenStateChangeEvent?.Invoke(SplashScreen.State);
	}
}
