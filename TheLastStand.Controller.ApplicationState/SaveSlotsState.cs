using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.View.SaveSlots;

namespace TheLastStand.Controller.ApplicationState;

public class SaveSlotsState : State
{
	public const string Name = "SaveSlots";

	public override string GetName()
	{
		return "SaveSlots";
	}

	public override void OnStateEnter()
	{
		TPSingleton<SaveSlotsPanel>.Instance.Open();
		TPSingleton<SaveSlotsPanel>.Instance.Refresh();
	}
}
