using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class RunInBackgroundPanel : SettingsFieldPanel
{
	[SerializeField]
	private BetterToggle runInBackgroundToggle;

	public void OnValueChanged()
	{
		SettingsController.SetRunInBackground(runInBackgroundToggle.isOn);
	}

	public override void Refresh()
	{
		base.Refresh();
		runInBackgroundToggle.isOn = TPSingleton<SettingsManager>.Instance.Settings.RunInBackground;
	}

	protected override void RefreshLocalizedTexts()
	{
		labelText.text = Localizer.Get("Settings_RunInBackground");
	}

	protected override void Awake()
	{
		base.Awake();
		Refresh();
	}
}
