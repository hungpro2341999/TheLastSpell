using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingGaugeEffect;

public class GainGoldDefinition : BuildingGaugeEffectDefinition
{
	public const string Name = "GainGold";

	public Node GoldGain { get; private set; }

	public GainGoldDefinition(XContainer container)
		: base("GainGold", container)
	{
	}

	public override BuildingGaugeEffectDefinition Clone()
	{
		GainGoldDefinition obj = base.Clone() as GainGoldDefinition;
		obj.GoldGain = GoldGain.Clone();
		return obj;
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = (container as XElement).Element("Gold");
		if (xElement.IsNullOrEmpty())
		{
			Debug.LogError("BuildingGaugeEffectDefinition must have Golds");
		}
		else
		{
			GoldGain = Parser.Parse(xElement.Value);
		}
	}
}
