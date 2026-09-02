using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.View.Modding;

namespace TheLastStand.Controller.ApplicationState;

public class ModListState : State
{
	public const string Name = "ModList";

	public override string GetName()
	{
		return "ModList";
	}

	public override void OnStateEnter()
	{
		TPSingleton<ModsView>.Instance.Open();
	}
}
