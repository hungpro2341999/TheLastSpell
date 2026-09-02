using System.IO;
using Steamworks;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Modding;
using TheLastStand.Manager.Modding;
using TheLastStand.Model.Modding;
using UnityEngine.Events;

namespace TheLastStand.Manager.SteamWorkshop;

public class SteamModLoader : ModLoader<SteamModLoader>
{
	public class Constants
	{
		public const int MaxItemsPerRequest = 50;
	}

	private CallResult<SteamUGCQueryCompleted_t> onGetSubscribedItemsQueryCompletedResult;

	private CallResult<SteamUGCQueryCompleted_t> onGetPublishedItemsQueryCompletedResult;

	private UGCQueryHandle_t getPublishedItemsQueryHandle;

	private UGCQueryHandle_t getSubscribedItemsQueryHandle;

	private bool publishedModsLoaded;

	private uint publishedItemspagesIndex = 1u;

	private uint subscribedItemsPagesIndex = 1u;

	private bool subscribedModsLoaded;

	private EUGCMatchingUGCType workshopItemType;

	public override bool ModsLoaded
	{
		get
		{
			if (subscribedModsLoaded)
			{
				return publishedModsLoaded;
			}
			return false;
		}
	}

	public UnityEvent OnPublishedItemsLoaded { get; set; } = new UnityEvent();

	public override void Init()
	{
		if (!initialized)
		{
			onGetSubscribedItemsQueryCompletedResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnGetSubscribedItemsQueryCompleted);
			onGetPublishedItemsQueryCompletedResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnGetPublishedItemsQueryCompleted);
			initialized = true;
		}
	}

	public bool CanUserUploadMod(Mod mod)
	{
		return ModManager.PublishedMods.Find((Mod x) => x.WorkshopId == mod.WorkshopId) != null;
	}

	public override void LoadMods()
	{
		subscribedModsLoaded = false;
		subscribedItemsPagesIndex = 1u;
		getSubscribedItemsQueryHandle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Subscribed, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, AppId_t.Invalid, SteamUtils.GetAppID(), subscribedItemsPagesIndex);
		SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(getSubscribedItemsQueryHandle);
		onGetSubscribedItemsQueryCompletedResult.Set(hAPICall);
		LoadPublishedMods();
	}

	public void LoadPublishedMods()
	{
		ModManager.PublishedMods.Clear();
		publishedModsLoaded = false;
		publishedItemspagesIndex = 1u;
		getPublishedItemsQueryHandle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, AppId_t.Invalid, SteamUtils.GetAppID(), publishedItemspagesIndex);
		SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(getPublishedItemsQueryHandle);
		onGetPublishedItemsQueryCompletedResult.Set(hAPICall);
	}

	private void CreateMod(SteamUGCDetails_t item)
	{
		if (SteamUGC.GetItemInstallInfo(item.m_nPublishedFileId, out var _, out var pchFolder, 1024u, out var _))
		{
			Mod mod = new ModController(new DirectoryInfo(pchFolder), item).Mod;
			if (mod.Version >= ModManager.ModMinVersion && mod.Modules.Count > 0)
			{
				TPSingleton<ModManager>.Instance.Log("[STEAM] <b>" + mod.ToString() + "</b> is correctly installed ! Adding it to Subscribed Mods list." + mod.ModulesToString(), CLogLevel.DETAILED);
				ModManager.SubscribedMods.Add(mod);
				return;
			}
			TPSingleton<ModManager>.Instance.Log($"[STEAM] <b>{mod}</b> is outdated (Mod Version : {mod.Version}) ! (Min Version supported : {ModManager.ModMinVersion}, Max Version supported : {ModManager.ModVersion}) Adding it to Outdated Mods list.", CLogLevel.DETAILED);
			mod.IsIncompatible = true;
			ModManager.OutdatedMods.Add(mod);
		}
	}

	private void OnGetPublishedItemsQueryCompleted(SteamUGCQueryCompleted_t param, bool bIOFailure)
	{
		string text = string.Empty;
		TPSingleton<ModManager>.Instance.Log($"Found {param.m_unNumResultsReturned} published mods by this user ! (Page : {publishedItemspagesIndex})", CLogLevel.MAJOR);
		for (uint num = 0u; num < param.m_unNumResultsReturned; num++)
		{
			SteamUGC.GetQueryUGCResult(getPublishedItemsQueryHandle, num, out var pDetails);
			Mod mod = new ModController(null, pDetails).Mod;
			ModManager.PublishedMods.Add(mod);
			text = text + " - " + mod.ToString() + mod.ModulesToString();
			if (num != param.m_unNumResultsReturned - 1)
			{
				text += "\r\n";
			}
		}
		TPSingleton<ModManager>.Instance.Log($"Published mods by this user (Page : {publishedItemspagesIndex}): \r\n{text}", CLogLevel.DETAILED);
		SteamUGC.ReleaseQueryUGCRequest(getPublishedItemsQueryHandle);
		if (param.m_unNumResultsReturned == 50 && param.m_unTotalMatchingResults > 50)
		{
			publishedModsLoaded = false;
			publishedItemspagesIndex++;
			getPublishedItemsQueryHandle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Published, workshopItemType, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, AppId_t.Invalid, SteamUtils.GetAppID(), subscribedItemsPagesIndex);
			SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(getPublishedItemsQueryHandle);
			onGetPublishedItemsQueryCompletedResult.Set(hAPICall);
		}
		else
		{
			publishedModsLoaded = true;
			OnPublishedItemsLoaded?.Invoke();
		}
	}

	private void OnGetSubscribedItemsQueryCompleted(SteamUGCQueryCompleted_t param, bool bIOFailure)
	{
		PublishedFileId_t[] array = new PublishedFileId_t[param.m_unNumResultsReturned];
		SteamUGC.GetSubscribedItems(array, (uint)array.Length);
		TPSingleton<ModManager>.Instance.Log($"Found {param.m_unNumResultsReturned} subscribed mods from steam ! (Page : {subscribedItemsPagesIndex})", CLogLevel.MAJOR);
		for (uint num = 0u; num < array.Length; num++)
		{
			SteamUGC.GetQueryUGCResult(getSubscribedItemsQueryHandle, num, out var pDetails);
			EItemState itemState = (EItemState)SteamUGC.GetItemState(array[num]);
			SteamUGC.GetItemInstallInfo(array[num], out var _, out var pchFolder, 1024u, out var _);
			if (itemState.HasFlag(EItemState.k_EItemStateInstalled) && Directory.Exists(pchFolder))
			{
				CreateMod(pDetails);
			}
			else
			{
				TPSingleton<ModManager>.Instance.Log($"This mod {pDetails.m_rgchTitle}(Steam Workshop Id : {pDetails.m_nPublishedFileId}) isn't correctly installed !", CLogLevel.DETAILED);
			}
		}
		SteamUGC.ReleaseQueryUGCRequest(getSubscribedItemsQueryHandle);
		if (param.m_unNumResultsReturned == 50 && param.m_unTotalMatchingResults > 50)
		{
			subscribedModsLoaded = false;
			subscribedItemsPagesIndex++;
			getSubscribedItemsQueryHandle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Published, workshopItemType, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, AppId_t.Invalid, SteamUtils.GetAppID(), subscribedItemsPagesIndex);
			SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(getSubscribedItemsQueryHandle);
			onGetSubscribedItemsQueryCompletedResult.Set(hAPICall);
		}
		else
		{
			subscribedModsLoaded = true;
		}
	}
}
