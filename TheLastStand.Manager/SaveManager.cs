using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Steamworks;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Database;
using TheLastStand.Database.WorldMap;
using TheLastStand.Definition.Meta;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Encryption;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using TheLastStand.Model;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Apocalypse;
using TheLastStand.Serialization.Item.ItemRestriction;
using TheLastStand.Serialization.Meta;
using TheLastStand.View.Menus;
using TheLastStand.View.SaveSlots;
using UnityEngine;

namespace TheLastStand.Manager;

public class SaveManager : Manager<SaveManager>
{
	public enum E_BrokenSaveReason
	{
		UNKNOWN,
		WRONG_VERSION,
		LOADING_ERROR,
		FILE_NOT_FOUND,
		MISSING_MOD,
		MISSING_DLC
	}

	public class Constants
	{
		public const string SaveFolderName = "Save";

		public const string SteamSaveSubFolderName = "Steam";

		public const string LocalSaveSubFolderName = "Local";

		public const string BackupFolderName = "Backups";

		public const string BackupMigrationFolderName = "BackupMigration";

		public const string AppSaveName = "AppSave";

		public const string GameSaveName = "GameSave";

		public const string AppSaveBetaName = "AppSaveBeta";

		public const string GameSaveBetaName = "GameSaveBeta";
	}

	public const int MaxProfileAmount = 5;

	private static string persistentDataPath;

	private static string cachedAppSaveFileName;

	private static string cachedGameSaveFileName;

	[SerializeField]
	private int appVersionTriggeringMigration = 8;

	[SerializeField]
	[Range(0f, 4f)]
	private int profile;

	[SerializeField]
	[Tooltip("Application Save version")]
	private byte appSaveVersion = 1;

	[SerializeField]
	[Tooltip("Game Save version")]
	private byte gameSaveVersion = 1;

	[SerializeField]
	[Tooltip("Settings Save version")]
	private byte settingsSaveVersion = 1;

	[SerializeField]
	[Tooltip("Minimum supported Application save version")]
	private int minimumAppSaveVersion = 1;

	[SerializeField]
	[Tooltip("Minimum supported Game save version")]
	private int minimumGameSaveVersion = 1;

	[SerializeField]
	[Tooltip("Minimum supported Settings save version")]
	private int minimumSettingsSaveVersion = 1;

	[SerializeField]
	[Tooltip("Disable save file encryption")]
	private bool disableSaveEncryption;

	[SerializeField]
	[Tooltip("Save serialization format")]
	private SaverLoader.E_SaveFormat saveFormat;

	[SerializeField]
	[Tooltip("Launching application with one of those version will result in trying to backup save files")]
	private int[] appVersionsTriggeringBackup;

	[SerializeField]
	[Tooltip("Don't compare version of current save")]
	private bool debugDontCompareFileVersion;

	[SerializeField]
	private DevConsole devConsole;

	public static bool NewlyPreloadedGameSave = false;

	public static byte AppSaveVersion => TPSingleton<SaveManager>.Instance.appSaveVersion;

	public static List<LoadFailedInfos> LoadFailedInfos { get; set; } = new List<LoadFailedInfos>();

	public static int CurrentProfileIndex
	{
		get
		{
			return TPSingleton<SaveManager>.Instance.profile;
		}
		set
		{
			TPSingleton<SaveManager>.Instance.profile = value;
		}
	}

	public static byte GameSaveVersion => TPSingleton<SaveManager>.Instance.gameSaveVersion;

	public static bool IsSaveEncryptionDisabled => TPSingleton<SaveManager>.Instance.disableSaveEncryption;

	public static int MinimumSupportedAppSaveVersion => TPSingleton<SaveManager>.Instance.minimumAppSaveVersion;

	public static int MinimumSupportedGameSaveVersion => TPSingleton<SaveManager>.Instance.minimumGameSaveVersion;

	public static int MinimumSupportedSettingsSaveVersion => TPSingleton<SaveManager>.Instance.minimumSettingsSaveVersion;

	public static string PersistentDataPath
	{
		get
		{
			if (string.IsNullOrEmpty(persistentDataPath))
			{
				persistentDataPath = UnityEngine.Application.persistentDataPath;
			}
			return persistentDataPath;
		}
	}

	public static byte SettingsSaveVersion => TPSingleton<SaveManager>.Instance.settingsSaveVersion;

	public static SaverLoader.E_SaveFormat SaveFormat => TPSingleton<SaveManager>.Instance.saveFormat;

	public static string AppSaveFileName
	{
		get
		{
			if (string.IsNullOrEmpty(cachedAppSaveFileName))
			{
				cachedAppSaveFileName = ((ApplicationManager.BuildType != ApplicationManager.E_BuildType.Beta) ? "AppSave" : "AppSaveBeta");
				return cachedAppSaveFileName;
			}
			return cachedAppSaveFileName;
		}
	}

