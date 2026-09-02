using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class WorldMapState : State
{
	public const string Name = "WorldMap";

	public override string GetName()
	{
		return "WorldMap";
	}

	public override void OnStateEnter()
	{
		if (!ScenesManager.IsActiveSceneWorldMap() && SceneManager.GetActiveScene().name != ScenesManager.WorldMapSceneName)
		{
			SceneManager.LoadScene(ScenesManager.WorldMapSceneName);
		}
	}
}
