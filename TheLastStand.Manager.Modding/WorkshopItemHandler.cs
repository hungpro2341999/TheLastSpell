using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using TPLib;
using TPLib.Log;
using TheLastStand.Model.Modding;
using TheLastStand.View.Modding;
using UnityEngine.Events;

namespace TheLastStand.Manager.Modding;

public static class WorkshopItemHandler
{
	public enum E_AbortUploadReason
	{
		None,
		Unsupported,
		ThumbnailSizeExceeded,
		ThumbnailWrongFileType,
		MissingManifestFile,
		MissingModTitle,
		BadVersion,
		NoModules
	}

	public class Constants
	{
		public const string SteamWorkshopIdFileName = "/steam_workshop.txt";
	}

	public class OnUploadSteamItemEvent : UnityEvent<SubmitItemUpdateResult_t>
	{
	}

	public class OnCheckCreatedItemsByUserEvent : UnityEvent<List<ulong>>
	{
	}

	private static bool initialized = false;

	private static CallResult<CreateItemResult_t> onCreateItemCallbackResult = null;

	private static CallResult<DeleteItemResult_t> onDeleteItemCallbackResult = null;

	private static CallResult<SubmitItemUpdateResult_t> onSubmitItemCreationUpdateResultCallResult = null;

	private static CallResult<SubmitItemUpdateResult_t> onSubmitItemUpdateResultCallResult = null;

	private static UGCUpdateHandle_t updateHandle = default(UGCUpdateHandle_t);

	public static OnCheckCreatedItemsByUserEvent OnCheckCreatedItemsByUser = new OnCheckCreatedItemsByUserEvent();

	public static OnUploadSteamItemEvent OnUploadSteamItem = new OnUploadSteamItemEvent();

	public static OnUploadSteamItemEvent OnFailUploadSteamItem = new OnUploadSteamItemEvent();

	public static Mod ModToUpload { get; set; }

