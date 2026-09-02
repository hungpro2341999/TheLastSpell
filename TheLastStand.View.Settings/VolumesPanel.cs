using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class VolumesPanel : MonoBehaviour
{
	public static class Consts
	{
		public static class LocalizationKeys
		{
			public const string AmbientVolume = "Settings_AmbientVolume";

			public const string MasterVolume = "Settings_MasterVolume";

			public const string MusicVolume = "Settings_MusicVolume";

			public const string UIVolume = "Settings_UIVolume";
		}
	}

	[SerializeField]
	private TextMeshProUGUI masterVolumeLabelText;

	[SerializeField]
	private VolumeSlider masterVolumeSlider;

	[SerializeField]
	private TextMeshProUGUI musicVolumeLabelText;

	[SerializeField]
	private VolumeSlider musicVolumeSlider;

	[SerializeField]
	private TextMeshProUGUI uiVolumeLabelText;

	[SerializeField]
	private VolumeSlider uiVolumeSlider;

	[SerializeField]
	private TextMeshProUGUI ambientVolumeLabelText;

	[SerializeField]
	private VolumeSlider ambientVolumeSlider;

	public void Refresh()
	{
		masterVolumeSlider.RefreshVolume(TPSingleton<SettingsManager>.Instance.Settings.MasterVolume);
		musicVolumeSlider.RefreshVolume(TPSingleton<SettingsManager>.Instance.Settings.MusicVolume);
		uiVolumeSlider.RefreshVolume(TPSingleton<SettingsManager>.Instance.Settings.UiVolume);
		ambientVolumeSlider.RefreshVolume(TPSingleton<SettingsManager>.Instance.Settings.AmbientVolume);
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
		masterVolumeLabelText.text = Localizer.Get("Settings_MasterVolume");
		musicVolumeLabelText.text = Localizer.Get("Settings_MusicVolume");
		uiVolumeLabelText.text = Localizer.Get("Settings_UIVolume");
		ambientVolumeLabelText.text = Localizer.Get("Settings_AmbientVolume");
	}
}
