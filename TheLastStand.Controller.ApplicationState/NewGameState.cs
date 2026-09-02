using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class NewGameState : State
{
	public const string Name = "NewGame";

	public override string GetName()
	{
		return "NewGame";
	}

	public override void OnStateEnter()
	{
		GameManager.EraseSave(SaveManager.CurrentProfileIndex);
		if (!ScenesManager.IsActiveSceneLevel())
		{
			SceneManager.LoadScene(ScenesManager.LoadLevelSceneName);
		}
	}
}
