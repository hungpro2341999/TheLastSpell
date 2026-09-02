using System;
using System.Collections.Generic;
using TMPro;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class TurnEndWarningsOptionPanel : MonoBehaviour
{
	public static class Constants
	{
		public const string ResourcesThresholdWarningText = "Settings_Warning_GoldAndMaterialsThreshold";
	}

	[SerializeField]
	private List<EndTurnWarningToggle> endTurnWarningsToggles = new List<EndTurnWarningToggle>();

	[SerializeField]
	private TextMeshProUGUI resourcesThresholdWarningText;

	public void Refresh()
	{
		foreach (EndTurnWarningToggle endTurnWarningsToggle in endTurnWarningsToggles)
		{
			endTurnWarningsToggle.Refresh();
		}
	}

	private void Awake()
	{
		Refresh();
		RefreshLocalizedTexts();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		RefreshLocalizedTexts();
	}

	private void RefreshLocalizedTexts()
	{
		resourcesThresholdWarningText.text = Localizer.Format("Settings_Warning_GoldAndMaterialsThreshold", 50);
	}
}
