using TheLastStand.Controller.Settings;

namespace TheLastStand.View.Settings;

public class ScreenShakeSlider : FloatSlider
{
	public override void OnValueSliderChange()
	{
		base.OnValueSliderChange();
		SettingsController.SetScreenShakes(base.Value);
	}
}
