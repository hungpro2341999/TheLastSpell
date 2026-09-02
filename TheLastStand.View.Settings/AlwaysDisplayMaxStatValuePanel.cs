using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class AlwaysDisplayMaxStatValuePanel : MonoBehaviour
{
	[SerializeField]
	private BetterToggle alwaysDisplayMaxStatValueToggle;

	public void OnValueChanged()
	{
		SettingsController.SetAlwaysDisplayMaxStatValue(alwaysDisplayMaxStatValueToggle.isOn);
	}

	public void Refresh()
	{
		alwaysDisplayMaxStatValueToggle.isOn = TPSingleton<SettingsManager>.Instance.Settings.AlwaysDisplayMaxStatValue;
	}
}
