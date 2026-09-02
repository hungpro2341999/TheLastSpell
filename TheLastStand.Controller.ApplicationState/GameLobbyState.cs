using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class GameLobbyState : State
{
	public const string Name = "GameLobby";

	public override string GetName()
	{
		return "GameLobby";
	}

	public override void OnStateEnter()
	{
		Scene activeScene = SceneManager.GetActiveScene();
		if (!(activeScene.name == ScenesManager.MainMenuSceneName) && activeScene.name != ScenesManager.SplashSceneName)
		{
			SceneManager.LoadScene(ScenesManager.MainMenuSceneName);
		}
	}
}
