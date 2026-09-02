using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class HideCompendium : MonoBehaviour
{
	[SerializeField]
	private BetterToggle hideCompendiumToggle;

	public void OnValueChanged()
	{
		SettingsController.SetHideCompendium(hideCompendiumToggle.isOn);
	}

	public void Refresh()
	{
		hideCompendiumToggle.isOn = TPSingleton<SettingsManager>.Instance.Settings.HideCompendium;
	}
}
