using TPLib;
using TPLib.Localization;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.View.Generic;
using TheLastStand.View.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class EraseSavePanel : SettingsFieldPanel
{
	[SerializeField]
	private BetterButton eraseSaveButton;

	[SerializeField]
	private GenericTooltipDisplayer genericTooltipDisplayer;

	public Selectable Selectable => eraseSaveButton;

	public override void Refresh()
	{
		base.Refresh();
		eraseSaveButton.Interactable = !ScenesManager.IsActiveSceneLevel();
		genericTooltipDisplayer.enabled = ScenesManager.IsActiveSceneLevel();
	}

	protected override void RefreshLocalizedTexts()
	{
		labelText.text = Localizer.Get("Settings_EraseSave");
	}

	protected override void Awake()
	{
		base.Awake();
		Refresh();
		eraseSaveButton.onClick.AddListener(OpenConsentPopup);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		eraseSaveButton.onClick.RemoveListener(OpenConsentPopup);
	}

	private void EraseSaves()
	{
		SaveManager.EraseSave(SaveManager.CurrentProfileIndex);
		SaveManager.LoadApp();
		ApocalypseManager.SetApocalypse(null);
		GlyphManager.ResetSelectedGlyphs();
		SaveManager.SafeLoadGameSaves();
		TPSingleton<MainMenuView>.Instance.Refresh();
		if (SettingsManager.CanCloseSettings())
		{
			SettingsManager.CloseSettings();
		}
		TPSingleton<SettingsManager>.Instance.Log("Saves has been erased!");
	}

	private void OnCancel()
	{
		OnConsentPopupClosed();
	}

	private void OnConfirm()
	{
		EraseSaves();
		OnConsentPopupClosed();
	}

	private void OnConsentPopupClosed()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<SettingsManager>.Instance.SettingsPanel.OnEraseSavePopupClosed();
		}
	}

	private void OpenConsentPopup()
	{
		GenericConsent.Open("Settings_ConfirmEraseSaveText", OnConfirm, OnCancel);
	}
}
