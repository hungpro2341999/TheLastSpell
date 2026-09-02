using System;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Settings;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class SpeedModePanel : SettingsFieldPanel
{
	[SerializeField]
	private TMP_Dropdown dropdown;

	public override void Refresh()
	{
		base.Refresh();
		dropdown.SetValueWithoutNotify((int)TPSingleton<SettingsManager>.Instance.Settings.SpeedMode);
	}

	protected override void RefreshLocalizedTexts()
	{
		labelText.text = Localizer.Get("Settings_SpeedMode_Label");
	}

	protected override void Awake()
	{
		base.Awake();
		Array values = Enum.GetValues(typeof(SettingsManager.E_SpeedMode));
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		foreach (int item in values)
		{
			list.Add(new TMP_Dropdown.OptionData($"Settings_SpeedMode_{(SettingsManager.E_SpeedMode)item}"));
		}
		dropdown.AddOptions(list);
		dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
	}

	private void OnDropdownValueChanged(int optionIndex)
	{
		SettingsController.SetSpeedMode((SettingsManager.E_SpeedMode)optionIndex);
	}
}
