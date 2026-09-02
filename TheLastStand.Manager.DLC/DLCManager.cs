using System;
using System.Collections.Generic;
using System.Text;
using PortraitAPI;
using Steamworks;
using TPLib.Log;
using TheLastStand.Definition.DLC;
using UnityEngine;

namespace TheLastStand.Manager.DLC;

public class DLCManager : Manager<DLCManager>
{
	[SerializeField]
	private DLCDefinition[] dlcDefinitions;

	private Dictionary<string, DLCDefinition> dlcDefinitionsById;

	public static bool Initialized { get; private set; }

	public Dictionary<string, bool> CachedIsDLCOwned { get; } = new Dictionary<string, bool>();

	public HashSet<string> OwnedDLCIds { get; } = new HashSet<string>();

	public static event Action OnInitialized;

	public DLCDefinition GetDLCFromId(string id)
	{
		if (!dlcDefinitionsById.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public bool IsDLCOwned(string id)
	{
		DLCDefinition dLCFromId = GetDLCFromId(id);
		if (dLCFromId == null)
		{
			LogError("Couldn't find DLC " + id + " definition !", CLogLevel.DETAILED);
			return false;
		}
		if (CachedIsDLCOwned.ContainsKey(id))
		{
			return CachedIsDLCOwned[id];
		}
		if (SteamManager.Initialized)
		{
			bool flag = SteamApps.BIsDlcInstalled((AppId_t)dLCFromId.SteamSpecificData.Id);
			AddCachedIsDLCOwned(id, flag);
			return flag;
		}
		LogError("Couldn't check if DLC " + id + " is owned, no available api could be called !", CLogLevel.DETAILED);
		return false;
	}

	public void LogOwnedDLCs()
	{
		StringBuilder stringBuilder = new StringBuilder("Owned DLCs: ");
		stringBuilder.Append(string.Join(", ", OwnedDLCIds));
		Log(stringBuilder, CLogLevel.MAJOR);
	}

	protected void AddCachedIsDLCOwned(string dlcId, bool isOwned)
	{
		if (!CachedIsDLCOwned.ContainsKey(dlcId))
		{
			CachedIsDLCOwned.Add(dlcId, isOwned);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (base._IsValid)
		{
			Init();
		}
	}

	private void Init()
	{
		dlcDefinitionsById = new Dictionary<string, DLCDefinition>();
		DLCDefinition[] array = dlcDefinitions;
		foreach (DLCDefinition dLCDefinition in array)
		{
			dlcDefinitionsById.Add(dLCDefinition.Id, dLCDefinition);
			if (IsDLCOwned(dLCDefinition.Id) && !OwnedDLCIds.Contains(dLCDefinition.Id))
			{
				OwnedDLCIds.Add(dLCDefinition.Id);
			}
		}
		PortraitAPIManager.InitOwnedDLCIds(OwnedDLCIds);
		Initialized = true;
		DLCManager.OnInitialized();
	}

	static DLCManager()
	{
		DLCManager.OnInitialized = delegate
		{
		};
	}
}
