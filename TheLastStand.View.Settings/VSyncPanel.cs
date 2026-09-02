using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class VSyncPanel : MonoBehaviour
{
	public static class Consts
	{
		public static class LocalizationKeys
		{
			public const string VSync = "Settings_VSync";
		}
	}

	[SerializeField]
	private TextMeshProUGUI vSyncLabelText;

	[SerializeField]
	private BetterToggle vSyncToggle;

	public void OnValueChanged()
	{
		SettingsController.SetVSync(vSyncToggle.isOn);
		TPSingleton<SettingsManager>.Instance.SettingsPanel.RefreshFrameRateCap();
	}

	public void Refresh()
	{
		vSyncToggle.isOn = TPSingleton<SettingsManager>.Instance.Settings.UseVSync;
		RefreshLocalizedTexts();
	}

	private void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		RefreshLocalizedTexts();
	}

	private void RefreshLocalizedTexts()
	{
		vSyncLabelText.text = Localizer.Get("Settings_VSync");
	}
}
