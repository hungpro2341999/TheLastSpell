using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.View.Settings;

public class InputDeviceTypeDropdownPanel : DropdownPanel
{
	private const int InputDeviceTypesCount = 3;

	public override void OnDropdownValueChange()
	{
		base.OnDropdownValueChange();
		SettingsManager.SetInputDeviceType((SettingsManager.E_InputDeviceType)dropdown.value);
	}

	protected override void InitializeOptionKeys()
	{
		dropdown.options = new List<TMP_Dropdown.OptionData>(3);
		int valueWithoutNotify = 0;
		for (int i = 0; i < 3; i++)
		{
			string text = $"DeviceType_{(SettingsManager.E_InputDeviceType)i}";
			dropdown.options.Add(new TMP_Dropdown.OptionData(text));
			if (i == (int)TPSingleton<SettingsManager>.Instance.Settings.InputDeviceType)
			{
				valueWithoutNotify = i;
			}
		}
		base.InitializeOptionKeys();
		dropdown.SetValueWithoutNotify(valueWithoutNotify);
	}
}
