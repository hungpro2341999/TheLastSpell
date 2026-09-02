using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphAddStartingGearGenerationPriorityEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "AddStartingGearGenerationPriority";

	public string ItemListId { get; private set; }

	public GlyphAddStartingGearGenerationPriorityEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in GlyphAddStartingGearGenerationPriorityEffectDefinition.");
		XAttribute xAttribute = obj.Attribute("ItemListId");
		GlyphDefinition.AssertIsTrue(xAttribute != null, "ItemListId attribute is missing in AddStartingGearGenerationPriority.");
		ItemListId = xAttribute.Value.Replace(base.TokenVariables);
	}
}
