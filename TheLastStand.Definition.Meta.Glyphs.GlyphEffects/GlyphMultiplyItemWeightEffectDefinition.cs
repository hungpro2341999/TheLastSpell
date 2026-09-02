using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphMultiplyItemWeightEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "MultiplyItemWeight";

	public string ItemId { get; private set; }

	public string ItemListId { get; private set; }

	public float WeightMultiplier { get; private set; }

	public bool IsCumulative { get; private set; } = true;

	public GlyphMultiplyItemWeightEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in GlyphIncreaseStartingResourcesEffectDefinition.");
		XAttribute xAttribute = obj.Attribute("ItemListId");
		GlyphDefinition.AssertIsTrue(xAttribute != null, "ItemListId attribute is missing in MultiplyItemWeight.");
		ItemListId = xAttribute.Value.Replace(base.TokenVariables);
		XAttribute xAttribute2 = obj.Attribute("ItemId");
		GlyphDefinition.AssertIsTrue(xAttribute2 != null, "ItemId attribute is missing in MultiplyItemWeight.");
		ItemId = xAttribute2.Value.Replace(base.TokenVariables);
		XAttribute xAttribute3 = obj.Attribute("WeightMultiplier");
		GlyphDefinition.AssertIsTrue(xAttribute3 != null, "WeightMultiplier attribute is missing in MultiplyItemWeight.");
		string text = xAttribute3.Value.Replace(base.TokenVariables);
		GlyphDefinition.AssertIsTrue(float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result), "WeightMultiplier could not be parsed into a float in MultiplyItemWeight : " + text);
		WeightMultiplier = result;
		XAttribute xAttribute4 = obj.Attribute("IsCumulative");
		if (xAttribute4 != null)
		{
			GlyphDefinition.AssertIsTrue(bool.TryParse(xAttribute4.Value, out var result2), "IsCumulative could not be parsed into a bool in MultiplyItemWeight : " + xAttribute4.Value);
			IsCumulative = result2;
		}
	}
}
