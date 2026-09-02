using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class ScreenShakesOptionPanel : MonoBehaviour
{
	[SerializeField]
	private FloatSlider screenShakesSlider;

	public void Refresh()
	{
		screenShakesSlider.RefreshValue(TPSingleton<SettingsManager>.Instance.Settings.ScreenShakesValue);
	}

	private void Start()
	{
		Refresh();
	}
}
