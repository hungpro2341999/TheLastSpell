using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class SpeedScalePanel : SettingsFieldPanel
{
	[SerializeField]
	protected BetterSlider slider;

	[SerializeField]
	private TextMeshProUGUI progressText;

	protected override void RefreshLocalizedTexts()
	{
		labelText.text = Localizer.Get("Settings_SpeedScale_Label");
	}

	public override void Refresh()
	{
		base.Refresh();
		slider.SetValueWithoutNotify(TPSingleton<SettingsManager>.Instance.Settings.SpeedScale);
		RefreshProgressText();
	}

	public void RefreshProgressText()
	{
		progressText.text = $"x{slider.value:#.#}";
	}

	protected override void Awake()
	{
		base.Awake();
		slider.minValue = SettingsManager.GameAccelerationMinValue;
		slider.maxValue = SettingsManager.GameAccelerationMaxValue;
		slider.onValueChanged.AddListener(OnSliderValueChanged);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		slider.onValueChanged.RemoveListener(OnSliderValueChanged);
	}

	private void OnSliderValueChanged(float newSpeedScale)
	{
		SettingsController.SetSpeedScale(newSpeedScale);
	}
}
