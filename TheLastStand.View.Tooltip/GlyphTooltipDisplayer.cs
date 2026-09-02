using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.View.WorldMap.Glyphs;

namespace TheLastStand.View.Tooltip;

public class GlyphTooltipDisplayer : IconTooltipDisplayer
{
	private GlyphDefinition glyphDefinition;

	private GlyphTooltip GlyphTooltip => targetTooltip as GlyphTooltip;

	public override void DisplayTooltip()
	{
		GlyphTooltip.Init(glyphDefinition, !isFromLightShop);
		base.DisplayTooltip();
	}

	public void Init(GlyphDefinition newGlyphDefinition, GlyphTooltip glyphTooltip = null, bool newIsFromLightShop = false)
	{
		glyphDefinition = newGlyphDefinition;
		Init(glyphTooltip, newIsFromLightShop);
	}
}
