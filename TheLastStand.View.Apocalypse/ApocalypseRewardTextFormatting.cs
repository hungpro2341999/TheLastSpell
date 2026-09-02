using TPLib.Localization;

namespace TheLastStand.View.Apocalypse;

public class ApocalypseRewardTextFormatting
{
	public string TextKey { get; private set; }

	public object[] TextParams { get; private set; }

	public ApocalypseRewardTextFormatting(string textKey, params object[] textParams)
	{
		TextKey = textKey;
		TextParams = textParams;
	}

	public string GetLocalizedText()
	{
		return Localizer.Format(TextKey, TextParams);
	}
}
