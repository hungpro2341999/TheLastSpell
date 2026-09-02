using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class EdgePanPanel : MonoBehaviour
{
	[SerializeField]
	private BetterToggle edgePanToggle;

	public void OnValueChanged()
	{
		SettingsController.SetEdgePan(edgePanToggle.isOn);
	}

	public void Refresh()
	{
		edgePanToggle.isOn = TPSingleton<SettingsManager>.Instance.Settings.EdgePan;
	}
}
