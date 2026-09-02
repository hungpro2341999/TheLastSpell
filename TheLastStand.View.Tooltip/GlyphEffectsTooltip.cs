using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.Utilities;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Manager.Meta;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip;

public class GlyphEffectsTooltip : TooltipBase
{
	private static class Constants
	{
		public const string MaxGlyphsDisplayed = "GlyphEffects_MaxGlyphsDisplayed";
	}

	[SerializeField]
	private TextMeshProUGUI effectsDescription;

	[SerializeField]
	private Image customModeIcon;

	[SerializeField]
	private TextMeshProUGUI customModeValue;

	private List<GlyphDefinition> selectedGlyphs;

	private bool glyphsConfigCustomModeEnabled;

	private int customModeBonusPoints;

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(RefreshText));
	}

	public void SetSelectedGlyphs(List<GlyphDefinition> newGlyphs)
	{
		selectedGlyphs = newGlyphs;
	}

	public void SetCustomModeEnabled(bool newCustomModeEnabled)
	{
		glyphsConfigCustomModeEnabled = newCustomModeEnabled;
	}

	public void SetCustomModePoints(int newCustomModePoints)
	{
		customModeBonusPoints = newCustomModePoints;
	}

	protected override bool CanBeDisplayed()
	{
		return !selectedGlyphs.IsNullOrEmpty();
	}

	protected override void RefreshContent()
	{
		RefreshText();
		RefreshCustomMode();
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(RefreshText));
	}

	private void RefreshText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (selectedGlyphs != null)
		{
			for (int i = 0; i < selectedGlyphs.Count && i < TPSingleton<GlyphManager>.Instance.MaxGlyphsDisplayed; i++)
			{
				stringBuilder.Append("<style=DarkShopKW>• " + selectedGlyphs[i].GetName() + "</style> : " + selectedGlyphs[i].GetDescription(null) + "\r\n");
			}
			if (TPSingleton<GlyphManager>.Instance.MaxGlyphsDisplayed < selectedGlyphs.Count)
			{
				stringBuilder.Append(Localizer.Get("GlyphEffects_MaxGlyphsDisplayed"));
			}
			effectsDescription.text = stringBuilder.ToString();
		}
	}

	private void RefreshCustomMode()
	{
		bool flag = glyphsConfigCustomModeEnabled && customModeBonusPoints > 0;
		customModeIcon.enabled = flag;
		customModeValue.enabled = flag;
		if (flag)
		{
			customModeValue.text = $"+{customModeBonusPoints}";
		}
	}
}
