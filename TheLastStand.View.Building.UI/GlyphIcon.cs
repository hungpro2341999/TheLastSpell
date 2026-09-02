using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.View.MetaShops;
using TheLastStand.View.Tooltip;
using TheLastStand.View.WorldMap.Glyphs;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class GlyphIcon : OraculumUnlockIcon
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private GlyphTooltipDisplayer GlyphTooltipDisplayer;

	public void Init(GlyphDefinition glyphDefinition, MetaUpgradeLineView containerUpgrade, GlyphTooltip glyphTooltip = null, bool isLightShop = false)
	{
		icon.sprite = AGlyphDisplay.GetGlyphIcon(glyphDefinition);
		GlyphTooltipDisplayer.Init(glyphDefinition, glyphTooltip, isLightShop);
		SetMetaUpgrade(containerUpgrade);
	}

	private void OnDisable()
	{
		if (GlyphTooltipDisplayer.IsDisplayingTargetTooltip)
		{
			GlyphTooltipDisplayer.HideTooltip();
		}
	}
}
