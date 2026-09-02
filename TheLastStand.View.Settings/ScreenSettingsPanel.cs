using System;
using TMPro;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class ScreenSettingsPanel : MonoBehaviour
{
	public static class Consts
	{
		public static class LocalizationKeys
		{
			public const string NotImplementedYet = "Settings_NotImplementedYet";

			public const string Resolution = "Settings_Resolution";

			public const string DisplayTitle = "Settings_DisplayTitle";

			public const string DisplayOption = "Settings_DisplayOption";
		}
	}

	[SerializeField]
	private TextMeshProUGUI resolutionLabelText;

	[SerializeField]
	private TextMeshProUGUI displayLabelText;

	[SerializeField]
	private ResolutionDropdownPanel resolutionDropdownPanel;

	[SerializeField]
	private WindowModeDropdownPanel windowModeDropdownPanel;

	[SerializeField]
	private MonitorIndexDropdownPanel monitorIndexDropdownPanel;

	[SerializeField]
	private VSyncPanel vSyncPanel;

	[SerializeField]
	private FrameRateCapPanel frameRateCapPanel;

	[SerializeField]
	private TextMeshProUGUI[] notImplementedLabelTexts;

	public FrameRateCapPanel FrameRateCapPanel => frameRateCapPanel;

	public void Refresh()
	{
		RefreshLocalizedTexts();
		resolutionDropdownPanel.Refresh();
		windowModeDropdownPanel.Refresh();
		monitorIndexDropdownPanel.Refresh();
		vSyncPanel.Refresh();
		frameRateCapPanel.Refresh();
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
		resolutionLabelText.text = Localizer.Get("Settings_Resolution");
		if (displayLabelText != null)
		{
			displayLabelText.text = Localizer.Get("Settings_DisplayTitle");
		}
		if (notImplementedLabelTexts != null)
		{
			for (int num = notImplementedLabelTexts.Length - 1; num >= 0; num--)
			{
				notImplementedLabelTexts[num].text = Localizer.Get("Settings_NotImplementedYet");
			}
		}
		FrameRateCapPanel.RefreshLocalizedTexts();
	}
}
