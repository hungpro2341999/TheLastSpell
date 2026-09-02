using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NaconAPI;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Database;
using TheLastStand.Definition.DLC;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.DLC;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Modding;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Apocalypse;
using TheLastStand.Serialization.Item.ItemRestriction;
using TheLastStand.Serialization.Meta;
using TheLastStand.View;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Manager;

[StringConverter(typeof(StringToTPSingletonConverter<ApplicationManager>))]
public class ApplicationManager : Manager<ApplicationManager>, ISerializable, IDeserializable
{
	public enum E_BuildType
	{
		Debug,
		PreAlpha,
		Alpha,
		Beta,
		Release
	}

	public static class Constants
	{
		public const string SteamPlatformVersionIdentifier = ".s";

		public const string GoGPlatformVersionIdentifier = ".g";
	}

	[SerializeField]
	private int majorVersion;

	[SerializeField]
	private int minorVersion = 1;

	[SerializeField]
	private int patchVersion = 1;

	[SerializeField]
	private int hotfixVersion;

	[SerializeField]
	private E_BuildType buildType;

	[SerializeField]
	private int logFilesToKeep = 5;

	[SerializeField]
	private float logBatchingFrequency = 1f;

	[SerializeField]
	private string currentStateName = string.Empty;

	private float lastBatchTime;

	public static TheLastStand.Model.Application Application { get; private set; }

	public static E_BuildType BuildType => TPSingleton<ApplicationManager>.Instance.buildType;

	public static string CurrentStateName
	{
		get
		{
			return TPSingleton<ApplicationManager>.Instance.currentStateName;
		}
		set
		{
			TPSingleton<ApplicationManager>.Instance.currentStateName = value;
		}
	}

	public static int LastLoadedVersion { get; private set; }

	public static int MajorVersion => TPSingleton<ApplicationManager>.Instance.majorVersion;

	public static int MinorVersion => TPSingleton<ApplicationManager>.Instance.minorVersion;

	public static int PatchVersion => TPSingleton<ApplicationManager>.Instance.patchVersion;

	public static int HotfixVersion => TPSingleton<ApplicationManager>.Instance.hotfixVersion;

	public static string VersionString => string.Format("v{0}.{1}.{2}.{3}{4}{5}{6}{7}", TPSingleton<ApplicationManager>.Instance.majorVersion, TPSingleton<ApplicationManager>.Instance.minorVersion, TPSingleton<ApplicationManager>.Instance.patchVersion, TPSingleton<ApplicationManager>.Instance.hotfixVersion, TPSingleton<ApplicationManager>.Instance.PlatformVersion, TPSingleton<ApplicationManager>.Instance.OwnedDLCVersion, TPSingleton<ApplicationManager>.Instance.BuildTypeString, ModManager.GameHasMods ? "_MODDED" : "");

	public string BuildTypeString => buildType switch
	{
		E_BuildType.Debug => "_debug", 
		E_BuildType.PreAlpha => "_pre-alpha", 
		E_BuildType.Alpha => "_alpha", 
		E_BuildType.Beta => "_beta", 
		_ => string.Empty, 
	};

	public string OwnedDLCVersion
	{
		get
		{
			if (!TPSingleton<DLCManager>.Exist())
			{
				return string.Empty;
			}
			if (TPSingleton<DLCManager>.Instance.OwnedDLCIds.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder(".");
				foreach (string ownedDLCId in TPSingleton<DLCManager>.Instance.OwnedDLCIds)
				{
					DLCDefinition dLCFromId = TPSingleton<DLCManager>.Instance.GetDLCFromId(ownedDLCId);
					if (dLCFromId != null)
					{
						stringBuilder.Append(dLCFromId.VersionIdentifier);
					}
				}
				return stringBuilder.ToString();
			}
			return string.Empty;
		}
	}

	public string PlatformVersion => ".s";

