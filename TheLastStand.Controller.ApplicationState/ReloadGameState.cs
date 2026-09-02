using System.Collections;
using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class ReloadGameState : State
{
	public const string Name = "ReloadGame";

	public override string GetName()
	{
		return "ReloadGame";
	}

	public override void OnStateEnter()
	{
		if (SceneManager.GetActiveScene().name != ScenesManager.LoadLevelSceneName)
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
