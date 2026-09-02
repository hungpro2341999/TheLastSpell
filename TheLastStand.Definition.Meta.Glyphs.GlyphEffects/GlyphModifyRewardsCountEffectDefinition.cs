using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyRewardsCountEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "ModifyRewardsCount";

	public int NightRewardsModifier { get; private set; }

	public int ProdRewardsModifier { get; private set; }

	public GlyphModifyRewardsCountEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in ModifyRewardsCount");
		XAttribute xAttribute = obj.Attribute("NightRewardsModifier");
		if (xAttribute != null)
		{
			GlyphDefinition.AssertIsTrue(int.TryParse(xAttribute.Value.Replace(base.TokenVariables), out var result), "Could not parse NightRewardsModifier into an int in ModifyRewardsCount");
			NightRewardsModifier = result;
		}
		XAttribute xAttribute2 = obj.Attribute("ProdRewardsModifier");
		if (xAttribute2 != null)
		{
			GlyphDefinition.AssertIsTrue(int.TryParse(xAttribute2.Value.Replace(base.TokenVariables), out var result2), "Could not parse ProdRewardsModifier into an int in ModifyRewardsCount");
			ProdRewardsModifier = result2;
		}
	}
}
