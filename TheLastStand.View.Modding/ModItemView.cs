using System.Collections.Generic;
using Steamworks;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Framework.UI;
using TheLastStand.Manager.Modding;
using TheLastStand.Manager.SteamWorkshop;
using TheLastStand.Model.Modding;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Modding;

public class ModItemView : MonoBehaviour
{
	public static class Constants
	{
		public const string ModNotUsed = "Modding_ModNotUsed";

		public const string UpdateInSteam = "Modding_UpdateInSteam";

		public const string UploadInSteam = "Modding_UploadInSteam";

		public const string ModLocationSteam = "Modding_ModLocation_Steam";

		public const string ModLocationLocal = "Modding_ModLocation_Local";

		public const string ItemNoTitle = "Modding_ItemNoTitle";

		public const string ItemAuthor = "Modding_ItemAuthor";

		public const string ItemNoAuthor = "Modding_ItemNoAuthor";

		public const string SteamWorkshopId = "Modding_SteamWorkshopId";

		public const string JumpLine = "\r\n";

		public const string ModdingAbortUploadTitleKey = "Modding_AbortUploadTitle";

		public const string ModdingUploadErrorTitleKey = "Modding_UploadErrorTitle";

		public const string ModdingUnsupportedErrorLabelKey = "Modding_UnsupportedErrorLabel";

		public const string ModdingAbortUploadPrefixKey = "Modding_AbortUpload_";

		public const string ModdingUpdatingItemKey = "Modding_UpdatingItem";

		public const string ModdingUploadingItemKey = "Modding_UploadingItem";
	}

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private BetterButton uploadButton;

	[SerializeField]
	private TextMeshProUGUI uploadButtonLabel;

	[SerializeField]
	private Image checkImage;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private TextMeshProUGUI uploadingLabel;

	[SerializeField]
	private RawTextTooltipDisplayer rawTextTooltipDisplayer;

	[SerializeField]
	private GenericTooltipDisplayer genericTooltipDisplayer;

	[SerializeField]
	private DataColor activeDataColor;

	[SerializeField]
	private DataColor inactiveDataColor;

	[SerializeField]
	private List<FontAssemblyLabel> modifiableFonts = new List<FontAssemblyLabel>();

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	private Mod mod;

	public List<FontAssemblyLabel> ModifiableFonts => modifiableFonts;

	public void Refresh()
	{
		Refresh(mod);
	}

