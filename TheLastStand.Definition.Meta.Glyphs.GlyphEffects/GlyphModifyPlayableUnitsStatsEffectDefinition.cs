using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyPlayableUnitsStatsEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "ModifyPlayableUnitsStats";

	public Dictionary<UnitStatDefinition.E_Stat, Node> StatsToModify { get; private set; }

	public GlyphModifyPlayableUnitsStatsEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in GlyphIncreaseStartingResourcesEffectDefinition.");
		StatsToModify = new Dictionary<UnitStatDefinition.E_Stat, Node>();
		foreach (XElement item in obj.Elements("StatToModify"))
		{
			GlyphDefinition.AssertIsTrue(Enum.TryParse<UnitStatDefinition.E_Stat>(item.Attribute("Stat").Value, out var result), "Could not parse Stat attribute in ModifyPlayableUnitsStats or the attribute is missing.");
			XAttribute xAttribute = item.Attribute("Value");
			GlyphDefinition.AssertIsTrue(xAttribute != null, "The Value attribute in ModifyPlayableUnitsStats is missing.");
			StatsToModify.Add(result, Parser.Parse(xAttribute.Value, base.TokenVariables));
		}
	}
}
