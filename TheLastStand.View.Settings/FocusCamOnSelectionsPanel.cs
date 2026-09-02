using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class FocusCamOnSelectionsPanel : MonoBehaviour
{
	[SerializeField]
	private BetterToggle focusCamOnSelectionsToggle;

	public void OnValueChanged()
	{
		SettingsController.SetFocusCamOnSelections(!focusCamOnSelectionsToggle.isOn);
	}

	public void Refresh()
	{
		focusCamOnSelectionsToggle.isOn = !TPSingleton<SettingsManager>.Instance.Settings.FocusCamOnSelections;
	}
}
