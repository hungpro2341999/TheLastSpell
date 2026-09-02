using TPLib;
using TheLastStand.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Manager;

public class ScenesManager : Manager<ScenesManager>
{
	[SerializeField]
	private SceneField splashScene;

	[SerializeField]
	private SceneField mainMenuScene;

	[SerializeField]
	private SceneField creditsScene;

	[SerializeField]
	private SceneField animatedCutsceneScene;

	[SerializeField]
	private SceneField loadLevelScene;

	[SerializeField]
	private SceneField loadWorldMapScene;

	[SerializeField]
	private SceneField levelScene;

	[SerializeField]
	private bool overrideLevelScene;

	[SerializeField]
	private SceneField levelSceneOverride;

	[SerializeField]
	private SceneField worldMapScene;

	[SerializeField]
	private bool overrideWorldMapScene;

	[SerializeField]
	private SceneField worldMapSceneOverride;

	[SerializeField]
	private SceneField metaShopScene;

	[SerializeField]
	private bool overrideMetaShopScene;

	[SerializeField]
	private SceneField metaShopSceneOverride;

	public static string CreditsSceneName => TPSingleton<ScenesManager>.Instance.creditsScene;

	public static string AnimatedCutsceneSceneName => TPSingleton<ScenesManager>.Instance.animatedCutsceneScene;

	public static string LevelSceneName
	{
		get
		{
			if (TPSingleton<ScenesManager>.Instance.overrideLevelScene)
			{
				TPSingleton<ScenesManager>.Instance.Log("Loading overridden level scene.");
				return TPSingleton<ScenesManager>.Instance.levelSceneOverride;
			}
			return TPSingleton<ScenesManager>.Instance.levelScene;
		}
	}

	public static string LoadLevelSceneName => TPSingleton<ScenesManager>.Instance.loadLevelScene;

	public static string LoadWorldMapSceneName => TPSingleton<ScenesManager>.Instance.loadWorldMapScene;

	public static string MainMenuSceneName => TPSingleton<ScenesManager>.Instance.mainMenuScene;

	public static string MetaShopSceneName
	{
		get
		{
			if (TPSingleton<ScenesManager>.Instance.overrideMetaShopScene)
			{
				TPSingleton<ScenesManager>.Instance.Log("Loading overridden meta shop scene.");
				return TPSingleton<ScenesManager>.Instance.metaShopSceneOverride;
			}
			return TPSingleton<ScenesManager>.Instance.metaShopScene;
		}
	}

	public static string SplashSceneName => TPSingleton<ScenesManager>.Instance.splashScene;

	public static string WorldMapSceneName
	{
		get
		{
			if (TPSingleton<ScenesManager>.Instance.overrideWorldMapScene)
			{
				TPSingleton<ScenesManager>.Instance.Log("Loading overridden world map scene.");
				return TPSingleton<ScenesManager>.Instance.worldMapSceneOverride;
			}
			return TPSingleton<ScenesManager>.Instance.worldMapScene;
		}
	}

	public static bool IsActiveSceneLevel()
	{
		string text = SceneManager.GetActiveScene().name;
		if (TPSingleton<ScenesManager>.Instance.overrideLevelScene)
		{
			return text == TPSingleton<ScenesManager>.Instance.levelSceneOverride;
		}
		return text == TPSingleton<ScenesManager>.Instance.levelScene;
	}

	public static bool IsActiveSceneWorldMap()
	{
		string text = SceneManager.GetActiveScene().name;
		if (TPSingleton<ScenesManager>.Instance.overrideWorldMapScene)
		{
			return text == TPSingleton<ScenesManager>.Instance.worldMapSceneOverride;
		}
		return text == TPSingleton<ScenesManager>.Instance.worldMapScene;
	}

	public static bool IsActiveSceneMetaShop()
	{
		string text = SceneManager.GetActiveScene().name;
		if (TPSingleton<ScenesManager>.Instance.overrideMetaShopScene)
		{
			return text == TPSingleton<ScenesManager>.Instance.metaShopSceneOverride;
		}
		return text == TPSingleton<ScenesManager>.Instance.metaShopScene;
	}

	public static bool IsSceneActive(string sceneName)
	{
		string text = SceneManager.GetActiveScene().name;
		if (TPSingleton<ScenesManager>.Instance.overrideMetaShopScene)
		{
			return text == TPSingleton<ScenesManager>.Instance.metaShopSceneOverride;
		}
		return text == sceneName;
	}
}