	public static void CreateItem(Mod mod)
	{
		ModToUpload = mod;
		if (SteamManager.Initialized && ModToUpload != null)
		{
			if (!initialized)
			{
				Init();
			}
			SteamAPICall_t hAPICall = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeFirst);
			onCreateItemCallbackResult.Set(hAPICall);
		}
	}

	public static void DeleteItem(PublishedFileId_t publishedFileId_T)
	{
		if (SteamManager.Initialized)
		{
			SteamAPICall_t hAPICall = SteamUGC.DeleteItem(publishedFileId_T);
			onDeleteItemCallbackResult.Set(hAPICall);
		}
	}

	public static void ModifyItem(Mod mod, string changeNote = null)
	{
		ModToUpload = mod;
		if (SteamManager.Initialized && ModToUpload != null)
		{
			if (!initialized)
			{
				Init();
			}
			updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), new PublishedFileId_t(ModToUpload.WorkshopId));
			if (ModToUpload.MustOverrideSteamDatas)
			{
				SteamUGC.SetItemTitle(updateHandle, ModToUpload.Title);
				SteamUGC.SetItemDescription(updateHandle, ModToUpload.Description);
			}
			if (ModToUpload.Thumbnail != null)
			{
				SteamUGC.SetItemPreview(updateHandle, ModToUpload.Thumbnail.FullName);
			}
			SteamUGC.SetItemContent(updateHandle, ModToUpload.DirectoryInfo.FullName);
			SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(updateHandle, SteamFriends.GetPersonaName() + " : " + (changeNote ?? $"Update of this mod ({ModToUpload.WorkshopId}) ! Mod Title : {ModToUpload.Title}."));
			onSubmitItemUpdateResultCallResult.Set(hAPICall);
		}
	}

	private static void Init()
	{
		onCreateItemCallbackResult = CallResult<CreateItemResult_t>.Create(OnCreateItemResult);
		onDeleteItemCallbackResult = CallResult<DeleteItemResult_t>.Create(OnDeleteItemResult);
		onSubmitItemCreationUpdateResultCallResult = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitItemCreationUpdateResult);
		onSubmitItemUpdateResultCallResult = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitItemUpdateResult);
	}

	private static async void CreateSteamWorkshopFile(PublishedFileId_t m_nPublishedFileId, Action onCompleted)
	{
		FileStream file = File.Open(ModToUpload.DirectoryInfo?.ToString() + "/steam_workshop.txt", FileMode.OpenOrCreate, FileAccess.Write);
		StreamWriter writer = new StreamWriter(file);
		await writer.WriteLineAsync(m_nPublishedFileId.m_PublishedFileId.ToString().Replace("\0", string.Empty));
		writer.Close();
		file.Close();
		onCompleted?.Invoke();
	}

	private static void OnCreateItemResult(CreateItemResult_t createItemResult, bool bIOFailure)
	{
		string changeNoteValue = TPSingleton<ModsView>.Instance.GetChangeNoteValue();
		updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), createItemResult.m_nPublishedFileId);
		SteamUGC.SetItemTitle(updateHandle, ModToUpload.Title);
		SteamUGC.SetItemDescription(updateHandle, ModToUpload.Description);
		SteamUGC.SetItemContent(updateHandle, ModToUpload.DirectoryInfo.FullName);
		if (ModToUpload.Thumbnail != null)
		{
			SteamUGC.SetItemPreview(updateHandle, ModToUpload.Thumbnail.FullName);
		}
		SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(updateHandle, SteamFriends.GetPersonaName() + " : " + (changeNoteValue ?? $"Creation of this mod ({createItemResult.m_nPublishedFileId.m_PublishedFileId}) ! Mod Title : {ModToUpload.Title}."));
		onSubmitItemCreationUpdateResultCallResult.Set(hAPICall);
	}

	private static void OnDeleteItemResult(DeleteItemResult_t deleteItemResult, bool bIOFailure)
	{
		if (deleteItemResult.m_eResult == EResult.k_EResultOK)
		{
			TPSingleton<ModManager>.Instance.Log($"This item({deleteItemResult.m_nPublishedFileId}) is deleted !", CLogLevel.MAJOR);
		}
		else
		{
			TPSingleton<ModManager>.Instance.LogWarning($"Failed to delete this item({deleteItemResult.m_nPublishedFileId}) !", CLogLevel.MAJOR);
		}
	}

	private static void OnSubmitItemCreationUpdateResult(SubmitItemUpdateResult_t submitItemUpdateResult, bool bIOFailure)
	{
		if (submitItemUpdateResult.m_eResult == EResult.k_EResultOK)
		{
			TPSingleton<ModManager>.Instance.Log($"This item ({submitItemUpdateResult.m_nPublishedFileId}) is published !", CLogLevel.MAJOR);
			CreateSteamWorkshopFile(submitItemUpdateResult.m_nPublishedFileId, delegate
			{
				OnUploadSteamItem.Invoke(submitItemUpdateResult);
			});
		}
		else
		{
			TPSingleton<ModManager>.Instance.LogWarning($"Failed to publish this item ({submitItemUpdateResult.m_nPublishedFileId}) ! Reason : {submitItemUpdateResult.m_eResult}", CLogLevel.MAJOR);
			OnFailUploadSteamItem?.Invoke(submitItemUpdateResult);
			DeleteItem(submitItemUpdateResult.m_nPublishedFileId);
		}
	}

	private static void OnSubmitItemUpdateResult(SubmitItemUpdateResult_t submitItemUpdateResult, bool bIOFailure)
	{
		if (submitItemUpdateResult.m_eResult == EResult.k_EResultOK)
		{
			TPSingleton<ModManager>.Instance.Log($"This item ({submitItemUpdateResult.m_nPublishedFileId}) is updated !", CLogLevel.MAJOR);
			OnUploadSteamItem.Invoke(submitItemUpdateResult);
		}
		else
		{
			TPSingleton<ModManager>.Instance.LogWarning($"Failed to update this item ({submitItemUpdateResult.m_nPublishedFileId}) ! Reason : {submitItemUpdateResult.m_eResult}", CLogLevel.MAJOR);
			OnFailUploadSteamItem?.Invoke(submitItemUpdateResult);
		}
	}
}
