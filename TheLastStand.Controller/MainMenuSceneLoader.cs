using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller;

public class MainMenuSceneLoader : ASceneLoader
{
	protected override IEnumerator LoadLevelAsyncCoroutine()
	{
		CLoggerManager.Log($"[{Time.realtimeSinceStartup}] scene load starting", LogType.Log, CLogLevel.NORMAL, showLogsInUnity, "MainMenuSceneLoader");
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);
		asyncLoad.allowSceneActivation = false;
		yield return WaitEndOfLoading(asyncLoad);
		CLoggerManager.Log($"[{Time.realtimeSinceStartup}] scene has just finished loading !", LogType.Log, CLogLevel.DETAILED, showLogsInUnity, "MainMenuSceneLoader");
		float timer = 0f;
		while (!TPSingleton<SplashScreenManager>.Instance.SplashScreenIsOver)
		{
			timer += Time.deltaTime;
			if (timer >= sceneLoadingLogGap)
			{
				timer = 0f;
				CLoggerManager.Log($"[{Time.realtimeSinceStartup}] scene loaded, minDuration stalling", LogType.Log, CLogLevel.DETAILED, showLogsInUnity, "MainMenuSceneLoader");
			}
			yield return null;
		}
		ApplicationManager.Application.ApplicationController.SetState("GameLobby");
		CLoggerManager.Log($"[{Time.realtimeSinceStartup}] scene ready", LogType.Log, CLogLevel.NORMAL, showLogsInUnity, "MainMenuSceneLoader");
		asyncLoad.allowSceneActivation = true;
	}

	protected override IEnumerator WaitEndOfLoading(AsyncOperation asyncOperation)
	{
		float timer = 0f;
		while (!(asyncOperation.progress >= 0.9f))
		{
			timer += Time.deltaTime;
			if (timer >= sceneLoadingLogGap)
			{
				timer = 0f;
				CLoggerManager.Log($"[{Time.realtimeSinceStartup}] scene currently loading", LogType.Log, CLogLevel.DETAILED, showLogsInUnity, "MainMenuSceneLoader");
			}
			yield return null;
		}
	}
}
