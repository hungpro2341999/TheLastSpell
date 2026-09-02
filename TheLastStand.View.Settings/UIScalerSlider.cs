using TMPro;
using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class UIScalerSlider : FloatSlider
{
	[SerializeField]
	private DataColor disclaimerActiveColor;

	[SerializeField]
	private DataColor disclaimerInactiveColor;

	[SerializeField]
	private TextMeshProUGUI disclaimerText;

	[SerializeField]
	private float minimumUISizeClamp = 0.7f;

	[SerializeField]
	private float maximumUISizeClamp = 3f;

	[SerializeField]
	private float maximumSliderValuesGap = 1.5f;

	public void OnEndDrag()
	{
		SettingsController.SetUISizeScale(base.Value / multiplier);
	}

	public override void OnValueSliderChange()
	{
		base.OnValueSliderChange();
		disclaimerText.color = ((base.Value / multiplier % 1f == 0f) ? disclaimerInactiveColor._Color : disclaimerActiveColor._Color);
		if (InputManager.IsLastControllerJoystick)
		{
			SettingsController.SetUISizeScale(base.Value / multiplier);
		}
	}

	public override void RefreshValue(float value, bool applyMultiplier = false)
	{
		base.RefreshValue(value, applyMultiplier);
		disclaimerText.color = ((base.Value / multiplier % 1f == 0f) ? disclaimerInactiveColor._Color : disclaimerActiveColor._Color);
	}

	public void AutoSetValueRange(Resolution resolution, Vector2 sizeReference)
	{
		float num = Mathf.Floor((float)resolution.width / sizeReference.x * multiplier) / multiplier;
		float num2 = Mathf.Floor((float)resolution.height / sizeReference.y * multiplier) / multiplier;
		float num3 = Mathf.Clamp((num2 > num) ? num : num2, 1.2f, maximumUISizeClamp);
		float num4 = Mathf.Clamp(num3 - maximumSliderValuesGap, minimumUISizeClamp, maximumUISizeClamp);
		slider.maxValue = num3 * multiplier;
		slider.minValue = num4 * multiplier;
		if (TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale <= 0f)
		{
			float num5 = Mathf.Ceil(num4);
			RefreshValue(num5, applyMultiplier: true);
			SettingsController.SetUISizeScale(num5);
		}
		else if (TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale < num4 || TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale > num3)
		{
			float num6 = Mathf.Clamp(TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale, num4, num3);
			RefreshValue(num6, applyMultiplier: true);
			SettingsController.SetUISizeScale(num6);
		}
	}
}
