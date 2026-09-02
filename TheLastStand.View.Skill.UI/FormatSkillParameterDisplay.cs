using TPLib.Localization;

namespace TheLastStand.View.Skill.UI;

public class FormatSkillParameterDisplay : SkillParameterDisplay
{
	private object[] effectNameParameters;

	public void Refresh(string effectName, string effectValue, string overrideSign = "", params object[] parameters)
	{
		effectNameParameters = parameters;
		base.Refresh(effectName, effectValue, overrideSign);
	}

	protected override void RefreshName()
	{
		effectNameText.text = Localizer.Format(nameLocalizationKey, effectNameParameters);
	}
}
