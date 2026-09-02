using System;
using TMPro;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.View.Settings;

public abstract class SettingsFieldPanel : MonoBehaviour
{
	protected static class Constants
	{
		public static class LocalizationKeys
		{
			public const string Language = "Settings_Language";

			public const string SmartCast = "Settings_SmartCast_Label";

			public const string SpeedMode = "Settings_SpeedMode_Label";

			public const string SpeedScale = "Settings_SpeedScale_Label";

			public const string RunInBackground = "Settings_RunInBackground";

			public const string RestrictedCursor = "Settings_RestrictedCursor";

			public const string ShowSkillsHotkeys = "Settings_ShowSkillsHotkeys";

			public const string EraseSave = "Settings_EraseSave";

			public const string InputDeviceType = "Settings_InputDeviceType";

			public const string ConfirmEraseSaveLocalizationKey = "Settings_ConfirmEraseSaveText";
		}
	}

	[SerializeField]
	protected TextMeshProUGUI labelText;

	public virtual void Refresh()
	{
		RefreshLocalizedTexts();
	}

	protected virtual void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	protected virtual void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	protected virtual void OnLocalize()
	{
		RefreshLocalizedTexts();
	}

	protected abstract void RefreshLocalizedTexts();
}
