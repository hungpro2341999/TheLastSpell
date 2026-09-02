using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs;

public class AGlyphDisplayWithCost : AGlyphDisplay
{
	private static class Constants
	{
		public const string GlyphCostIconsPath = "View/Sprites/UI/Meta/Glyphs/Costs/Glyphs_Cost_";
	}

	[SerializeField]
	protected Image glyphCost;

	public override void Init(GlyphDefinition glyphDefinition)
	{
		base.Init(glyphDefinition);
		glyphCost.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Meta/Glyphs/Costs/Glyphs_Cost_" + glyphDefinition.Cost, failSilently: true) ?? ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Meta/Glyphs/Costs/Glyphs_Cost_1");
	}
}
