using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class SettingsBottomPanel : MonoBehaviour
{
	public static class Consts
	{
		public const string QuitGame = "QuitGame";

		public const string ResumeGame = "ResumeGame";

		public const string ResumeMainMenu = "ResumeMainMenu";

		public const string Abandon = "Abandon";

		public const string SkipTutorial = "SkipTutorial";
	}

	[SerializeField]
	private TextMeshProUGUI abandonButtonText;

	[SerializeField]
	private TextMeshProUGUI quitGameButtonText;

	[SerializeField]
	private TextMeshProUGUI resumeButtonText;

	public void Refresh()
	{
		RefreshLocalizedTexts();
	}

	private void RefreshLocalizedTexts()
	{
		quitGameButtonText.text = Localizer.Get("QuitGame");
		resumeButtonText.text = Localizer.Get(TPSingleton<GameManager>.Exist() ? "ResumeGame" : "ResumeMainMenu");
		abandonButtonText.text = Localizer.Get((TPSingleton<WorldMapCityManager>.Instance.SelectedCity != null && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap) ? "SkipTutorial" : "Abandon");
	}

	private void Awake()
	{
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
}
