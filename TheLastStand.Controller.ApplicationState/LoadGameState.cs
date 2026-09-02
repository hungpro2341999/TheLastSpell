using System.Collections;
using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class LoadGameState : State
{
	public const string Name = "LoadGame";

	public override string GetName()
	{
		return "LoadGame";
	}

	public override void OnStateEnter()
	{
		if (!ScenesManager.IsActiveSceneLevel() && SceneManager.GetActiveScene().name != ScenesManager.LoadLevelSceneName)
		{
			TPSingleton<ApplicationManager>.Instance.StartCoroutine(WaitForSavesCompletionThenLoadGame());
		}
	}

	private IEnumerator WaitForSavesCompletionThenLoadGame()
	{
		yield return new WaitUntil(() => SaverLoader.AreSavesCompleted());
		SceneManager.LoadScene(ScenesManager.LoadLevelSceneName);
	}
}