	public static string GameSaveFileName
	{
		get
		{
			if (string.IsNullOrEmpty(cachedGameSaveFileName))
			{
				cachedGameSaveFileName = ((ApplicationManager.BuildType != ApplicationManager.E_BuildType.Beta) ? "GameSave" : "GameSaveBeta");
				return cachedGameSaveFileName;
			}
			return cachedGameSaveFileName;
		}
	}

	public string FormerAppSaveFilePath => PersistentDataPath + "/Save/" + AppSaveFileName + "." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");

	public bool FormerAppSaveExists => File.Exists(FormerAppSaveFilePath);

	public string FormerGameSaveFilePath => PersistentDataPath + "/Save/" + GameSaveFileName + "." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");

	public bool FormerGameSaveExists => File.Exists(FormerGameSaveFilePath);

	public string FormerKeyRemappingSaveFilePath => PersistentDataPath + "/Save/InputMappingSave.xml";

	public bool FormerKeyRemappingSaveExists => File.Exists(FormerKeyRemappingSaveFilePath);

	public string FormerSettingSaveFilePath => PersistentDataPath + string.Format("/{0}/SettingsSave.{1}", "Save", SaveFormat);

	public bool FormerSettingSaveExists => File.Exists(FormerSettingSaveFilePath);

	public SaverLoader.SerializedContainerLoadingInfo<SerializedGameState> CurrentPreloadedGameSave
	{
		get
		{
			SaverLoader.SerializedContainerLoadingInfo<SerializedGameState>[] preloadedGameSaves = PreloadedGameSaves;
			if (preloadedGameSaves == null)
			{
				return null;
			}
			return preloadedGameSaves[CurrentProfileIndex];
		}
	}

	public SaverLoader.SerializedContainerLoadingInfo<SerializedGameState>[] PreloadedGameSaves { get; private set; }

	public SerializedApplicationState[] PreloadedAppSaves { get; private set; }

	public static int GetCurrentProfilePathIndex()
	{
		return GetProfilePathIndex(CurrentProfileIndex);
	}

	public static int GetProfilePathIndex(int profileIndex)
	{
		return profileIndex + 1;
	}

	public static bool DoesCurrentAppSaveExist()
	{
		return DoesAppSaveExist(CurrentProfileIndex);
	}

	public static bool DoesAppSaveExist(int profileIndex)
	{
		if (!File.Exists(GetAppSaveFilePath(profileIndex)))
		{
			return File.Exists(GetAppSaveBackupFilePath(profileIndex));
		}
		return true;
	}

	public static string GetCurrentAppSaveFilePath()
	{
		return GetAppSaveFilePath(CurrentProfileIndex);
	}

