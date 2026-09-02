using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Generic;

public class GenericTooltip : TooltipBase
{
	[SerializeField]
	protected TextMeshProUGUI tooltipText;

	[SerializeField]
	private DataColor hotkeysColor;

	protected object[] argsToFormat;

	protected string textToDisplay = string.Empty;

	protected string hotkeyText = string.Empty;

	protected bool mustFormatHotkey;

	public void SetLocalizedContent(string text, string rewiredActionKey, bool shouldFormatHotKey = false)
	{
		SetContentWithHotkeys(text, rewiredActionKey, shouldFormatHotKey);
	}

	public void SetContent(string localizationKey, params object[] argsToFormat)
	{
		string empty = string.Empty;
		empty = ((argsToFormat == null) ? Localizer.Get(localizationKey) : string.Format(Localizer.Format(localizationKey, argsToFormat)));
		SetContentWithHotkeys(empty, null);
	}

	public void SetContentWithHotkeys(string text, string rewiredActionKey, bool mustFormatHotkey = false)
	{
		textToDisplay = text;
		hotkeyText = string.Empty;
		string[] localizedHotkeysForAction;
		if (!string.IsNullOrEmpty(rewiredActionKey) && (localizedHotkeysForAction = InputManager.GetLocalizedHotkeysForAction(rewiredActionKey)) != null)
		{
			string text2 = string.Empty;
			int i = 0;
			for (int num = localizedHotkeysForAction.Length; i < num; i++)
			{
				text2 = text2 + ((i > 0) ? ", " : string.Empty) + localizedHotkeysForAction[i];
			}
			text2 = "<b>[" + text2 + "]</b>";
			if (hotkeysColor != null)
			{
				text2 = "<color=#" + hotkeysColor._HexCode + ">" + text2 + "</color>";
			}
			hotkeyText = text2;
		}
		this.mustFormatHotkey = mustFormatHotkey;
	}

	protected override bool CanBeDisplayed()
	{
		return tooltipText.text != string.Empty;
	}

	protected override void RefreshContent()
	{
		tooltipText.text = (mustFormatHotkey ? string.Format(textToDisplay, hotkeyText) : (textToDisplay + " " + hotkeyText));
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshContent();
		}
	}
}
