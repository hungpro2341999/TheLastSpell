using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Localization.ScriptableObjects;
using TPLib.Log;
using TheLastStand.Manager.SteamWorkshop;
using TheLastStand.Model.Modding;
using TheLastStand.Model.Modding.Module;
using UnityEngine;

namespace TheLastStand.Manager.Modding;

public class ModManager : Manager<ModManager>
{
	public delegate void ModsLoadedHandler();

	[SerializeField]
	private uint modVersion = 2u;

	[SerializeField]
	private uint modMinVersion = 1u;

	public static bool IsModder;

	private readonly WaitUntil waitUntilSteamManagerInitialized = new WaitUntil(() => SteamManager.Initialized || SteamManager.InitializationFailed);

	private readonly WaitUntil waitUntilSteamModsLoaded = new WaitUntil(() => ModLoader<SteamModLoader>.Instance.ModsLoaded);

	private List<Mod> localizationMods = new List<Mod>();

	private List<Mod> fontMods = new List<Mod>();

	private List<Mod> perksMods = new List<Mod>();

	private readonly HashSet<string> modIdsInUse = new HashSet<string>();

	public static bool GameHasMods => SubscribedMods.Count > 0;

	public static uint ModVersion => TPSingleton<ModManager>.Instance.modVersion;

	public static uint ModMinVersion => TPSingleton<ModManager>.Instance.modMinVersion;

	public static List<Mod> PublishedMods { get; } = new List<Mod>();

	public static List<Mod> SubscribedMods { get; } = new List<Mod>();

	public static List<Mod> OutdatedMods { get; } = new List<Mod>();

	public static HashSet<string> ModIdsInUse
	{
		get
		{
			if (!TPSingleton<ModManager>.Exist())
			{
				return new HashSet<string>();
			}
			return TPSingleton<ModManager>.Instance.modIdsInUse;
		}
	}

	public event ModsLoadedHandler OnModsLoadedEvent;

	public static void ManageFilesAtPath<T>(DirectoryInfo directoryInfo, ExternalFileLoader<T> modFileLoader) where T : class, new()
	{
		FileInfo[] array = null;
		try
		{
			array = directoryInfo.GetFiles();
		}
		catch (Exception message)
		{
			TPSingleton<ModManager>.Instance.LogError(message);
		}
		if (array != null)
		{
			modFileLoader.OnGetFiles(array);
		}
	}

	public IEnumerator LoadMods()
	{
		yield return CrossPlatformModsLoading();
		try
		{
			ModLoader<LocalModLoader>.Instance.LoadMods();
		}
		catch (Exception arg)
		{
			LogError($"Failed to load Local Mods, Exception : {arg}");
		}
		if (SubscribedMods.Count > 0)
		{
			fontMods = SubscribedMods.FindAll((Mod x) => x.ModController.HasModule<FontModule>());
			localizationMods = SubscribedMods.FindAll((Mod x) => x.ModController.HasModule<LocalizationModule>());
			perksMods = SubscribedMods.FindAll((Mod x) => x.ModController.HasModule<PerksModule>());
			foreach (Mod subscribedMod in SubscribedMods)
			{
				if (subscribedMod.IsUsed && subscribedMod.IsSaveBlocking)
				{
					modIdsInUse.Add(subscribedMod.Id);
				}
			}
			LinkFontAssembliesToLanguages();
			try
			{
				CheckModsUsage();
			}
			catch (Exception arg2)
			{
				LogError($"Failed to manage Mods, Exception : {arg2}");
			}
		}
		this.OnModsLoadedEvent?.Invoke();
	}

