using TPLib;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerFromSettings : MonoBehaviour
{
	private CanvasScaler canvasScaler;

	private void UpdateScale(float scale)
	{
		canvasScaler.scaleFactor = scale;
	}

	private void Awake()
	{
		canvasScaler = GetComponent<CanvasScaler>();
		TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent.AddListener(UpdateScale);
		UpdateScale(TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale);
	}

	private void OnDestroy()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent.RemoveListener(UpdateScale);
		}
	}
}