	[DevConsoleCommand("DamnedSouls", Options = DevConsoleCommandOptions.CanReadWrite)]
	public static string DebugDamnedSouls
	{
		get
		{
			return Application.DamnedSouls.ToString();
		}
		set
		{
			uint damnedSouls = Convert.ToUInt32(value, 10);
			Application.DamnedSouls = damnedSouls;
			switch (CurrentStateName)
			{
			case "Game":
				GameView.TopScreenPanel.TurnPanel.PhasePanel.RefreshSoulsText();
				_ = GameManager.CurrentStateName;
				_ = 16;
				if (GameManager.CurrentStateName == Game.E_State.MetaShops)
				{
					TPSingleton<DarkShopManager>.Instance.RefreshTexts();
				}
				break;
			case "MetaShops":
				TPSingleton<DarkShopManager>.Instance.RefreshTexts();
				break;
			}
		}
	}

	[DevConsoleCommand("DamnedSoulsObtainedTotal", Options = DevConsoleCommandOptions.CanRead)]
	public static string DebugDamnedSoulsObtained => Application.DamnedSoulsObtained.ToString();

	[DevConsoleCommand("IntroductionSeen", Options = DevConsoleCommandOptions.CanReadWrite)]
	public static bool DebugHasSeenIntroduction
	{
		get
		{
			return Application.HasSeenIntroduction;
		}
		set
		{
			Application.HasSeenIntroduction = value;
		}
	}

	public static void HandleGameOver(Game.E_GameOverCause cause)
	{
		Application.RunsCompleted++;
		if (cause == Game.E_GameOverCause.MagicSealsCompleted)
		{
			Application.RunsWon++;
		}
		if (Application.RunsCompleted == 1)
		{
			Application.DamnedSouls = (uint)TPSingleton<ResourceDatabase>.Instance.FirstRunDamnedSoulsGain;
			Application.DamnedSoulsObtained = (uint)TPSingleton<ResourceDatabase>.Instance.FirstRunDamnedSoulsGain;
		}
		else
		{
			TPSingleton<TrophyManager>.Instance.AutoGainDamnedSouls(cause != Game.E_GameOverCause.MagicSealsCompleted);
		}
	}

	public static void Quit()
	{
		TPSingleton<ApplicationManager>.Instance.StartCoroutine(TPSingleton<ApplicationManager>.Instance.QuitCoroutine());
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		LastLoadedVersion = saveVersion;
		SerializedApplicationState serializedApplicationState = container as SerializedApplicationState;
		TPSingleton<AchievementManager>.Instance.Deserialize(serializedApplicationState?.Achievements);
		TPSingleton<MetaUpgradesManager>.Instance.Deserialize(serializedApplicationState?.MetaUpgrades);
		TPSingleton<MetaConditionManager>.Instance.DeserializeFromAppSave(serializedApplicationState?.MetaConditions);
		MetaUpgradesManager.ActivateNewAvailableUpgradesInApplication();
		MetaUpgradesManager.RefreshLockedDLCMetaUpgrades();
		Application.DamnedSouls = serializedApplicationState?.DamnedSouls ?? 0;
		Application.DamnedSoulsObtained = serializedApplicationState?.DamnedSoulsObtained ?? 0;
		Application.HasSeenIntroduction = serializedApplicationState?.HasSeenIntroduction ?? false;
		Application.DaysPlayed = serializedApplicationState?.DaysPlayed ?? 0;
		Application.RunsCompleted = serializedApplicationState?.RunsCompleted ?? 0;
		Application.RunsWon = serializedApplicationState?.RunsWon ?? 0;
		Application.TutorialDone = serializedApplicationState?.TutorialDone ?? false;
		Application.ApplicationQuitInOraculum = serializedApplicationState?.ApplicationQuitInOraculum ?? false;
		Application.TutorialsRead = ((serializedApplicationState?.TutorialsRead != null) ? new List<string>(serializedApplicationState?.TutorialsRead) : new List<string>());
		TPSingleton<ApocalypseManager>.Instance.GlobalDeserialize(serializedApplicationState?.GlobalApocalypse);
		TPSingleton<WorldMapCityManager>.Instance.DeserializeCities(((int?)serializedApplicationState?.SaveVersion) ?? (-1), serializedApplicationState?.Cities);
		ApocalypseManager.RefreshTiersUnlockedState();
		TPSingleton<MetaShopsManager>.Instance.Deserialize(serializedApplicationState?.MetaShops, saveVersion);
		TPSingleton<MetaNarrationsManager>.Instance.Deserialize(serializedApplicationState?.MetaNarrations, saveVersion);
		TPSingleton<GlyphManager>.Instance.Deserialize(serializedApplicationState, saveVersion);
		TPSingleton<ItemRestrictionManager>.Instance.Deserialize(serializedApplicationState?.ItemRestrictions, saveVersion);
		if (saveVersion <= 12)
		{
			TPSingleton<AchievementManager>.Instance.TriggerApplicationBackwardCompatibility(saveVersion);
		}
	}

