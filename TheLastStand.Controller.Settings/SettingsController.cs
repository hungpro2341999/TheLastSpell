using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TheLastStand.Manager;
using TheLastStand.Model.Settings;
using UnityEngine;

namespace TheLastStand.Controller.Settings;

public class SettingsController
{
	public TheLastStand.Model.Settings.Settings Settings { get; }

	public SettingsController()
	{
		Settings = new TheLastStand.Model.Settings.Settings(this);
	}

	public static void SetAllowDataCollection(bool value)
	{
		TPSingleton<SettingsManager>.Instance.Settings.AllowDataCollection = value;
	}

	public static void SetAlwaysDisplayMaxStatValue(bool value)
	{
		TPSingleton<SettingsManager>.Instance.Settings.AlwaysDisplayMaxStatValue = value;
	}

	public static void SetAlwaysDisplayUnitPortraitAttribute(bool value)
	{
		TPSingleton<SettingsManager>.Instance.Settings.AlwaysDisplayUnitPortraitAttribute = value;
	}

	public static void SetAmbientVolume(float volume)
	{
		TPSingleton<SettingsManager>.Instance.Settings.AmbientVolume = Mathf.Clamp01(volume);
		TPSingleton<SettingsManager>.Instance.AmbientVolumeSettingChangeEvent.Invoke(TPSingleton<SettingsManager>.Instance.Settings.AmbientVolume);
	}

	public static void SetEdgePan(bool value)
	{
		TPSingleton<SettingsManager>.Instance.Settings.EdgePan = value;
	}

	public static void SetEdgePanOverUI(bool value)
	{
		TPSingleton<SettingsManager>.Instance.Settings.EdgePanOverUI = value;
	}

	public static void SetFocusCamOnSelections(bool value)
	{
		TPSingleton<SettingsManager>.Instance.Settings.FocusCamOnSelections = value;
	}

	public static void SetHideCompendium(bool value)
	{
		TPSingleton<SettingsManager>.Instance.Settings.HideCompendium = value;
	}

	public static void SetSmartCast(bool value)
	{
		TPSingleton<SettingsManager>.Instance.Settings.SmartCast = value;
	}

	public static void SetTargetFrameRate(int frameRate)
	{
		TPSingleton<SettingsManager>.Instance.Settings.FrameRateCap = frameRate;
		Application.targetFrameRate = frameRate;
	}

	public static void SetKeyboardLayout(SettingsManager.E_KeyboardLayout keyboardLayout)
	{
		TPSingleton<SettingsManager>.Instance.Settings.KeyboardLayout = keyboardLayout;
	}

	public static void SetMasterVolume(float volume)
	{
		TPSingleton<SettingsManager>.Instance.Settings.MasterVolume = Mathf.Clamp01(volume);
		TPSingleton<SettingsManager>.Instance.MasterVolumeSettingChangeEvent.Invoke(TPSingleton<SettingsManager>.Instance.Settings.MasterVolume);
	}

	public static void SetMonitorIndex(int index)
	{
		TPSingleton<SettingsManager>.Instance.Settings.MonitorIndex = index;
		PlayerPrefs.SetInt("UnitySelectMonitor", TPSingleton<SettingsManager>.Instance.Settings.MonitorIndex);
	}

	public static void SetMusicVolume(float volume)
	{
		TPSingleton<SettingsManager>.Instance.Settings.MusicVolume = Mathf.Clamp01(volume);
		TPSingleton<SettingsManager>.Instance.MusicVolumeSettingChangeEvent.Invoke(TPSingleton<SettingsManager>.Instance.Settings.MusicVolume);
	}

