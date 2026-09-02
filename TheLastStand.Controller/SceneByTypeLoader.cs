using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Controller;

public class SceneByTypeLoader : SceneLoader
{
	public enum E_SceneType
	{
		None,
		Splash,
		MainMenu,
		Map,
		Level,
		MetaShop,
		Credits
	}

	[Header("Level type")]
	[SerializeField]
	private E_SceneType levelType;

	public override string SceneName
	{
		get
		{
			switch (levelType)
			{
			case E_SceneType.Splash:
				return ScenesManager.SplashSceneName;
			case E_SceneType.MainMenu:
				return ScenesManager.MainMenuSceneName;
			case E_SceneType.Map:
				return ScenesManager.WorldMapSceneName;
			case E_SceneType.Level:
				return ScenesManager.LevelSceneName;
			case E_SceneType.MetaShop:
				return ScenesManager.MetaShopSceneName;
			case E_SceneType.Credits:
				return ScenesManager.CreditsSceneName;
			default:
				TPSingleton<ScenesManager>.Instance.Log("SceneType could not be converted to a correct scene name.", CLogLevel.DETAILED, forcePrintInUnity: true);
				return string.Empty;
			}
		}
	}
}
