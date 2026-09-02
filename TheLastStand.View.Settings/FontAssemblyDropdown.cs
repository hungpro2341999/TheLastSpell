using System;
using System.Collections.Generic;
using TMPro;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Localization.ScriptableObjects;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class FontAssemblyDropdown : DropdownPanel
{
	[SerializeField]
	private FontAssemblyLabel fontAssemblyLabel;

	private int fontIndex;

	public override void OnDropdownValueChange()
	{
		base.OnDropdownValueChange();
		fontIndex = dropdown.value;
		if (FontSettings.IsActivated)
		{
			fontAssemblyLabel.SetTargettedFontAssemblyIndex(fontIndex);
			FontManager.OnAssemblyIndexHasChanged(fontIndex);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		InitializeOptionKeys();
	}

	protected override void InitializeOptionKeys()
	{
		if (FontSettings.IsActivated)
		{
			fontIndex = FontManager.FontAssemblyIndex;
			List<FontAssembly> fontAssemblies = FontManager.GetFontAssemblies(Localizer.language);
			int count = fontAssemblies.Count;
			dropdown.options = new List<TMP_Dropdown.OptionData>(count);
			for (int i = 0; i < count; i++)
			{
				FontAssembly fontAssembly = fontAssemblies[i];
				dropdown.options.Add(new TMP_Dropdown.OptionData(fontAssembly.Id));
			}
			fontAssemblyLabel.SetTargettedFontAssemblyIndex(fontIndex);
		}
		base.InitializeOptionKeys();
		dropdown.SetValueWithoutNotify(fontIndex);
	}

	private void DropdownOpened()
	{
		GameObject gameObject = dropdown.transform.GetChild(dropdown.transform.childCount - 1).gameObject;
		if (FontSettings.IsActivated)
		{
			FontAssemblyLabel[] componentsInChildren = gameObject.GetComponentsInChildren<FontAssemblyLabel>(includeInactive: false);
			int count = FontManager.CurrentFontAssemblies.Count;
			for (int i = 0; i < count; i++)
			{
				componentsInChildren[i].SetTargettedFontAssemblyIndex(i);
			}
		}
	}
}
