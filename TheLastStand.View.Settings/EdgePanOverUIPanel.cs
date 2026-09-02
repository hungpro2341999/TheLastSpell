using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class EdgePanOverUIPanel : MonoBehaviour
{
	[SerializeField]
	private BetterToggle edgePanOverUIToggle;

	public void OnValueChanged()
	{
		SettingsController.SetEdgePanOverUI(edgePanOverUIToggle.isOn);
	}

	public void Refresh()
	{
		edgePanOverUIToggle.isOn = TPSingleton<SettingsManager>.Instance.Settings.EdgePanOverUI;
	}
}
