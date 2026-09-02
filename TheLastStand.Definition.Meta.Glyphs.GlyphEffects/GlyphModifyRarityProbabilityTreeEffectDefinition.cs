using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyRarityProbabilityTreeEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "ModifyRarityProbabilityTree";

	public string TreeId { get; private set; }

	public Dictionary<int, int> ProbabilityModifiers { get; private set; }

	public GlyphModifyRarityProbabilityTreeEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in ModifyRarityProbabilityTree.");
		XAttribute xAttribute = obj.Attribute("TreeId");
		GlyphDefinition.AssertIsTrue(xAttribute != null, "TreeId attribute is missing in ModifyRarityProbabilityTree.");
		TreeId = xAttribute.Value;
		ProbabilityModifiers = new Dictionary<int, int>();
		foreach (XElement item in obj.Elements("Probability"))
		{
			XAttribute xAttribute2 = item.Attribute("Weight");
			GlyphDefinition.AssertIsTrue(xAttribute2 != null, "Weight attribute is missing in Probability element in ModifyRarityProbabilityTree.");
			GlyphDefinition.AssertIsTrue(int.TryParse(xAttribute2.Value.Replace(base.TokenVariables), out var result), "Could not parse Weight attribute into an int in Probability element in ModifyRarityProbabilityTree.");
			GlyphDefinition.AssertIsTrue(int.TryParse(item.Value.Replace(base.TokenVariables), out var result2), "Could not parse Probability into an int in ModifyRarityProbabilityTree.");
			ProbabilityModifiers.AddValueOrCreateKey(result2, result, (int a, int b) => a + b);
		}
	}
}
