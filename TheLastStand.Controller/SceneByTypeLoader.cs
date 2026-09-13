using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Controller;

/// <summary>
/// Thành phần hỗ trợ tải Scene của Unity dựa trên loại màn chơi được chọn (Splash, MainMenu, Map, Level, MetaShop...).
/// </summary>
public class SceneByTypeLoader : SceneLoader
{
	#region Enums & Serialized Fields

	/// <summary>
	/// Định nghĩa các loại Scene trong trò chơi.
	/// </summary>
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

	#endregion

	#region Properties

	/// <summary>
	/// Tên Scene thực tế trong Unity tương ứng với loại Scene được thiết lập.
	/// </summary>
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

	#endregion
}
