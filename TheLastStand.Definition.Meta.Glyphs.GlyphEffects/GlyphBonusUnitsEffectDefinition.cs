using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphBonusUnitsEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "BonusUnits";

	public bool IncreaseUnitsLimit { get; private set; } = true;

	public List<UnitGenerationDefinition> UnitGenerationDefinitions { get; private set; }

	public GlyphBonusUnitsEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		GlyphDefinition.AssertIsTrue(obj != null, "BonusUnits received null in Deserialize.");
		XAttribute xAttribute = obj.Attribute("IncreaseUnitsLimit");
		if (xAttribute != null)
		{
			GlyphDefinition.AssertIsTrue(bool.TryParse(xAttribute.Value, out var result), "Could not parse IncreaseUnitsLimit into a bool in BonusUnits.");
			IncreaseUnitsLimit = result;
		}
		UnitGenerationDefinitions = new List<UnitGenerationDefinition>();
		foreach (XElement item in obj.Elements("Unit"))
		{
			UnitGenerationDefinitions.Add(new UnitGenerationDefinition(item));
		}
	}
}