	public void Refresh(Mod mod)
	{
		this.mod = mod;
		StyleOverrideDatas.Style fontTags = mod.ModController.GetFontTags();
		rawTextTooltipDisplayer.TargetTooltip = TPSingleton<ModsView>.Instance.RawTextTooltip;
		RefreshLabel(fontTags);
		checkImage.enabled = mod.IsUsed && !mod.IsIncompatible;
		RefreshSteamUploadButton();
		rawTextTooltipDisplayer.Text = string.Empty;
		if (!mod.IsUsed)
		{
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 40f);
			if (mod.Location == Mod.E_ModLocation.STEAM)
			{
				rawTextTooltipDisplayer.Text = Localizer.Get("Modding_ModNotUsed") + "\r\n\r\n";
			}
		}
		RawTextTooltipDisplayer obj = rawTextTooltipDisplayer;
		obj.Text = obj.Text + fontTags.OpenTag + mod.Description + fontTags.CloseTag;
		AddSteamWorkshopIdInTooltip();
		RawTextTooltipDisplayer obj2 = rawTextTooltipDisplayer;
		obj2.Text = obj2.Text + "\r\n" + ((mod.Version < ModManager.ModMinVersion) ? Localizer.Format("Modding_ModBadVersion", mod.Version) : Localizer.Format("Modding_ModVersion", mod.Version));
		RefreshModifiableFonts();
	}

	private void RefreshLabel(StyleOverrideDatas.Style style)
	{
		text.text = Localizer.Get((mod.Location == Mod.E_ModLocation.STEAM) ? "Modding_ModLocation_Steam" : "Modding_ModLocation_Local") + " ";
		TextMeshProUGUI textMeshProUGUI = text;
		textMeshProUGUI.text = textMeshProUGUI.text + ((mod.Title != string.Empty) ? (style.OpenTag + mod.Title + style.OpenTag) : Localizer.Get("Modding_ItemNoTitle")) + " ";
		text.text += ((mod.Author != string.Empty) ? Localizer.Format("Modding_ItemAuthor", style.OpenTag + mod.Author + style.OpenTag) : Localizer.Get("Modding_ItemNoAuthor"));
		if (mod.IsIncompatible)
		{
			TextMeshProUGUI textMeshProUGUI2 = text;
			textMeshProUGUI2.text = textMeshProUGUI2.text + " " + Localizer.Get("Modding_Incompatible_Label");
		}
		text.color = (mod.IsUsed ? activeDataColor._Color : inactiveDataColor._Color);
	}

	private void OnItemUploaded(SubmitItemUpdateResult_t param)
	{
		WorkshopItemHandler.OnUploadSteamItem.RemoveListener(OnItemUploaded);
		ModLoader<SteamModLoader>.Instance.OnPublishedItemsLoaded.AddListener(OnPublishedItemsLoaded);
		ModLoader<SteamModLoader>.Instance.LoadPublishedMods();
	}

	private void OnItemUploadFailed(SubmitItemUpdateResult_t param)
	{
		WorkshopItemHandler.OnFailUploadSteamItem.RemoveListener(OnItemUploadFailed);
		string textLocKey = "Modding_UnsupportedErrorLabel";
		GenericPopUp.Open("Modding_UploadErrorTitle", textLocKey);
		uploadingLabel.gameObject.SetActive(value: false);
		Refresh(mod);
		TPSingleton<ModsView>.Instance.UnlockRaycasts();
		TPSingleton<ModsView>.Instance.IsUploadingMod = false;
	}

	private void OnPublishedItemsLoaded()
	{
		ModLoader<SteamModLoader>.Instance.OnPublishedItemsLoaded.RemoveListener(OnPublishedItemsLoaded);
		uploadingLabel.gameObject.SetActive(value: false);
		Refresh(mod);
		TPSingleton<ModsView>.Instance.UnlockRaycasts();
		TPSingleton<ModsView>.Instance.IsUploadingMod = false;
	}

	private void RefreshSteamUploadButton()
	{
		if (mod.Location == Mod.E_ModLocation.LOCAL)
		{
			uploadButtonLabel.text = Localizer.Get(mod.HasAWorkshopId ? "Modding_UpdateInSteam" : "Modding_UploadInSteam");
			uploadButton.onClick.RemoveListener(OpenChangeNotePopup);
			TPSingleton<ModsView>.Instance.SubmitChangeNoteButton.onClick.RemoveListener(UploadItemToSteam);
			if (ModLoader<SteamModLoader>.Instance.CanUserUploadMod(mod))
			{
				if (ModManager.IsModder)
				{
					uploadButton.gameObject.SetActive(value: true);
					uploadButton.onClick.AddListener(OpenChangeNotePopup);
					TPSingleton<ModsView>.Instance.SubmitChangeNoteButton.onClick.AddListener(UploadItemToSteam);
				}
				else
				{
					uploadButton.gameObject.SetActive(value: false);
				}
			}
			else if (ModManager.IsModder)
			{
				if (!mod.HasAWorkshopId)
				{
					uploadButton.gameObject.SetActive(value: true);
					uploadButton.onClick.AddListener(OpenChangeNotePopup);
					TPSingleton<ModsView>.Instance.SubmitChangeNoteButton.onClick.AddListener(UploadItemToSteam);
				}
				else
				{
					uploadButton.interactable = false;
					genericTooltipDisplayer.SetTargetTooltip(TPSingleton<ModsView>.Instance.WarningTooltip);
					genericTooltipDisplayer.gameObject.SetActive(value: true);
				}
			}
			else
			{
				uploadButton.gameObject.SetActive(value: false);
			}
		}
		else
		{
			uploadButton.gameObject.SetActive(value: false);
		}
	}

	private void OpenChangeNotePopup()
	{
		TPSingleton<ModsView>.Instance.OpenChangeNotePopup();
	}

	private void UploadItemToSteam()
	{
		if (!mod.ModController.CanBeUploaded(out var uploadErrorReason))
		{
			GenericPopUp.Open("Modding_AbortUploadTitle", "Modding_AbortUpload_" + uploadErrorReason);
			return;
		}
		WorkshopItemHandler.OnUploadSteamItem.AddListener(OnItemUploaded);
		WorkshopItemHandler.OnFailUploadSteamItem.AddListener(OnItemUploadFailed);
		TPSingleton<ModsView>.Instance.LockRaycasts();
		TPSingleton<ModsView>.Instance.IsUploadingMod = true;
		if (mod.HasAWorkshopId)
		{
			WorkshopItemHandler.ModifyItem(mod, TPSingleton<ModsView>.Instance.GetChangeNoteValue());
			uploadingLabel.text = Localizer.Get("Modding_UpdatingItem");
		}
		else
		{
			WorkshopItemHandler.CreateItem(mod);
			uploadingLabel.text = Localizer.Get("Modding_UploadingItem");
		}
		uploadingLabel.gameObject.SetActive(value: true);
		uploadButton.gameObject.SetActive(value: false);
	}

	private void AddSteamWorkshopIdInTooltip()
	{
		if (ModManager.IsModder && mod.HasAWorkshopId)
		{
			rawTextTooltipDisplayer.Text += "\r\n\r\n";
			rawTextTooltipDisplayer.Text += Localizer.Format("Modding_SteamWorkshopId", mod.WorkshopId);
		}
	}

	public void RefreshModifiableFonts()
	{
		simpleFontLocalizedParent?.RefreshChildren();
	}
}
