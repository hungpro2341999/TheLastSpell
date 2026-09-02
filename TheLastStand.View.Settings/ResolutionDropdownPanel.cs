using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Settings;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class ResolutionDropdownPanel : DropdownPanel
{
	private Resolution resolution;

	public override void OnDropdownValueChange()
	{
		base.OnDropdownValueChange();
		for (int num = SettingsManager.SupportedResolutions.Length - 1; num >= 0; num--)
		{
			Resolution resolution = SettingsManager.SupportedResolutions[num];
			string text = $"{resolution.width}x{resolution.height} {resolution.refreshRate}Hz";
			if (base.OptionKeys[dropdown.value] == text)
			{
				this.resolution = resolution;
				SettingsController.SetResolution(this.resolution);
				break;
			}
		}
	}

	protected override void InitializeOptionKeys()
	{
		this.resolution = TPSingleton<SettingsManager>.Instance.Settings.Resolution;
		int num = SettingsManager.SupportedResolutions.Length;
		dropdown.options = new List<TMP_Dropdown.OptionData>(num);
		int num2 = -1;
		for (int i = 0; i < num; i++)
		{
			Resolution resolution = SettingsManager.SupportedResolutions[i];
			dropdown.options.Add(new TMP_Dropdown.OptionData($"{resolution.width}x{resolution.height} {resolution.refreshRate}Hz"));
			if (num2 == -1 && resolution.width == this.resolution.width && resolution.height == this.resolution.height && resolution.refreshRate <= this.resolution.refreshRate)
			{
				num2 = i;
			}
		}
		if (num2 == -1)
		{
			TPSingleton<SettingsManager>.Instance.LogWarning($"No valid resolution has been found in dropdown options for screen resolution {this.resolution.width}x{this.resolution.height} {this.resolution.refreshRate}Hz.", CLogLevel.DETAILED);
			num2 = 0;
		}
		base.InitializeOptionKeys();
		dropdown.value = num2;
	}

	public void DebugInitializeOptionKeys()
	{
		InitializeOptionKeys();
	}
}
