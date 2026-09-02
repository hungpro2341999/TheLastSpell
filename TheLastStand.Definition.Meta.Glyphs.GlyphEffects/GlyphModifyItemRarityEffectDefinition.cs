using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyItemRarityEffectDefinition : GlyphEffectDefinition
{
	public class ItemRarityModifier
	{
		public int MinRarityIndex { get; set; } = -1;
	}

	public const string Name = "ModifyItemRarity";

	public const string MinRarityName = "MinRarity";

	public string ItemTag { get; private set; }

	public int MinRarityIndex { get; private set; }

	public GlyphModifyItemRarityEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in GlyphModifyItemRarityEffectDefinition.");
		XAttribute xAttribute = obj.Attribute("Tag");
		GlyphDefinition.AssertIsTrue(xAttribute != null, "Tag attribute is missing in ModifyItemRarity");
		ItemTag = xAttribute?.Value;
		foreach (XElement item in obj.Elements("MinRarity"))
		{
			GlyphDefinition.AssertIsTrue(int.TryParse(item.Value, out var result), "Could not parse MinRarity into an int in ModifyItemRarity : " + item.Value);
			MinRarityIndex = result;
		}
	}
}