	public ISerializedData Serialize()
	{
		return new SerializedApplicationState
		{
			Achievements = TPSingleton<AchievementManager>.Instance.Serialize(),
			DamnedSouls = Application.DamnedSouls,
			DamnedSoulsObtained = Application.DamnedSoulsObtained,
			HasSeenIntroduction = Application.HasSeenIntroduction,
			DaysPlayed = Application.DaysPlayed,
			RunsCompleted = Application.RunsCompleted,
			RunsWon = Application.RunsWon,
			TutorialDone = Application.TutorialDone,
			ApplicationQuitInOraculum = Application.ApplicationQuitInOraculum,
			MetaUpgrades = (TPSingleton<MetaUpgradesManager>.Instance.Serialize() as SerializedMetaUpgrades),
			MetaConditions = (TPSingleton<MetaConditionManager>.Instance.SerializeToAppSave() as SerializedMetaConditions),
			GlobalApocalypse = (TPSingleton<ApocalypseManager>.Instance.GlobalSerialize() as SerializedGlobalApocalypse),
			Cities = (TPSingleton<WorldMapCityManager>.Instance.Serialize() as SerializedCities),
			MetaShops = (TPSingleton<MetaShopsManager>.Instance.Serialize() as SerializedMetaShops),
			MetaNarrations = (TPSingleton<MetaNarrationsManager>.Instance.Serialize() as SerializedNarrations),
			Glyphs = TPSingleton<GlyphManager>.Instance.Serialize(),
			TutorialsRead = new List<string>(Application.TutorialsRead),
			ItemRestrictions = (TPSingleton<ItemRestrictionManager>.Instance.Serialize() as SerializedItemRestrictions)
		};
	}

	protected override void Awake()
	{
		base.Awake();
		if (base._IsValid)
		{
			TPSingleton<DebugManager>.Instance.RegisterAssemblyCommands();
			TPGameVersion.__GetGameVersion = () => VersionString;
			if (logFilesToKeep <= 0)
			{
				LogError($"Please do not specify a number of logs to keep inferior or equal with 0! (currently specified: {logFilesToKeep}).\nI put this value to 1 as a precaution.", CLogLevel.DETAILED);
				logFilesToKeep = 1;
			}
			if (logBatchingFrequency < 0f)
			{
				LogError($"Please do not specify a log batching frequency inferior to 0! (currently specified: {logBatchingFrequency}).\nValue automatically set to 0.", CLogLevel.DETAILED);
				logBatchingFrequency = 0f;
			}
			CLoggerManager.Start("TLS", logBatchingFrequency, (byte)logFilesToKeep);
			Application = new ApplicationController().Application;
			InitState();
			Log("Game version: " + VersionString, CLogLevel.MAJOR);
			if (NaconAPIHandler.IsInitialized)
			{
				Log(Analytics.AnalyticsDebugLog ?? "", CLogLevel.MAJOR);
			}
			if (TPSingleton<DLCManager>.Exist())
			{
				TPSingleton<DLCManager>.Instance.LogOwnedDLCs();
			}
		}
	}

	private void InitState()
	{
		string text = SceneManager.GetActiveScene().name;
		if (Application.ApplicationQuitInOraculum)
		{
			Log("Initializing application state, considering last application execution has been quit while in the Oraculum.", CLogLevel.MAJOR);
		}
		if (text == ScenesManager.SplashSceneName)
		{
			Application.ApplicationController.SetState("SplashScreen");
		}
		else if (text == ScenesManager.MainMenuSceneName)
		{
			Application.ApplicationController.SetState("GameLobby");
		}
		else if (ScenesManager.IsActiveSceneLevel())
		{
			Application.ApplicationController.SetState(GameManager.DisableAutoLoad ? "NewGame" : "LoadGame");
		}
		else if (ScenesManager.IsActiveSceneWorldMap())
		{
			Application.ApplicationController.SetState("WorldMap");
		}
		else if (ScenesManager.IsActiveSceneMetaShop())
		{
			Application.ApplicationController.SetState("MetaShops");
		}
		else if (text == "Level Editor")
		{
			Application.ApplicationController.SetState("LevelEditor");
		}
		else if (text == ScenesManager.CreditsSceneName)
		{
			Application.ApplicationController.SetState("Credits");
		}
		else if (text == ScenesManager.AnimatedCutsceneSceneName)
		{
			Application.ApplicationController.SetState("AnimatedCutscene");
		}
		else
		{
			LogError("Unknown scene \"" + text + "\" to initialize application state.", CLogLevel.MAJOR);
		}
	}

