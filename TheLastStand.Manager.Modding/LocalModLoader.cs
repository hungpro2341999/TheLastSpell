using System;
using System.IO;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Modding;
using TheLastStand.Model.Modding;
using UnityEngine;

namespace TheLastStand.Manager.Modding;

public class LocalModLoader : ModLoader<LocalModLoader>
{
	public static string ModsFolderPath = Application.persistentDataPath + "/Mods/";

	public override void Init()
	{
	}

	public override void LoadMods()
	{
		if (!Directory.Exists(ModsFolderPath))
		{
			return;
		}
		DirectoryInfo[] directories = new DirectoryInfo(ModsFolderPath).GetDirectories();
		TPSingleton<ModManager>.Instance.Log($"Found {directories.Length} mods from local storage !", CLogLevel.MAJOR);
		foreach (DirectoryInfo directoryInfo in directories)
		{
			if (directoryInfo.GetDirectories() != null && directoryInfo.GetFiles() != null)
			{
				Mod mod = null;
				try
				{
					mod = new ModController(directoryInfo).Mod;
				}
				catch (Exception arg)
				{
					TPSingleton<ModManager>.Instance.LogError($"An unknown error occured during loading of a mod ! Exception : {arg}");
				}
				if (mod != null && mod.HasManifest)
				{
					if (mod.Modules.Count > 0 && mod.Version >= ModManager.ModMinVersion)
					{
						TPSingleton<ModManager>.Instance.Log($"[LOCAL] <b>{mod}</b> is correctly installed ! Adding it to Subscribed Mods list." + mod.ModulesToString(), CLogLevel.DETAILED);
						ModManager.SubscribedMods.Add(mod);
						continue;
					}
					TPSingleton<ModManager>.Instance.Log($"[LOCAL] <b>{mod}</b> is outdated (Mod Version : {mod.Version}) ! (Min Version supported : {ModManager.ModMinVersion}, Max Version supported : {ModManager.ModVersion}) Adding it to Outdated Mods list.", CLogLevel.DETAILED);
					mod.IsIncompatible = true;
					ModManager.OutdatedMods.Add(mod);
				}
				else
				{
					TPSingleton<ModManager>.Instance.Log("A local mod can't be loaded because there is no manifest.xml file or there is no valid modules ! (Path : " + directoryInfo.FullName + ")", CLogLevel.DETAILED);
				}
			}
			else
			{
				TPSingleton<ModManager>.Instance.Log("A local mod can't be loaded ! (Path : " + directoryInfo.FullName + ")", CLogLevel.DETAILED);
			}
		}
	}
}
