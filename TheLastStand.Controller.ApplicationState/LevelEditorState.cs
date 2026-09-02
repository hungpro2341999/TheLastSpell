using TheLastStand.Framework.Automaton;

namespace TheLastStand.Controller.ApplicationState;

public class LevelEditorState : State
{
	public const string Name = "LevelEditor";

	public override string GetName()
	{
		return "LevelEditor";
	}

	public override void OnStateEnter()
	{
	}
}
