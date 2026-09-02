using System;
using TPLib.Localization;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class MultipleLocalizationKeyTooltipDisplayer : GenericTooltipDisplayer
{
	[Serializable]
	public class KeyWithCondition
	{
		[SerializeField]
		private string key;

		[SerializeField]
		private bool showOnlyIfSelectableIsUninteractable;

		public string Key => key;

		public bool ShowOnlyIfSelectableIsUninteractable => showOnlyIfSelectableIsUninteractable;
	}

	[SerializeField]
	private KeyWithCondition[] otherKeys;

	[SerializeField]
	private Selectable selectable;

	public override bool CanDisplayTooltip()
	{
		return true;
	}

	protected override void SetTooltipContent(GenericTooltip tooltip)
	{
		string text = Localizer.Get(base.LocaKey);
		for (int i = 0; i < otherKeys.Length; i++)
		{
			if (otherKeys[i].ShowOnlyIfSelectableIsUninteractable && !selectable.interactable)
			{
				text = text + "\r\n " + Localizer.Get(otherKeys[i].Key);
			}
			else if (!otherKeys[i].ShowOnlyIfSelectableIsUninteractable)
			{
				text = text + "\r\n " + Localizer.Get(otherKeys[i].Key);
			}
		}
		tooltip.SetLocalizedContent(text, null);
	}

	protected override void DisplayTooltip(bool display)
	{
		GenericTooltip genericTooltip = UIManager.GenericTooltip ?? targetTooltip;
		if (display)
		{
			SetTooltipContent(genericTooltip);
			genericTooltip.Display();
		}
		else
		{
			genericTooltip.Hide();
		}
	}
}