	public static string GetAppSaveFilePath(int profileIndex)
	{
		return PersistentDataPath + string.Format("/{0}/{1}/Profile{2}/{3}.{4}", "Save", GetSaveSubFolderPath(), profileIndex + 1, AppSaveFileName, IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");
	}

	public static string GetCurrentAppSaveBackupFilePath()
	{
		return GetCurrentBackupFolderPath() + "/" + AppSaveFileName + "__BACKUP." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");
	}

	public static string GetAppSaveBackupFilePath(int profileIndex)
	{
		return GetBackupFolderPath(profileIndex) + "/" + AppSaveFileName + "__BACKUP." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");
	}

	public static string GetCurrentAppSaveTemporaryBackupFilePath()
	{
		return GetCurrentBackupFolderPath() + "/" + AppSaveFileName + "__BACKUP_TMP." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");
	}

	public static string GetCurrentBackupFolderPath()
	{
		return GetBackupFolderPath(CurrentProfileIndex);
	}

	public static string GetBackupFolderPath(int profileIndex)
	{
		return PersistentDataPath + string.Format("/{0}/{1}/Profile{2}/{3}", "Save", GetSaveSubFolderPath(), GetProfilePathIndex(profileIndex), "Backups");
	}

	public static bool DoesCurrentGameSaveExist()
	{
		return DoesGameSaveExist(CurrentProfileIndex);
	}

	public static bool DoesGameSaveExist(int profileIndex)
	{
		if (!File.Exists(GetGameSaveFilePath(profileIndex)))
		{
			return File.Exists(GetGameSaveBackupFilePath(profileIndex));
		}
		return true;
	}

	public static string GetCurrentGameSaveBackupFilePath()
	{
		return GetGameSaveBackupFilePath(CurrentProfileIndex);
	}

	public static string GetGameSaveBackupFilePath(int profileIndex)
	{
		return GetBackupFolderPath(profileIndex) + "/" + GameSaveFileName + "__BACKUP." + (TPSingleton<SaveManager>.Instance.disableSaveEncryption ? TPSingleton<SaveManager>.Instance.saveFormat.ToString().ToLower() : "bin");
	}

	public static string GetGameSaveTemporaryBackupFilePath()
	{
		return GetCurrentBackupFolderPath() + "/" + GameSaveFileName + "__BACKUP_TMP." + (TPSingleton<SaveManager>.Instance.disableSaveEncryption ? TPSingleton<SaveManager>.Instance.saveFormat.ToString().ToLower() : "bin");
	}

	public static string GetCurrentGameSaveFilePath()
	{
		return GetGameSaveFilePath(CurrentProfileIndex);
	}

	public static string GetGameSaveFilePath(int profileIndex)
	{
		return PersistentDataPath + string.Format("/{0}/{1}/Profile{2}/{3}.{4}", "Save", GetSaveSubFolderPath(), GetProfilePathIndex(profileIndex), GameSaveFileName, TPSingleton<SaveManager>.Instance.disableSaveEncryption ? TPSingleton<SaveManager>.Instance.saveFormat.ToString().ToLower() : "bin");
	}

	public static string GetSettingsFilePath()
	{
		return string.Format("{0}/{1}/{2}/SettingsSave.{3}", PersistentDataPath, "Save", GetSaveSubFolderPath(), SaveFormat);
	}

	public static bool DoesSettingsSaveExist()
	{
		return File.Exists(GetSettingsFilePath());
	}

	public static void BackupSaveFilesBeforeVersionUpdate()
	{
		if (!DoesAppVersionTriggeredBackup() || !DoesCurrentAppSaveExist())
		{
			return;
		}
		try
		{
			string currentAppSaveFilePath = GetCurrentAppSaveFilePath();
			SerializedApplicationState serializedApplicationState = SaverLoader.Load<SerializedApplicationState>(currentAppSaveFilePath, !IsSaveEncryptionDisabled);
			if (!TPSingleton<SaveManager>.Instance.debugDontCompareFileVersion && serializedApplicationState.SaveVersion >= AppSaveVersion)
			{
				return;
			}
			string text = $"{serializedApplicationState.GameMajorVersion}.{serializedApplicationState.GameMinorVersion}.{serializedApplicationState.GamePatchVersion}.{serializedApplicationState.GameHotfixVersion}";
			string text2 = PersistentDataPath + "/Save/" + GetSaveSubFolderPath() + "/Backups/Backup_v" + text;
			bool flag = Directory.Exists(text2);
			if (flag && Directory.GetFiles(text2).Length != 0)
			{
				return;
			}
			TPSingleton<SaveManager>.Instance.Log("Backing up save files from version " + text + ".", CLogLevel.DETAILED, forcePrintInUnity: true);
			if (!flag)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(text2));
			}
			using (File.Open(currentAppSaveFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				SaverLoader.CopyFileTo(currentAppSaveFilePath, text2 + "/" + AppSaveFileName + "." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin"));
			}
			if (DoesCurrentGameSaveExist())
			{
				string currentGameSaveFilePath = GetCurrentGameSaveFilePath();
				using (File.Open(currentGameSaveFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					SaverLoader.CopyFileTo(currentGameSaveFilePath, text2 + "/" + GameSaveFileName + "." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin"));
					return;
				}
			}
		}
		catch (Exception ex)
		{
			TPSingleton<SaveManager>.Instance.LogError("Caught exception while backing up save files after major update. Error Message : " + ex.Message, CLogLevel.MAJOR, forcePrintInUnity: true, printStackTrace: false);
		}
	}

	public static void ChangeCurrentProfile(int profileIndex)
	{
		TPSingleton<MetaConditionManager>.Instance.EraseConditionsControllers();
		CurrentProfileIndex = Mathf.Clamp(profileIndex, 0, 4);
		TPSingleton<SaveManager>.Instance.Log($"Profile changed to : Profile{GetCurrentProfilePathIndex()}", CLogLevel.MAJOR);
		LoadApp();
		if (!DoesCurrentAppSaveExist())
		{
			SaveApp();
		}
		if (ScenesManager.IsSceneActive(ScenesManager.MainMenuSceneName))
		{
			TPSingleton<MainMenuView>.Instance.Refresh();
		}
		SettingsManager.Save();
	}

	public static string CorruptAppSave(int profileIndex)
	{
		string result = SaverLoader.MarkFileAsCorrupted(GetAppSaveFilePath(profileIndex));
		SaverLoader.MarkFileAsCorrupted(GetAppSaveBackupFilePath(profileIndex));
		CorruptGameSave(profileIndex);
		return result;
	}

	public static string CorruptGameSave(int profileIndex)
	{
		string result = SaverLoader.MarkFileAsCorrupted(GetGameSaveFilePath(profileIndex));
		SaverLoader.MarkFileAsCorrupted(GetGameSaveBackupFilePath(profileIndex));
		return result;
	}

	private static bool DoesAppVersionTriggeredBackup()
	{
		bool result = false;
		for (int i = 0; i < TPSingleton<SaveManager>.Instance.appVersionsTriggeringBackup.Length; i++)
		{
			if (TPSingleton<SaveManager>.Instance.appVersionsTriggeringBackup[i] == AppSaveVersion)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static void EraseSave(int profileIndex)
	{
		SaverLoader.Erase(GetAppSaveFilePath(profileIndex));
		SaverLoader.Erase(GetAppSaveBackupFilePath(profileIndex));
		GameManager.EraseSave(profileIndex);
		TPSingleton<MetaConditionManager>.Instance.EraseConditionsControllers();
		TPSingleton<SaveManager>.Instance.PreloadedAppSaves[profileIndex] = null;
		TPSingleton<SaveManager>.Instance.PreloadedGameSaves[profileIndex] = null;
	}

	public static E_BrokenSaveReason? GetBrokenSaveReasonFromException(Exception e)
	{
		if (e == null)
		{
			return null;
		}
		return (e is SaverLoader.FileDoesNotExistException) ? E_BrokenSaveReason.FILE_NOT_FOUND : ((e is SaverLoader.WrongSaveVersionException) ? E_BrokenSaveReason.WRONG_VERSION : ((e is SaverLoader.MissingDLCException) ? E_BrokenSaveReason.MISSING_DLC : ((e is SaverLoader.MissingModException) ? E_BrokenSaveReason.MISSING_MOD : ((e is SaverLoader.SaveLoadingFailedException) ? E_BrokenSaveReason.LOADING_ERROR : E_BrokenSaveReason.UNKNOWN))));
	}

	public static string GetSaveSubFolderPath()
	{
		return string.Format("{0}/{1}", "Steam", SteamUser.GetSteamID());
	}

	public static void PreloadAllProfileApp()
	{
		TPSingleton<SaveManager>.Instance.PreloadedAppSaves = new SerializedApplicationState[5];
		for (int i = 0; i < 5; i++)
		{
			PreloadProfileApp(i);
		}
	}

	private static void PreloadProfileApp(int profileIndex)
	{
		if (!DoesAppSaveExist(profileIndex))
		{
			return;
		}
		SerializedApplicationState serializedApplicationState;
		try
		{
			try
			{
				serializedApplicationState = TPSingleton<ApplicationManager>.Instance.TryLoad(profileIndex);
			}
			catch (Exception e)
			{
				serializedApplicationState = TPSingleton<ApplicationManager>.Instance.TryLoadBackup(profileIndex, e);
			}
		}
		catch (SaverLoader.WrongSaveVersionException ex)
		{
			TPSingleton<SaveManager>.Instance.LogError(ex.FilePath + ": Loading failed because of a wrong version, marking game save file corrupted as well", CLogLevel.MAJOR, forcePrintInUnity: true, printStackTrace: false);
			LoadFailedInfos.Add(new LoadFailedInfos(ex.FilePath, E_BrokenSaveReason.WRONG_VERSION));
			CorruptAppSave(profileIndex);
			serializedApplicationState = null;
		}
		catch (SaverLoader.FileDoesNotExistException ex2)
		{
			TPSingleton<SaveManager>.Instance.LogError(ex2.FilePath + ": Loading failed because of files were not found, marking game save file corrupted as well", CLogLevel.MAJOR, forcePrintInUnity: true, printStackTrace: false);
			LoadFailedInfos.Add(new LoadFailedInfos(ex2.FilePath, E_BrokenSaveReason.FILE_NOT_FOUND));
			CorruptAppSave(profileIndex);
			serializedApplicationState = null;
		}
		catch (SaverLoader.SaveLoadingFailedException ex3)
		{
			TPSingleton<SaveManager>.Instance.LogError(ex3.FilePath + ": Could not load the Application save, marking game save file corrupted as well", CLogLevel.MAJOR, forcePrintInUnity: true, printStackTrace: false);
			LoadFailedInfos.Add(new LoadFailedInfos(ex3.FilePath, E_BrokenSaveReason.LOADING_ERROR));
			CorruptAppSave(profileIndex);
			serializedApplicationState = null;
		}
		catch (Exception ex4)
		{
			TPSingleton<SaveManager>.Instance.LogError("Caught exception while loading " + AppSaveFileName + ". Error Message : " + ex4.Message, CLogLevel.MAJOR, forcePrintInUnity: true, printStackTrace: false);
			TPSingleton<SaveManager>.Instance.LogError($"A critical error occured while loading the appsave\n{ex4}\nPlease catch this specific exception earlier on and add a proper message for it.", CLogLevel.DETAILED);
			LoadFailedInfos.Add(new LoadFailedInfos(CorruptAppSave(profileIndex)));
			serializedApplicationState = null;
		}
		TPSingleton<SaveManager>.Instance.PreloadedAppSaves[profileIndex] = serializedApplicationState;
	}

	public static void LoadApp()
	{
		SerializedApplicationState serializedApplicationState = TPSingleton<SaveManager>.Instance.PreloadedAppSaves[CurrentProfileIndex];
		TPSingleton<ApplicationManager>.Instance.Deserialize(serializedApplicationState, ((int?)serializedApplicationState?.SaveVersion) ?? (-1));
		TPSingleton<SaveManager>.Instance.Log($"Application save loaded in Profile{GetCurrentProfilePathIndex()}!", CLogLevel.MAJOR);
	}

	public static void SafeLoadGameSaves()
	{
		TPSingleton<SaveManager>.Instance.PreloadedGameSaves = new SaverLoader.SerializedContainerLoadingInfo<SerializedGameState>[5];
		NewlyPreloadedGameSave = true;
		for (int i = 0; i < 5; i++)
		{
			if (DoesGameSaveExist(i))
			{
				Exception caughtException = null;
				Exception caughtException2 = null;
				SerializedGameState loadedContainer = SaverLoader.SafeLoad<SerializedGameState>(GetGameSaveFilePath(i), !IsSaveEncryptionDisabled, out caughtException, MinimumSupportedGameSaveVersion) ?? SaverLoader.SafeLoad<SerializedGameState>(GetGameSaveBackupFilePath(i), !IsSaveEncryptionDisabled, out caughtException2, MinimumSupportedGameSaveVersion);
				(E_BrokenSaveReason?, Exception) tuple = (GetBrokenSaveReasonFromException(caughtException), caughtException);
				(E_BrokenSaveReason?, Exception) tuple2 = (GetBrokenSaveReasonFromException(caughtException2), caughtException2);
				TPSingleton<SaveManager>.Instance.PreloadedGameSaves[i] = new SaverLoader.SerializedContainerLoadingInfo<SerializedGameState>
				{
					FailedLoadsInfo = new(E_BrokenSaveReason?, Exception)[2] { tuple, tuple2 },
					LoadedContainer = loadedContainer
				};
			}
		}
	}

	public static void SaveApp()
	{
		SaverLoader.EnqueueSave(E_SaveType.App);
	}

	private static void DeserializeProfileIndexFromSettings()
	{
		if (DoesSettingsSaveExist())
		{
			SerializedSettings serializedSettings = SaverLoader.Load<SerializedSettings>(GetSettingsFilePath(), useEncryption: false);
			if (serializedSettings != null)
			{
				CurrentProfileIndex = TPSingleton<SettingsManager>.Instance.DeserializeCurrentProfileIndex(serializedSettings, serializedSettings.SaveVersion);
				TPSingleton<SaveManager>.Instance.Log($"Set profile to Profile{GetCurrentProfilePathIndex()} from the settings save.", CLogLevel.MAJOR);
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (base._IsValid)
		{
			SaveEncoder.Initialize(new byte[32]
			{
				101, 99, 90, 70, 120, 51, 114, 74, 52, 102,
				52, 68, 75, 52, 101, 88, 84, 112, 100, 90,
				112, 90, 68, 76, 51, 107, 77, 85, 101, 53,
				67, 83
			}, new byte[8] { 20, 5, 8, 1, 255, 37, 19, 170 });
			TPSingleton<SaveManager>.Instance.Log($"Application current save versions: [App:{AppSaveVersion}] [Game:{GameSaveVersion}] [Settings:{SettingsSaveVersion}].", CLogLevel.MAJOR);
			if (FormerAppSaveExists && SaverLoader.Load<SerializedApplicationState>(FormerAppSaveFilePath, !IsSaveEncryptionDisabled).SaveVersion <= appVersionTriggeringMigration)
			{
				ExecuteMultiProfileAndAccountMigration();
			}
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent += OnApplicationStateChange;
			BackupSaveFilesBeforeVersionUpdate();
			PreloadAllProfileApp();
			DeserializeProfileIndexFromSettings();
			LoadApp();
			SaveApp();
			if (!(ApplicationManager.Application.State is SplashScreen))
			{
				SafeLoadGameSaves();
			}
		}
	}

	public void ExecuteMultiProfileAndAccountMigration()
	{
		TPSingleton<SaveManager>.Instance.Log("Trying to migrate save files to their new locations", CLogLevel.MAJOR);
		string text = Path.Combine(PersistentDataPath, "Save");
		string text2 = Path.Combine(text, GetSaveSubFolderPath());
		string text3 = text + "_BackupMigration";
		string text4 = Path.Combine(text, "Backups");
		string newPath = text3 + "/" + AppSaveFileName + "." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");
		string newPath2 = text3 + "/" + GameSaveFileName + "." + (IsSaveEncryptionDisabled ? SaveFormat.ToString().ToLower() : "bin");
		string newPath3 = $"{text3}/SettingsSave.{SaveFormat}";
		string newPath4 = text3 + "/InputMappingSave.xml";
		if (!Directory.Exists(text2))
		{
			TPSingleton<SaveManager>.Instance.Log("Create the new save folder : " + text2, CLogLevel.MAJOR);
			Directory.CreateDirectory(text2);
		}
		if (!Directory.Exists(text3))
		{
			TPSingleton<SaveManager>.Instance.Log("Create a BackupMigration folder at " + text3 + " ; to copy files outside cloud folders to keep them locally", CLogLevel.MAJOR);
			Directory.CreateDirectory(text3);
		}
		if (!DoesCurrentAppSaveExist() && FormerAppSaveExists)
		{
			SaverLoader.SafeCopyFileTo(FormerAppSaveFilePath, GetCurrentAppSaveFilePath());
			SaverLoader.SafeCopyFileTo(FormerAppSaveFilePath, newPath);
			SaverLoader.SafeDeleteFile(FormerAppSaveFilePath);
		}
		if (!DoesCurrentGameSaveExist() && FormerGameSaveExists)
		{
			SaverLoader.SafeCopyFileTo(FormerGameSaveFilePath, GetCurrentGameSaveFilePath());
			SaverLoader.SafeCopyFileTo(FormerGameSaveFilePath, newPath2);
			SaverLoader.SafeDeleteFile(FormerGameSaveFilePath);
		}
		if (!DoesSettingsSaveExist() && FormerSettingSaveExists)
		{
			SaverLoader.SafeCopyFileTo(FormerSettingSaveFilePath, GetSettingsFilePath());
			SaverLoader.SafeCopyFileTo(FormerSettingSaveFilePath, newPath3);
			SaverLoader.SafeDeleteFile(FormerSettingSaveFilePath);
		}
		if (!KeyRemappingSaverLoader.KeyRemappingSaveExists && FormerKeyRemappingSaveExists)
		{
			SaverLoader.SafeCopyFileTo(FormerKeyRemappingSaveFilePath, KeyRemappingSaverLoader.KeyRemappingSaveFilePath);
			SaverLoader.SafeCopyFileTo(FormerKeyRemappingSaveFilePath, newPath4);
			SaverLoader.SafeDeleteFile(FormerKeyRemappingSaveFilePath);
		}
		if (!Directory.Exists(Path.Combine(text2, "Backups")) && Directory.Exists(text4))
		{
			SaverLoader.SafeCopyDirectoryTo(text4, text2);
			SaverLoader.SafeCopyDirectoryTo(text4, text3);
			SaverLoader.SafeDeleteDirectory(text4);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (TPSingleton<ApplicationManager>.Exist())
		{
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent -= OnApplicationStateChange;
		}
	}

	private void OnApplicationStateChange(State state)
	{
		if (!(state is WorldMapState))
		{
			if (state is GameLobbyState)
			{
				SafeLoadGameSaves();
				PreloadProfileApp(CurrentProfileIndex);
			}
		}
		else
		{
			PreloadedGameSaves = null;
		}
	}

	[DevConsoleCommand("ChangeProfile")]
	public static void DebugChangeProfile(int profileIndex)
	{
		if (ApplicationManager.Application.State.GetName() == "GameLobby" && CurrentProfileIndex != profileIndex)
		{
			ChangeCurrentProfile(profileIndex);
			if (TPSingleton<SaveManager>.Instance.devConsole != null)
			{
				TPSingleton<SaveManager>.Instance.devConsole.Log($"Profile changed to : Profile{GetCurrentProfilePathIndex()}!");
			}
		}
	}

	[DevConsoleCommand("CreateEndGameSave")]
	public static void DebugCreateEndGameSave(int profileIndex = 0)
	{
		EraseSave(profileIndex);
		List<SerializedCity> list = new List<SerializedCity>();
		int num = 0;
		int apocalypseLevelCompletedToUnlock = ApocalypseDatabase.OrderedTierDefinitions[^1].ApocalypseLevelCompletedToUnlock;
		foreach (KeyValuePair<string, CityDefinition> cityDefinition in CityDatabase.CityDefinitions)
		{
			SerializedCity item = new SerializedCity
			{
				Id = cityDefinition.Key,
				SelectedGlyphs = new List<string>(),
				NumberOfRuns = 1,
				NumberOfWins = 1,
				MaxApoPassed = ((num == 1) ? apocalypseLevelCompletedToUnlock : 0)
			};
			list.Add(item);
			num++;
		}
		SerializedNarration serializedNarration = new SerializedNarration
		{
			AlreadyUsedReplicasIds = MetaNarrationDatabase.DarkGoddessNarrationDefinition.ReplicaDefinitions.Select((MetaReplicaDefinition replica) => replica.Id).ToList()
		};
		serializedNarration.AlreadyUsedReplicasIds.AddRange(MetaNarrationDatabase.DarkGoddessNarrationDefinition.MandatoryReplicaDefinitions.Select((MetaReplicaDefinition replica) => replica.Id));
		SerializedNarration serializedNarration2 = new SerializedNarration
		{
			AlreadyUsedReplicasIds = MetaNarrationDatabase.LightGoddessNarrationDefinition.ReplicaDefinitions.Select((MetaReplicaDefinition replica) => replica.Id).ToList()
		};
		serializedNarration2.AlreadyUsedReplicasIds.AddRange(MetaNarrationDatabase.LightGoddessNarrationDefinition.MandatoryReplicaDefinitions.Select((MetaReplicaDefinition replica) => replica.Id));
		List<SerializedMetaUpgrade> activatedUpgrades = MetaDatabase.MetaUpgradesDefinitions.Select((KeyValuePair<string, MetaUpgradeDefinition> upgrade) => new SerializedMetaUpgrade
		{
			Id = upgrade.Key
		}).ToList();
		uint count = (uint)CityDatabase.CityDefinitions.Count;
		TPSingleton<SaveManager>.Instance.PreloadedAppSaves[profileIndex] = new SerializedApplicationState
		{
			Cities = new SerializedCities
			{
				Cities = list
			},
			HasSeenIntroduction = true,
			DamnedSouls = 200000u,
			DamnedSoulsObtained = 200000u,
			DaysPlayed = 100u,
			RunsCompleted = count,
			RunsWon = count,
			TutorialDone = true,
			GlobalApocalypse = new SerializedGlobalApocalypse
			{
				MaxAvailableApocalypseIndex = apocalypseLevelCompletedToUnlock,
				ApocalypseModifiersUnlockSeen = ApocalypseDatabase.ModifierDefinitions.Keys.ToList(),
				SelectedModifierSteps = new List<SerializedApocalypseModifierStep>()
			},
			MetaNarrations = new SerializedNarrations
			{
				DarkNarration = serializedNarration,
				LightNarration = serializedNarration2
			},
			MetaShops = new SerializedMetaShops
			{
				MetaUpgradesAlreadySeen = MetaDatabase.MetaUpgradesDefinitions.Keys.ToList()
			},
			MetaUpgrades = new SerializedMetaUpgrades
			{
				ActivatedUpgrades = activatedUpgrades
			},
			TutorialsRead = TutorialDatabase.TutorialsDefinitions.Keys.ToList()
		};
		TPSingleton<SaveManager>.Instance.PreloadedAppSaves[profileIndex].UpdateHeader();
		ChangeCurrentProfile(profileIndex);
		if (TPSingleton<SaveSlotsPanel>.Instance.PopupState == SaveSlotsPanel.E_State.Opened)
		{
			TPSingleton<SaveSlotsPanel>.Instance.Refresh();
		}
	}

	[DevConsoleCommand("LoadAllProfile")]
	public static void DebugLoadAllProfile()
	{
		for (int i = 0; i < 5; i++)
		{
			string appSaveFilePath = GetAppSaveFilePath(i);
			if (!File.Exists(appSaveFilePath))
			{
				TPSingleton<SaveManager>.Instance.Log($"No save in profile {GetProfilePathIndex(i)}", CLogLevel.MAJOR);
				continue;
			}
			SerializedApplicationState serializedApplicationState = TPSingleton<ApplicationManager>.Instance.TryLoad(i);
			if (serializedApplicationState != null)
			{
				StringBuilder stringBuilder = new StringBuilder($"Slot {i}");
				string cityName = serializedApplicationState.Cities.SelectedCityId;
				SerializedCity serializedCity = serializedApplicationState.Cities.Cities.FirstOrDefault((SerializedCity serializedCity2) => serializedCity2.Id == cityName);
				if (serializedCity != null)
				{
					string text = (serializedCity.NumberOfRuns + 1).ToString();
					string text2 = cityName + ((serializedCity.NumberOfRuns > 0) ? (" #" + text) : string.Empty);
					stringBuilder.AppendLine();
					stringBuilder.AppendLine(text2 ?? "");
					stringBuilder.AppendLine($"Difficulty: {CityDatabase.CityDefinitions[serializedCity.Id].DifficultySkullsNb}");
					appSaveFilePath = GetGameSaveFilePath(i);
					if (File.Exists(appSaveFilePath))
					{
						Exception caughtException;
						SerializedGameState serializedGameState = SaverLoader.SafeLoad<SerializedGameState>(appSaveFilePath, !IsSaveEncryptionDisabled, out caughtException, MinimumSupportedGameSaveVersion);
						if (serializedGameState != null)
						{
							stringBuilder.AppendLine(TimeSpan.FromSeconds(serializedGameState.TotalTimeSpent).ToString("hh\\:mm\\:ss"));
							stringBuilder.AppendLine(File.GetLastWriteTime(appSaveFilePath).ToString("yyyy/MM/dd HH:mm"));
							int num = ((serializedGameState.Game.Cycle == Game.E_Cycle.Day) ? serializedGameState.Game.DayNumber : serializedGameState.Game.NightHour);
							string arg = ((serializedGameState.Game.Cycle == Game.E_Cycle.Day) ? serializedGameState.Game.DayTurn.ToString() : $"Turn {serializedGameState.Game.NightHour}");
							string arg2 = ((serializedGameState.Game.Cycle != Game.E_Cycle.Night || serializedGameState.BossData.BossPhase == null) ? serializedGameState.Game.Cycle.ToString() : "Boss Night");
							stringBuilder.AppendLine($"{arg2} {num} - {arg}");
							if (serializedGameState.Apocalypse.ApocalypseIndex > 0)
							{
								stringBuilder.AppendLine($"Apocalypse index: {serializedGameState.Apocalypse.ApocalypseIndex}");
							}
							else
							{
								stringBuilder.AppendLine("No apocalypse.");
							}
						}
					}
					int count = serializedCity.SelectedGlyphs.Count;
					if (count > 0)
					{
						stringBuilder.AppendLine("Omens:");
						for (int num2 = 0; num2 < count; num2++)
						{
							stringBuilder.AppendLine(serializedCity.SelectedGlyphs[num2] ?? "");
						}
					}
					else
					{
						stringBuilder.AppendLine("No omens.");
					}
					if (serializedApplicationState.ItemRestrictions.ItemFamilies.Find((SerializedItemRestrictionFamily restriction) => !restriction.IsSelected) != null)
					{
						stringBuilder.AppendLine("Has item restriction.");
					}
					else
					{
						stringBuilder.AppendLine("No item restriction.");
					}
				}
				else
				{
					stringBuilder.AppendLine("Campaign map.");
				}
				stringBuilder.AppendLine($"{serializedApplicationState.DamnedSoulsObtained} tainted essence obtained");
				List<SerializedMetaUpgrade> list = new List<SerializedMetaUpgrade>(serializedApplicationState.MetaUpgrades.LockedUpgrades);
				list.AddRange(serializedApplicationState.MetaUpgrades.ActivatedUpgrades);
				list.AddRange(serializedApplicationState.MetaUpgrades.FullfilledUpgrades);
				list.AddRange(serializedApplicationState.MetaUpgrades.UnlockedUpgrades);
				int count2 = list.FindAll((SerializedMetaUpgrade upgrade) => MetaDatabase.MetaUpgradesDefinitions[upgrade.Id].DamnedSoulsShop).Count;
				int count3 = serializedApplicationState.MetaUpgrades.ActivatedUpgrades.FindAll((SerializedMetaUpgrade upgrade) => MetaDatabase.MetaUpgradesDefinitions[upgrade.Id].DamnedSoulsShop).Count;
				stringBuilder.AppendLine($"Freude's favor: {serializedApplicationState.MetaUpgrades.ActivatedUpgrades.Count - count3}/{list.Count - count2}.");
				stringBuilder.AppendLine($"Schaden's favor: {count3}/{count2}.");
				TPSingleton<SaveManager>.Instance.Log(stringBuilder.ToString(), CLogLevel.MAJOR);
			}
			else
			{
				TPSingleton<SaveManager>.Instance.Log($"No save in profile {i}", CLogLevel.MAJOR);
			}
		}
	}

	[DevConsoleCommand("EraseSave")]
	public static void DebugEraseSave(int profileIndex)
	{
		if (ScenesManager.IsActiveSceneLevel())
		{
			TPSingleton<SaveManager>.Instance.devConsole.Log("Can not erase a save in the level scene.");
			return;
		}
		EraseSave(profileIndex);
		LoadApp();
		ApocalypseManager.SetApocalypse(null);
		GlyphManager.ResetSelectedGlyphs();
		SafeLoadGameSaves();
		TPSingleton<MainMenuView>.Instance.Refresh();
		TPSingleton<SaveManager>.Instance.devConsole.Log($"Save erased in profile {GetProfilePathIndex(profileIndex)}.");
	}

	[DevConsoleCommand("SaveTransferLocalToSteamFolder")]
	public static void DebugTransferLocalSaveToSteamFolder()
	{
		if (!ScenesManager.IsSceneActive(ScenesManager.MainMenuSceneName))
		{
			return;
		}
		if (!SteamAPI.IsSteamRunning())
		{
			CLoggerManager.Log("Can't transfer Local save to Steam folder because Steam isn't launched !", LogType.Error);
			return;
		}
		string path = Path.Combine(PersistentDataPath, "Save");
		if (!Directory.Exists(Path.Combine(path, "Local")))
		{
			return;
		}
		string text = Path.Combine(PersistentDataPath, "Save", "Steam", SteamUser.GetSteamID().ToString());
		SaverLoader.SafeCopyFileTo(Path.Combine(path, "Local", $"SettingsSave.{SaveFormat}"), Path.Combine(text, $"SettingsSave.{SaveFormat}"));
		SaverLoader.SafeCopyFileTo(Path.Combine(path, "Local", "InputMappingSave.xml"), Path.Combine(text, "InputMappingSave.xml"));
		for (int i = 1; i <= 5; i++)
		{
			string text2 = Path.Combine(path, "Local", $"Profile{i}");
			if (Directory.Exists(text2))
			{
				SaverLoader.SafeCopyDirectoryTo(text2, text);
			}
		}
		TPSingleton<MetaConditionManager>.Instance.EraseConditionsControllers();
		LoadApp();
		TPSingleton<SettingsManager>.Instance.Init();
		TPSingleton<MainMenuView>.Instance.Refresh();
	}
}
