using System;
using System.Text;
using Steamworks;
using TheLastStand;
using UnityEngine;

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	[SerializeField]
	private uint appId = 1105670u;

	[SerializeField]
	private uint demoAppId = 1297280u;

	[SerializeField]
	private bool isDemo;

	private static bool? cachedIsRunningOnSteamDeck;

	protected static SteamManager s_instance;

	protected static bool s_EverInitialized;

	protected bool m_bInitialized;

	protected bool m_bInitializationFailed;

	protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	public string ConsoleDebugLog = "Steam";

	protected static SteamManager Instance
	{
		get
		{
			if (s_instance == null)
			{
				return new GameObject("SteamManager").AddComponent<SteamManager>();
			}
			return s_instance;
		}
	}

	public static bool IsRunningOnSteamDeck
	{
		get
		{
			if (!cachedIsRunningOnSteamDeck.HasValue)
			{
				if (Initialized)
				{
					return InitCachedIsRunningOnSteamDeck();
				}
				return false;
			}
			return cachedIsRunningOnSteamDeck.Value;
		}
	}

	public static bool Initialized => Instance.m_bInitialized;

	public static bool InitializationFailed => Instance.m_bInitializationFailed;

	private static bool InitCachedIsRunningOnSteamDeck()
	{
		cachedIsRunningOnSteamDeck = SteamUtils.IsSteamRunningOnSteamDeck();
		return cachedIsRunningOnSteamDeck.Value;
	}

	protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	protected virtual void Awake()
	{
		if (s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		s_instance = this;
		if (s_EverInitialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!Packsize.Test())
		{
			OnSteamInitialisationFailed("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
			return;
		}
		if (!DllCheck.Test())
		{
			OnSteamInitialisationFailed("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
			return;
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(new AppId_t(isDemo ? demoAppId : appId)))
			{
				OnSteamInitialisationFailed("RestartAppIfNecessary() returned true");
				return;
			}
		}
		catch (DllNotFoundException ex)
		{
			OnSteamInitialisationFailed("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex);
			return;
		}
		m_bInitialized = SteamAPI.Init();
		if (!m_bInitialized)
		{
			OnSteamInitialisationFailed("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
			return;
		}
		s_EverInitialized = true;
		Log("Initialized", this);
		Analytics.InitializeAfterDLCManager();
	}

	protected virtual void OnEnable()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
		{
			m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

	protected void OnSteamInitialisationFailed(string failMessage)
	{
		LogError(failMessage, this);
		m_bInitializationFailed = true;
		Application.Quit();
	}

	protected virtual void OnDestroy()
	{
		if (!(s_instance != this))
		{
			s_instance = null;
			if (m_bInitialized)
			{
				Log("Shutting down", this);
				SteamAPI.Shutdown();
			}
		}
	}

	protected virtual void Update()
	{
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}

	public void Log(string log)
	{
		Debug.Log("#" + ConsoleDebugLog + "#" + log);
	}

	public void Log(string log, UnityEngine.Object context)
	{
		Debug.Log("#" + ConsoleDebugLog + "#" + log, context);
	}

	public void LogError(string log)
	{
		Debug.LogError("#" + ConsoleDebugLog + "#" + log);
	}

	public void LogError(string log, UnityEngine.Object context)
	{
		Debug.LogError("#" + ConsoleDebugLog + "#" + log, context);
	}

	public void LogWarning(string log)
	{
		Debug.LogWarning("#" + ConsoleDebugLog + "#" + log);
	}

	public void LogWarning(string log, UnityEngine.Object context)
	{
		Debug.LogWarning("#" + ConsoleDebugLog + "#" + log, context);
	}
}
