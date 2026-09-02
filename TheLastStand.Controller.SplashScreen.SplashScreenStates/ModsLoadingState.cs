using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Manager.Modding;

namespace TheLastStand.Controller.SplashScreen.SplashScreenStates;

public class ModsLoadingState : State
{
	public const string Name = "ModsLoading";

	public override string GetName()
	{
		return "ModsLoading";
	}

	public override void OnStateEnter()
	{
		TPSingleton<ModManager>.Instance.StartLoadMods();
	}
}
