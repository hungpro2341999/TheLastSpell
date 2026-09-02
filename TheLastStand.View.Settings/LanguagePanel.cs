using TPLib.Localization;

namespace TheLastStand.View.Settings;

public class LanguagePanel : SettingsFieldPanel
{
	protected override void RefreshLocalizedTexts()
	{
		labelText.text = Localizer.Get("Settings_Language");
	}
}