	public static void SetResolution(Resolution resolution)
	{
		TPSingleton<SettingsManager>.Instance.Settings.Resolution = resolution;
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRate);
		Cursor.lockState = CursorLockMode.None;
		TPSingleton<SettingsManager>.Instance.StartCoroutine(TPSingleton<SettingsManager>.Instance.WaitForResolutionChange());
	}

	public static void SetRestrictedCursor(bool isCursorRestricted)
	{
		TPSingleton<SettingsManager>.Instance.Settings.IsCursorRestricted = isCursorRestricted;
		Cursor.lockState = (isCursorRestricted ? CursorLockMode.Confined : CursorLockMode.None);
	}

	public static void SetRunInBackground(bool runInBackground)
	{
		TPSingleton<SettingsManager>.Instance.Settings.RunInBackground = runInBackground;
		Application.runInBackground = runInBackground;
	}

	public static void SetShowSkillsHotkeys(bool showSkillsHotkeys)
	{
		TPSingleton<SettingsManager>.Instance.Settings.ShowSkillsHotkeys = showSkillsHotkeys;
	}

	public static void SetUISizeScale(float scale)
	{
		TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale = scale;
		TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent?.Invoke(scale);
	}

	public static void SetUIVolume(float volume)
	{
		TPSingleton<SettingsManager>.Instance.Settings.UiVolume = Mathf.Clamp01(volume);
		TPSingleton<SettingsManager>.Instance.UiVolumeSettingChangeEvent.Invoke(TPSingleton<SettingsManager>.Instance.Settings.UiVolume);
	}

	public static void SetVSync(bool useVSync)
	{
		TPSingleton<SettingsManager>.Instance.Settings.UseVSync = useVSync;
		QualitySettings.vSyncCount = (TPSingleton<SettingsManager>.Instance.Settings.UseVSync ? 1 : 0);
	}

	public static void SetWindowMode(SettingsManager.E_WindowMode windowMode)
	{
		TPSingleton<SettingsManager>.Instance.Settings.WindowMode = windowMode;
		switch (TPSingleton<SettingsManager>.Instance.Settings.WindowMode)
		{
		case SettingsManager.E_WindowMode.Windowed:
			Screen.fullScreenMode = FullScreenMode.Windowed;
			break;
		case SettingsManager.E_WindowMode.FullScreen:
			Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
			break;
		case SettingsManager.E_WindowMode.BorderlessWindow:
			Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
			break;
		}
		Cursor.lockState = CursorLockMode.None;
		TPSingleton<SettingsManager>.Instance.StartCoroutine(TPSingleton<SettingsManager>.Instance.WaitForFullScreenModeChange());
	}

	public static void ToggleGameSpeed(bool isOn)
	{
		if (!TPSingleton<TimeManager>.Instance.IsModifyingTimeScale || !DebugManager.DebugMode)
		{
			TPSingleton<SettingsManager>.Instance.IsTimeScaleAccelerated = isOn;
			Time.timeScale = (isOn ? TPSingleton<SettingsManager>.Instance.Settings.SpeedScale : 1f);
		}
	}

	public static void ToggleTurnEndWarning(SettingsManager.E_EndTurnWarning warningType, bool state)
	{
		if (TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings.Length > (int)warningType)
		{
			TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[(int)warningType] = state;
		}
	}

	public static void SetScreenShakes(float value)
	{
		TPSingleton<SettingsManager>.Instance.Settings.ScreenShakesValue = Mathf.Clamp01(value);
	}

	public static void SetSpeedMode(SettingsManager.E_SpeedMode speedMode)
	{
		TPSingleton<SettingsManager>.Instance.Settings.SpeedMode = speedMode;
	}

	public static void SetSpeedScale(float speedScale)
	{
		TPSingleton<SettingsManager>.Instance.Settings.SpeedScale = Mathf.Clamp(speedScale, SettingsManager.GameAccelerationMinValue, SettingsManager.GameAccelerationMaxValue);
	}

	[DevConsoleCommand("ToggleSmartCast")]
	public static void DebugToggleSmartCast()
	{
		SetSmartCast(!TPSingleton<SettingsManager>.Instance.Settings.SmartCast);
	}
}