	private void LinkFontAssembliesToLanguages()
	{
		for (int i = 0; i < fontMods.Count; i++)
		{
			if (!fontMods[i].ModController.TryGetModule<FontModule>(out var module))
			{
				continue;
			}
			for (int j = 0; j < localizationMods.Count; j++)
			{
				if (!localizationMods[j].ModController.TryGetModule<LocalizationModule>(out var module2))
				{
					continue;
				}
				for (int k = 0; k < module.FontModuleController.FontAssemblies.Count; k++)
				{
					ModdedFontAssembly moddedFontAssembly = module.FontModuleController.FontAssemblies[k];
					if (module2.LocalizationModuleController.TryGetLinkedLanguages(moddedFontAssembly.Id, out var languages))
					{
						string[] array = languages;
						foreach (string language in array)
						{
							LinkFontAssemblyToLanguage(language, moddedFontAssembly);
						}
					}
				}
			}
		}
	}

	private void LinkFontAssemblyToLanguage(string language, ModdedFontAssembly fontAssembly)
	{
		if (!FontManager.EveryFontAssemblies.ContainsKey(language))
		{
			FontManager.EveryFontAssemblies.Add(language, new List<FontAssembly> { fontAssembly });
		}
		else if (FontSettings.LanguageCanSupportsMultipleFontAssembly)
		{
			FontManager.EveryFontAssemblies[language].Add(fontAssembly);
		}
		else
		{
			FontManager.EveryFontAssemblies[language][0] = fontAssembly;
		}
	}

	private IEnumerator CrossPlatformModsLoading()
	{
		yield return waitUntilSteamManagerInitialized;
		if (!SteamManager.InitializationFailed)
		{
			ModLoader<SteamModLoader>.Instance.Init();
			ModLoader<SteamModLoader>.Instance.LoadMods();
			yield return waitUntilSteamModsLoaded;
		}
		else
		{
			TPSingleton<ModManager>.Instance.LogWarning("SteamManager can't be initialized ! Can't load Steam Workshop Mods !", CLogLevel.MAJOR);
		}
	}

	public void StartLoadMods()
	{
		StartCoroutine(LoadMods());
	}

	private void CheckModsUsage()
	{
		string text = string.Empty;
		string text2 = string.Empty;
		int modIndex;
		for (modIndex = 0; modIndex < SubscribedMods.Count; modIndex++)
		{
			if (modIndex != SubscribedMods.Count - 1 && SubscribedMods.GetRange(modIndex + 1, SubscribedMods.Count - (modIndex + 1)).Find((Mod x) => x.WorkshopId == SubscribedMods[modIndex].WorkshopId) != null && SubscribedMods[modIndex].Location == Mod.E_ModLocation.STEAM)
			{
				Log($"This Mod from steam : {SubscribedMods[modIndex].Item.Value.m_rgchTitle} (Steam Workshop Id : {SubscribedMods[modIndex].Item.Value.m_nPublishedFileId}) is loaded but not used !", CLogLevel.DETAILED);
				SubscribedMods[modIndex].IsUsed = false;
				text2 += $" - {SubscribedMods[modIndex]})\r\n";
			}
			else
			{
				text += $" - {SubscribedMods[modIndex]}\r\n";
			}
		}
		Log($"Loaded {SubscribedMods.Count} Mods ({SubscribedMods.Count((Mod x) => x.Location == Mod.E_ModLocation.STEAM)} from steam and {SubscribedMods.Count((Mod x) => x.Location == Mod.E_ModLocation.LOCAL)} from local storage) and there are {SubscribedMods.FindAll((Mod x) => x.IsUsed).Count} used mods ({SubscribedMods.Count((Mod x) => x.IsUsed && x.Location == Mod.E_ModLocation.STEAM)} from steam and {SubscribedMods.Count((Mod x) => x.IsUsed && x.Location == Mod.E_ModLocation.LOCAL)} from local storage) !", CLogLevel.MAJOR);
		if (text != string.Empty)
		{
			Log("Mods used : \r\n" + text, CLogLevel.DETAILED);
		}
		if (text2 != string.Empty)
		{
			Log("Mods unused : \r\n" + text2, CLogLevel.DETAILED);
		}
	}
}
