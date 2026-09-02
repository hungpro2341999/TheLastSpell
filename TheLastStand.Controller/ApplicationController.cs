using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Controller;

public class ApplicationController
{
	public delegate void ApplicationStateChangeHandler(State state);

	public TheLastStand.Model.Application Application { get; private set; }

	public event ApplicationStateChangeHandler ApplicationStateChangeEvent;

	public ApplicationController()
	{
		Application = new TheLastStand.Model.Application(this);
	}

	public void BackToPreviousState()
	{
		if (Application.PreviousStatesStack.Count == 0)
		{
			TPSingleton<ApplicationManager>.Instance.LogError($"Trying to back to a previous state (currently {Application.State}) but no previous state found!", CLogLevel.MAJOR);
		}
		else
		{
			Application.SetState(Application.PreviousStatesStack.Pop());
		}
	}

	public void SetState(string stateName)
	{
		if (!Application.StatesPool.TryGetValue(stateName, out var value))
		{
			switch (stateName)
			{
			case "ExitApp":
				value = new ExitAppState();
				break;
			case "Game":
				value = new GameState();
				break;
			case "GameLobby":
				value = new GameLobbyState();
				break;
			case "LoadGame":
				value = new LoadGameState();
				break;
			case "ReloadGame":
				value = new ReloadGameState();
				break;
			case "LoadWorldMap":
				value = new LoadWorldMapState();
				break;
			case "WorldMap":
				value = new WorldMapState();
				break;
			case "MetaShops":
				value = new MetaShopsState();
				break;
			case "NewGame":
				value = new NewGameState();
				break;
			case "Settings":
				value = new SettingsState();
				break;
			case "LevelEditor":
				value = new LevelEditorState();
				break;
			case "Credits":
				value = new CreditsState();
				break;
			case "AnimatedCutscene":
				value = new AnimatedCutsceneState();
				break;
			case "ModList":
				value = new ModListState();
				break;
			case "SplashScreen":
				value = new TheLastStand.Controller.ApplicationState.SplashScreen();
				break;
			case "SaveSlots":
				value = new SaveSlotsState();
				break;
			case "LoadMainMenuFromGameState":
				value = new LoadMainMenuFromGameState();
				break;
			default:
				TPSingleton<ApplicationManager>.Instance.LogError("Unknown state " + stateName, CLogLevel.MAJOR);
				return;
			}
			Application.StatesPool.Add(stateName, value);
		}
		switch (stateName)
		{
		case "GameLobby":
			if (ScenesManager.IsSceneActive(ScenesManager.SplashSceneName) || !(Application.State?.GetName() != "LoadMainMenuFromGameState"))
			{
				break;
			}
			goto case "Credits";
		case "Credits":
		case "Game":
		case "AnimatedCutscene":
		case "LoadGame":
		case "LoadWorldMap":
		case "MetaShops":
		case "NewGame":
		case "WorldMap":
		case "LoadMainMenuFromGameState":
			if (Application.State == null)
			{
				SetState(value);
			}
			else
			{
				TPSingleton<ApplicationManager>.Instance.StartCoroutine(SetStateCoroutine(value));
			}
			return;
		}
		SetState(value);
	}

	public void SetState(State newState, params object[] args)
	{
		TPSingleton<ApplicationManager>.Instance.Log($"State transition : {Application.State} -> {newState}", CLogLevel.DETAILED);
		Application.SetState(newState);
		ApplicationManager.CurrentStateName = Application.State.GetName();
		this.ApplicationStateChangeEvent?.Invoke(Application.State);
	}

	public IEnumerator SetStateCoroutine(State newState)
	{
		CanvasFadeManager.FadeIn();
		yield return new WaitUntil(() => TPSingleton<CanvasFadeManager>.Instance.FadeIsOver);
		SetState(newState);
	}
}
