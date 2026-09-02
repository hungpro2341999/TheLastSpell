using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class EndTurnWarningToggle : MonoBehaviour
{
	[SerializeField]
	private BetterToggle toggle;

	[SerializeField]
	private SettingsManager.E_EndTurnWarning warningType;

	public void OnValueChanged()
	{
		SettingsController.ToggleTurnEndWarning(warningType, toggle.isOn);
	}

	public void Refresh()
	{
		toggle.isOn = TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[(int)warningType];
	}
}
