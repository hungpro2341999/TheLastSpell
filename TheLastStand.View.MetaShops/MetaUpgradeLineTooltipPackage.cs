using TheLastStand.View.Building.UI;
using TheLastStand.View.Item;
using TheLastStand.View.WorldMap.Glyphs;

namespace TheLastStand.View.MetaShops;

public class MetaUpgradeLineTooltipPackage
{
	public ItemTooltip ItemTooltip { get; private set; }

	public GlyphTooltip GlyphTooltip { get; private set; }

	public BuildingConstructionTooltip BuildingTooltip { get; private set; }

	public BuildingActionTooltip BuildingActionTooltip { get; private set; }

	public BuildingUpgradeTooltip BuildingUpgradeTooltip { get; private set; }

	public MetaUpgradeLineTooltipPackage(ItemTooltip itemTooltip = null, GlyphTooltip glyphTooltip = null, BuildingConstructionTooltip buildingTooltip = null, BuildingActionTooltip buildingActionTooltip = null, BuildingUpgradeTooltip buildingUpgradeTooltip = null)
	{
		ItemTooltip = itemTooltip;
		GlyphTooltip = glyphTooltip;
		BuildingTooltip = buildingTooltip;
		BuildingActionTooltip = buildingActionTooltip;
		BuildingUpgradeTooltip = buildingUpgradeTooltip;
	}
}
