using System.Collections;
using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Controller.ApplicationState;

public class LoadMainMenuFromGameState : State
{
	public const string Name = "LoadMainMenuFromGameState";

	public override string GetName()
	{
		return "LoadMainMenuFromGameState";
	}

	public override void OnStateEnter()
	{
		TPSingleton<ApplicationManager>.Instance.StartCoroutine(WaitForSavesCompletionThenLoadMainMenu());
	}

	private IEnumerator WaitForSavesCompletionThenLoadMainMenu()
	{
		yield return new WaitUntil(() => SaverLoader.AreSavesCompleted());
		ApplicationManager.Application.ApplicationController.SetState("GameLobby");
	}
}
