using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class AllowDataCollectionPanel : MonoBehaviour
{
	[SerializeField]
	private BetterToggle allowDataCollectionToggle;

	public void OnValueChanged()
	{
		SettingsController.SetAllowDataCollection(allowDataCollectionToggle.isOn);
	}

	public void Refresh()
	{
		allowDataCollectionToggle.isOn = TPSingleton<SettingsManager>.Instance.Settings.AllowDataCollection;
	}
}
