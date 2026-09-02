using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class BuildingModifierMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "BuildingModifier";

	public string BuildingId { get; private set; }

	public int GoldCostReduction { get; private set; }

	public int HealthBonus { get; private set; }

	public int PassiveProductionBonus { get; private set; }

	public sbyte MaxCityInstancesBonus { get; private set; }

	public BuildingModifierMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		BuildingId = xElement.Attribute("Id").Value;
		XElement xElement2 = xElement.Element("GoldCostReduction");
		if (xElement2 != null)
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				Debug.LogError("GoldCostReduction element has an invalid value!");
				return;
			}
			GoldCostReduction = result;
		}
		XElement xElement3 = xElement.Element("MaxCityInstancesBonus");
		if (xElement3 != null)
		{
			if (!sbyte.TryParse(xElement3.Value, out var result2))
			{
				Debug.LogError("MaxCityInstancesBonus element has an invalid value!");
				return;
			}
			MaxCityInstancesBonus = result2;
		}
		XElement xElement4 = xElement.Element("HealthBonus");
		if (xElement4 != null)
		{
			if (!int.TryParse(xElement4.Value, out var result3))
			{
				Debug.LogError("HealthBonus element has an invalid value!");
				return;
			}
			HealthBonus = result3;
		}
		XElement xElement5 = xElement.Element("PassiveProductionBonus");
		if (xElement5 != null)
		{
			if (int.TryParse(xElement5.Value, out var result4))
			{
				PassiveProductionBonus = result4;
			}
			else
			{
				Debug.LogError("PassiveProductionBonus element has an invalid value!");
			}
		}
	}

	public override string ToString()
	{
		return "BuildingModifier (" + BuildingId + ")";
	}
}