	private IEnumerator QuitCoroutine()
	{
		yield return new WaitUntil(() => SaverLoader.AreSavesCompleted());
		UnityEngine.Application.Quit();
	}

	public SerializedApplicationState TryLoad(int profileIndex)
	{
		SerializedApplicationState serializedApplicationState = SaverLoader.Load<SerializedApplicationState>(SaveManager.GetAppSaveFilePath(profileIndex), !SaveManager.IsSaveEncryptionDisabled);
		TPSingleton<SaveManager>.Instance.Log($"Application file save version : {serializedApplicationState.SaveVersion}", CLogLevel.MAJOR, forcePrintInUnity: true);
		if (serializedApplicationState.SaveVersion < SaveManager.MinimumSupportedAppSaveVersion)
		{
			throw new SaverLoader.WrongSaveVersionException(SaveManager.GetAppSaveFilePath(profileIndex), shouldMarkAsCorrupted: true);
		}
		return serializedApplicationState;
	}

	public SerializedApplicationState TryLoadBackup(int profileIndex, Exception e)
	{
		string appSaveBackupFilePath = SaveManager.GetAppSaveBackupFilePath(profileIndex);
		if (File.Exists(appSaveBackupFilePath))
		{
			try
			{
				string appSaveFilePath = SaveManager.GetAppSaveFilePath(profileIndex);
				LoadFailedInfos loadFailedInfos = ((!(e is SaverLoader.SaveLoadingFailedException ex)) ? new LoadFailedInfos(SaverLoader.MarkFileAsCorrupted(appSaveFilePath), SaveManager.E_BrokenSaveReason.LOADING_ERROR) : new LoadFailedInfos(ex.FilePath, SaveManager.E_BrokenSaveReason.LOADING_ERROR));
				SerializedApplicationState serializedApplicationState = null;
				TPSingleton<SaveManager>.Instance.LogWarning("First AppSave loading failed! Trying to load BACKUP file.", CLogLevel.MAJOR);
				TPSingleton<SaveManager>.Instance.LogWarning($"Failed loading exception message : {e}", CLogLevel.MAJOR);
				serializedApplicationState = SaverLoader.Load<SerializedApplicationState>(SaveManager.GetCurrentAppSaveBackupFilePath(), !SaveManager.IsSaveEncryptionDisabled);
				TPSingleton<SaveManager>.Instance.Log($"Application Backup file save version : {serializedApplicationState.SaveVersion}", CLogLevel.MAJOR, forcePrintInUnity: true);
				if (serializedApplicationState.SaveVersion < SaveManager.MinimumSupportedAppSaveVersion)
				{
					throw new SaverLoader.WrongSaveVersionException(appSaveBackupFilePath, shouldMarkAsCorrupted: true);
				}
				SaverLoader.CopyFileTo(appSaveBackupFilePath, appSaveFilePath);
				loadFailedInfos.BackupHasBeenLoaded = true;
				SaveManager.LoadFailedInfos.Add(loadFailedInfos);
				return serializedApplicationState;
			}
			catch (Exception)
			{
				throw e;
			}
		}
		throw e;
	}

	private void Update()
	{
		if (CLoggerManager.LogBatchingFrequency > 0f && Time.time - lastBatchTime >= CLoggerManager.LogBatchingFrequency)
		{
			CLoggerManager.WriteBatches();
			lastBatchTime = Time.time;
		}
	}

	[DevConsoleCommand("SaveApplication")]
	public static void DebugSaveApplication()
	{
		SaveManager.SaveApp();
	}
}
