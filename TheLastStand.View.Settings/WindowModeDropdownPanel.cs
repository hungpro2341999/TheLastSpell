using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Manager;

namespace TheLastStand.View.Settings;

public class WindowModeDropdownPanel : DropdownPanel
{
	private const int WindowModesCount = 3;

	private SettingsManager.E_WindowMode windowMode;

	public override void OnDropdownValueChange()
	{
		base.OnDropdownValueChange();
		windowMode = (SettingsManager.E_WindowMode)dropdown.value;
		SettingsController.SetWindowMode(windowMode);
	}

	protected override void InitializeOptionKeys()
	{
		windowMode = TPSingleton<SettingsManager>.Instance.Settings.WindowMode;
		dropdown.options = new List<TMP_Dropdown.OptionData>(3);
		for (int i = 0; i < 3; i++)
		{
			dropdown.options.Add(new TMP_Dropdown.OptionData($"WindowMode_{(SettingsManager.E_WindowMode)i}"));
		}
		base.InitializeOptionKeys();
		dropdown.value = (int)windowMode;
	}

	public override void Refresh()
	{
		windowMode = TPSingleton<SettingsManager>.Instance.Settings.WindowMode;
		dropdown.value = (int)windowMode;
		base.Refresh();
	}
}
