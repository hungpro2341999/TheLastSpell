using System.Collections.Generic;
using Rewired;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.UI;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Modding;
using TheLastStand.Model.Modding;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Modding;

public class ModsView : TPSingleton<ModsView>, IOverlayUser
{
	public static class Constants
	{
		public const string PopupLocalizedFontChildren = "Popup";

		public const string ModsListLocalizedFontChildren = "ModsList";
	}

	[SerializeField]
	private ComplexFontLocalizedParent complexFontLocalizedParent;

	[SerializeField]
	private Canvas windowCanvas;

	[SerializeField]
	private BetterButton openWindowButton;

	[SerializeField]
	private BetterButton closeWindowButton;

	[SerializeField]
	private ModItemView modItemViewPrefab;

	[SerializeField]
	private Transform modItemParent;

	[SerializeField]
	private RawTextTooltip rawTextTooltip;

	[SerializeField]
	private GenericTooltip warningTooltip;

	[SerializeField]
	private GameObject blocker;

	[SerializeField]
	private TextMeshProUGUI moddingVersionsLabel;

	[SerializeField]
	private Canvas changeNotePopupCanvas;

	[SerializeField]
	private TMP_InputField changeNoteInputField;

	[SerializeField]
	private BetterButton cancelChangeNoteButton;

	[SerializeField]
	private BetterButton submitChangeNoteButton;

	public bool IsUploadingMod;

	private readonly List<ModItemView> modItemViews = new List<ModItemView>();

	private bool populated;

	private bool previousDeveloperMode;

	public int OverlaySortingOrder => windowCanvas.sortingOrder - 1;

	public RawTextTooltip RawTextTooltip => rawTextTooltip;

	public GenericTooltip WarningTooltip => warningTooltip;

	public BetterButton SubmitChangeNoteButton => submitChangeNoteButton;

	public void LockRaycasts()
	{
		blocker.SetActive(value: true);
	}

	public void Open()
	{
		modItemViews.ForEach(delegate(ModItemView x)
		{
			x.Refresh();
		});
		Refresh();
		windowCanvas.enabled = true;
		CameraView.AttenuateWorldForPopupFocus(this);
	}

	public void OpenChangeNotePopup()
	{
		changeNotePopupCanvas.enabled = true;
		changeNoteInputField.text = string.Empty;
		complexFontLocalizedParent.TargetKey = "Popup";
		complexFontLocalizedParent.RefreshChildren();
	}

	public void UnlockRaycasts()
	{
		blocker.SetActive(value: false);
	}

	public string GetChangeNoteValue()
	{
		if (!(changeNoteInputField.text != string.Empty))
		{
			return null;
		}
		return changeNoteInputField.text;
	}

	private void Close()
	{
		ApplicationManager.Application.ApplicationController.BackToPreviousState();
		windowCanvas.enabled = false;
		CameraView.AttenuateWorldForPopupFocus(null);
	}

	private void CloseChangeNotePopup()
	{
		changeNotePopupCanvas.enabled = false;
	}

	private void OnDestroy()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		switch (controllerType)
		{
		case ControllerType.Joystick:
			openWindowButton.gameObject.SetActive(value: false);
			break;
		default:
			openWindowButton.gameObject.SetActive(ShouldEnableOpenButton());
			break;
		}
	}

	private void PopulateWindow(List<Mod> subscribedMods, List<Mod> outdatedMods)
	{
		if (!populated)
		{
			for (int i = 0; i < subscribedMods.Count; i++)
			{
				ModItemView modItemView = Object.Instantiate(modItemViewPrefab, modItemParent);
				modItemView.gameObject.name = "Mod Item " + subscribedMods[i].Title;
				modItemView.Refresh(subscribedMods[i]);
				modItemViews.Add(modItemView);
			}
			for (int j = 0; j < outdatedMods.Count; j++)
			{
				ModItemView modItemView2 = Object.Instantiate(modItemViewPrefab, modItemParent);
				modItemView2.gameObject.name = "Mod Item " + outdatedMods[j].Title;
				modItemView2.Refresh(outdatedMods[j]);
				modItemViews.Add(modItemView2);
			}
			populated = true;
		}
	}

	private void Refresh()
	{
		moddingVersionsLabel.text = Localizer.Format("Modding_Versions_Label", ModManager.ModVersion, ModManager.ModMinVersion);
		complexFontLocalizedParent.TargetKey = "ModsList";
		complexFontLocalizedParent.RefreshChildren();
	}

	private bool ShouldEnableOpenButton()
	{
		if (TPSingleton<ModManager>.Exist())
		{
			if (ModManager.SubscribedMods.Count <= 0)
			{
				return ModManager.OutdatedMods.Count > 0;
			}
			return true;
		}
		return false;
	}

	private void Start()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		if (ShouldEnableOpenButton())
		{
			PopulateWindow(ModManager.SubscribedMods, ModManager.OutdatedMods);
			openWindowButton.onClick.AddListener(OnOpenWindowButtonClicked);
			closeWindowButton.onClick.AddListener(OnCloseWindowButtonClicked);
			cancelChangeNoteButton.onClick.AddListener(CloseChangeNotePopup);
			submitChangeNoteButton.onClick.AddListener(CloseChangeNotePopup);
		}
		else
		{
			openWindowButton.gameObject.SetActive(value: false);
		}
	}

	private void OnCloseWindowButtonClicked()
	{
		Close();
	}

	private void OnOpenWindowButtonClicked()
	{
		ApplicationManager.Application.ApplicationController.SetState("ModList");
	}

	private void Update()
	{
		if (ApplicationManager.Application.State.GetName() != "ModList")
		{
			return;
		}
		if (TheLastStand.Manager.InputManager.GetButtonDown(23) && !IsUploadingMod && !GenericPopUp.IsOpen)
		{
			Close();
		}
		if (previousDeveloperMode != ModManager.IsModder)
		{
			previousDeveloperMode = ModManager.IsModder;
			modItemViews.ForEach(delegate(ModItemView x)
			{
				x.Refresh();
			});
		}
	}
}
