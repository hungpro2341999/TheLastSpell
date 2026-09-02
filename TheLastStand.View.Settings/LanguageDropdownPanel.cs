using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class LanguageDropdownPanel : DropdownPanel
{
	[SerializeField]
	private LanguageLabel languageLabel;

	private string language;

	private Transform optionsContainer;

	private bool hasRefreshedAfterExpand;

	public override void OnDropdownValueChange()
	{
		base.OnDropdownValueChange();
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.Settings.LanguageAfterRestart = string.Empty;
		}
		for (int num = Localizer.knownLanguages.Length - 1; num >= 0; num--)
		{
			string text = Localizer.knownLanguages[num];
			if (base.OptionKeys[dropdown.value] == text)
			{
				Localizer.language = text;
				languageLabel.SetTargettedLanguage(text);
				break;
			}
		}
	}

	protected override void InitializeOptionKeys()
	{
		language = Localizer.language;
		languageLabel.SetTargettedLanguage(language);
		int num = Localizer.knownLanguages.Length;
		dropdown.options = new List<TMP_Dropdown.OptionData>(num);
		int valueWithoutNotify = 0;
		for (int i = 0; i < num; i++)
		{
			string text = Localizer.knownLanguages[i];
			dropdown.options.Add(new TMP_Dropdown.OptionData(text));
			if (text == language)
			{
				valueWithoutNotify = i;
			}
		}
		base.InitializeOptionKeys();
		dropdown.SetValueWithoutNotify(valueWithoutNotify);
	}

	private void Update()
	{
		if (!dropdown.IsExpanded)
		{
			hasRefreshedAfterExpand = false;
		}
		else if (!hasRefreshedAfterExpand)
		{
			OnDropdownOpened();
			hasRefreshedAfterExpand = true;
		}
	}

	private void OnDropdownOpened()
	{
		if (optionsContainer == null)
		{
			optionsContainer = dropdown.transform.GetChild(dropdown.transform.childCount - 1);
		}
		LanguageLabel[] componentsInChildren = optionsContainer.GetComponentsInChildren<LanguageLabel>(includeInactive: false);
		int i = 0;
		for (int num = Localizer.knownLanguages.Length; i < num; i++)
		{
			string targettedLanguage = Localizer.knownLanguages[i];
			componentsInChildren[i].SetTargettedLanguage(targettedLanguage);
		}
	}
}
