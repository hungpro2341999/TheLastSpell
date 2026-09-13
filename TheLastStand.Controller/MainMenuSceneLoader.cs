using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller;

/// <summary>
/// Bộ nạp Scene Menu chính (MainMenu) bất đồng bộ kết hợp xử lý đợi màn hình chào (SplashScreen) kết thúc.
/// </summary>
public class MainMenuSceneLoader : ASceneLoader
{
	#region Async Loading Coroutines

	/// <summary>
	/// Coroutine tải Scene bất đồng bộ trong nền, chờ nạp xong tài nguyên và đợi màn hình Splash Screen kết thúc trước khi kích hoạt Scene.
	/// </summary>
	protected override IEnumerator LoadLevelAsyncCoroutine()
	{
		CLoggerManager.Log($"[{Time.realtimeSinceStartup}] scene load starting", LogType.Log, CLogLevel.NORMAL, showLogsInUnity, "MainMenuSceneLoader");
		
		// Bắt đầu nạp Scene bất đồng bộ nhưng tạm dừng kích hoạt
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);
		asyncLoad.allowSceneActivation = false;
		
		// Chờ cho đến khi nạp được tối thiểu 90% dữ liệu
		yield return WaitEndOfLoading(asyncLoad);
		CLoggerManager.Log($"[{Time.realtimeSinceStartup}] scene has just finished loading !", LogType.Log, CLogLevel.DETAILED, showLogsInUnity, "MainMenuSceneLoader");
		
		// Đợi màn hình SplashScreen chạy xong hoàn toàn
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
		
		// Chuyển trạng thái ứng dụng sang GameLobby (Sảnh chờ / Menu)
		ApplicationManager.Application.ApplicationController.SetState("GameLobby");
		CLoggerManager.Log($"[{Time.realtimeSinceStartup}] scene ready", LogType.Log, CLogLevel.NORMAL, showLogsInUnity, "MainMenuSceneLoader");
		
		// Cho phép kích hoạt hiển thị Scene
		asyncLoad.allowSceneActivation = true;
	}

	/// <summary>
	/// Chờ tiến trình nạp Scene đạt mốc hoàn tất cơ bản (>= 90%).
	/// </summary>
	/// <param name="asyncOperation">Tác vụ nạp bất đồng bộ của Unity.</param>
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

	#endregion
}
