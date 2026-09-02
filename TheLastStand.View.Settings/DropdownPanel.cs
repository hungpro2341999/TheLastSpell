using System;
using TMPro;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class DropdownPanel : MonoBehaviour
{
	[SerializeField]
	protected bool useLocalization = true;

	[SerializeField]
	protected TMP_Dropdown dropdown;

	protected int optionsCount;

	protected string[] OptionKeys { get; private set; }

	public virtual void OnDropdownValueChange()
	{
	}

	public virtual void Refresh()
	{
		RefreshOptionTexts();
	}

	protected virtual void Awake()
	{
		if (useLocalization)
		{
			Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		}
	}

	protected virtual void InitializeOptionKeys()
	{
		optionsCount = dropdown.options.Count;
		OptionKeys = new string[optionsCount];
		for (int i = 0; i < optionsCount; i++)
		{
			OptionKeys[i] = dropdown.options[i].text;
		}
	}

	protected virtual void OnDestroy()
	{
		if (useLocalization)
		{
			Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		}
	}

	protected virtual void RefreshOptionTexts()
	{
		if (optionsCount != 0)
		{
			for (int i = 0; i < optionsCount; i++)
			{
				dropdown.options[i].text = (useLocalization ? Localizer.Get(OptionKeys[i]) : OptionKeys[i]);
			}
			dropdown.RefreshShownValue();
		}
	}

	protected virtual void Start()
	{
		InitializeOptionKeys();
		Refresh();
	}

	private void OnLocalize()
	{
		RefreshOptionTexts();
	}
}
