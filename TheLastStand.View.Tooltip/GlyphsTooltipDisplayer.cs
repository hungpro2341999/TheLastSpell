using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Manager.WorldMap;
using UnityEngine;

namespace TheLastStand.View.Tooltip;

public class GlyphsTooltipDisplayer : SpriteChangeTooltipDisplayer
{
	[SerializeField]
	private Sprite customModeSpriteOff;

	[SerializeField]
	private Sprite customModeSpriteOn;

	private List<GlyphDefinition> selectedGlyphs;

	private bool glyphsConfigCustomModeEnabled;

	private int customModeBonusPoints;

	private Sprite initOffSprite;

	private Sprite initOnSprite;

	private GlyphEffectsTooltip GlyphEffectsTooltip => targetTooltip as GlyphEffectsTooltip;

	public void SetSelectedGlyphs(List<GlyphDefinition> newGlyphs)
	{
		selectedGlyphs = newGlyphs;
	}

	public void SetCustomModeEnabled(bool newCustomModeEnabled)
	{
		glyphsConfigCustomModeEnabled = newCustomModeEnabled;
		offSprite = (glyphsConfigCustomModeEnabled ? customModeSpriteOff : initOffSprite);
		onSprite = (glyphsConfigCustomModeEnabled ? customModeSpriteOn : initOnSprite);
		image.sprite = offSprite;
	}

	public void SetCustomModePoints(int newCustomModePoints)
	{
		customModeBonusPoints = newCustomModePoints;
	}

	private void Start()
	{
		initOffSprite = offSprite;
		initOnSprite = onSprite;
		bool flag = TPSingleton<WorldMapCityManager>.Instance.SelectedCity != null;
		SetSelectedGlyphs(flag ? TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs : null);
		SetCustomModeEnabled(flag && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled);
		SetCustomModePoints(flag ? TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GetCustomModeBonusPoints() : 0);
	}

	public override void DisplayTooltip()
	{
		if (selectedGlyphs != null)
		{
			GlyphEffectsTooltip.SetSelectedGlyphs(selectedGlyphs);
			GlyphEffectsTooltip.SetCustomModeEnabled(glyphsConfigCustomModeEnabled);
			GlyphEffectsTooltip.SetCustomModePoints(customModeBonusPoints);
		}
		GlyphEffectsTooltip.FollowElement.ChangeTarget(base.transform);
		base.DisplayTooltip();
	}
}
