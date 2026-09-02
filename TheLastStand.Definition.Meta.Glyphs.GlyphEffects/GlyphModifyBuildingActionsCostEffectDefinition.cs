using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyBuildingActionsCostEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "ModifyBuildingActionsCost";

	public Dictionary<string, int> BuildingActionCostModifiers { get; private set; }

	public int ModifiersDailyLimit { get; private set; }

	public GlyphModifyBuildingActionsCostEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in ModifyBuildingActionsCost");
		XAttribute xAttribute = obj.Attribute("ModifiersDailyLimit");
		GlyphDefinition.AssertIsTrue(xAttribute != null, "ModifiersDailyLimit is missing in ModifyBuildingActionsCost");
		GlyphDefinition.AssertIsTrue(int.TryParse(xAttribute.Value.Replace(base.TokenVariables), out var result), "Could not parse ModifiersDailyLimit into an int in ModifyBuildingActionsCost : " + xAttribute.Value.Replace(base.TokenVariables));
		ModifiersDailyLimit = result;
		BuildingActionCostModifiers = new Dictionary<string, int>();
		foreach (XElement item in obj.Elements("BuildingActionCostModifier"))
		{
			XAttribute xAttribute2 = item.Attribute("BuildingActionId");
			GlyphDefinition.AssertIsTrue(xAttribute2 != null, "BuildingActionId attribute is missing in BuildingActionCostModifier in ModifyBuildingActionsCost");
			XAttribute xAttribute3 = item.Attribute("CostModifier");
			GlyphDefinition.AssertIsTrue(xAttribute3 != null, "CostModifier attribute is missing in BuildingActionCostModifier in ModifyBuildingActionsCost");
			GlyphDefinition.AssertIsTrue(int.TryParse(xAttribute3.Value.Replace(base.TokenVariables), out var result2), "CostModifier could not be parsed into an int in ModifyBuildingActionsCost : " + xAttribute3.Value.Replace(base.TokenVariables));
			BuildingActionCostModifiers.Add(xAttribute2.Value.Replace(base.TokenVariables), result2);
		}
	}
}
