using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class LoadWorldMapState : State
{
	public const string Name = "LoadWorldMap";

	public override string GetName()
	{
		return "LoadWorldMap";
	}

	public override void OnStateEnter()
	{
		if (!ScenesManager.IsActiveSceneWorldMap() && SceneManager.GetActiveScene().name != ScenesManager.LoadWorldMapSceneName)
		{
			SceneManager.LoadScene(ScenesManager.LoadWorldMapSceneName);
		}
	}
}
